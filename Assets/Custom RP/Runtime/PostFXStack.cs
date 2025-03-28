using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

public partial class PostFXStack
{
    private const string bufferName = "Post FX";

    private CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    private ScriptableRenderContext context;
    
    private Camera camera;

    private bool useHDR;
    
    private PostFXSettings settings;

    enum Pass
    {
        BloomHorizontal,
        BloomVertical,
        BloomAdd,
        BloomScatter,
        BloomScatterFinal,
        BloomPrefilter,
        BloomPrefilterFireflies,
        Copy,
        ReduceColor,
        DitherBayer,
        HalftonePattern,
        ToneMappingACES,
        ToneMappingNeutral,
        ToneMappingReinhard
    }
    
    int postFXResultId = Shader.PropertyToID("_PostFXResult");
    
    int fxSourceId = Shader.PropertyToID("_PostFXSource");
    int fxSource2Id = Shader.PropertyToID("_PostFXSource2");
    
    int bloomPrefilterId = Shader.PropertyToID("_BloomPrefilter");
    int bloomThresholdId = Shader.PropertyToID("_BloomThreshold");
    int bloomIntensityId = Shader.PropertyToID("_BloomIntensity");
    int bloomBicubicUpsamplingId = Shader.PropertyToID("_BloomBicubicUpsampling");
    
    int discreteLevelId  =  Shader.PropertyToID("_DiscreteLevel");
    int reduceColorGrayScaleId = Shader.PropertyToID("_ReduceColorGrayScale");
    
    int ditherBayerGrayScaleId = Shader.PropertyToID("_DitherBayerGrayScale");
    
    int halftoneTileSizeInverseId = Shader.PropertyToID("_HalftoneTileSizeInverse");
    
    private static string[] ditherBayerKeywords =
    {
        "_BAYER4",
        "_BAYER8"
    };
    
    private const int maxBloomPyramidLevels = 16;

    private int bloomPyramidId;
    
    public bool isActive => settings != null;

    public PostFXStack()
    {
        bloomPyramidId = Shader.PropertyToID("_BloomPyramid0");
        for (int i = 0; i < maxBloomPyramidLevels * 2; i++)
        {
            Shader.PropertyToID("_BloomPyramid" + i);
        }
    }

    bool DoBloom(int sourceId)
    {
        PostFXSettings.BloomSettings bloom = settings.Bloom;
        int width = camera.pixelWidth /2 , height = camera.pixelHeight /2 ;
        if (
            bloom.maxIterations == 0 || bloom.intensity <= 0 ||
            height < bloom.downscaleLimit * 2 || width < bloom.downscaleLimit * 2
        )
        {
            return false;
        }

        buffer.BeginSample("Bloom");
        Vector4 threshold;
        threshold.x = Mathf.GammaToLinearSpace(bloom.threshold);
        threshold.y = threshold.x * bloom.thresholdKnee;
        threshold.z = 2f * threshold.y;
        threshold.w = 0.25f / (threshold.y + 0.00001f);
        threshold.y -= threshold.x;
        buffer.SetGlobalVector(bloomThresholdId, threshold);
        RenderTextureFormat format = useHDR ? RenderTextureFormat.DefaultHDR :  RenderTextureFormat.Default;
        buffer.GetTemporaryRT(
            bloomPrefilterId, width, height, 0, FilterMode.Bilinear, format
        );
        Draw(sourceId, bloomPrefilterId, bloom.fadeFireflies ? Pass.BloomPrefilterFireflies : Pass.BloomPrefilter);
        width /= 2;
        height /= 2;
        int fromId = bloomPrefilterId, toId = bloomPyramidId + 1;
        int i;
        for (i = 0; i < bloom.maxIterations; i++)
        {
            if (height < bloom.downscaleLimit || width < bloom.downscaleLimit)
            {
                break;
            }
            int midId = toId - 1;
            buffer.GetTemporaryRT(
                midId,width,height,0,FilterMode.Bilinear,format
                );
            buffer.GetTemporaryRT(
                toId,width,height,0,FilterMode.Bilinear,format
                );
            Draw(fromId,midId,Pass.BloomHorizontal);
            Draw(midId,toId,Pass.BloomVertical);
            fromId = toId;
            toId += 2;
            width /= 2;
            height /= 2;
        }
        buffer.ReleaseTemporaryRT(bloomPrefilterId);
        buffer.SetGlobalFloat(bloomBicubicUpsamplingId,bloom.bicubicUpsampling ? 1f  : 0f);
        Pass combinePass;
        Pass finalPass;
        float finalIntensity;
        if (bloom.mode == PostFXSettings.BloomSettings.Mode.Additive)
        {
            combinePass = finalPass = Pass.BloomAdd;
            buffer.SetGlobalFloat(bloomIntensityId,1f);
            finalIntensity = bloom.intensity;
        }
        else
        {
            combinePass = Pass.BloomScatter;
            finalPass = Pass.BloomScatterFinal;
            buffer.SetGlobalFloat(bloomIntensityId,bloom.scatter);
            finalIntensity = Mathf.Min(bloom.intensity, 0.95f);
        }
        if (i > 1)
        {
            buffer.ReleaseTemporaryRT(fromId - 1);
            toId -= 5;
            for (i -= 1; i > 0; i--)
            {
                buffer.SetGlobalTexture(fxSource2Id, toId + 1);
                Draw(fromId, toId, combinePass);
                buffer.ReleaseTemporaryRT(fromId);
                buffer.ReleaseTemporaryRT(toId + 1);
                fromId = toId;
                toId -= 2;
            }
        }
        else
        {
            buffer.ReleaseTemporaryRT(bloomPyramidId);
        }
        buffer.SetGlobalFloat(bloomIntensityId, finalIntensity);
        buffer.SetGlobalTexture(fxSource2Id,sourceId);
        buffer.GetTemporaryRT(
            postFXResultId, camera.pixelWidth, camera.pixelHeight, 0,
            FilterMode.Bilinear, format
        );
        Draw(fromId,postFXResultId, finalPass);
        buffer.ReleaseTemporaryRT(fromId);
        buffer.EndSample("Bloom");
        return true;
    }

    void DoToneMapping(int sourceId)
    {
        PostFXSettings.ToneMappingSettings.Mode mode = settings.ToneMapping.mode;
        Pass pass = mode < 0 ? Pass.Copy : Pass.ToneMappingACES + (int)mode;
        Draw(sourceId, BuiltinRenderTextureType.CameraTarget, pass);
    }
    
    void DoReduceColor(int sourceId)
    {
        buffer.BeginSample("ReduceColor");
        PostFXSettings.ReduceColorSettings reduceColor = settings.ReduceColor;
        buffer.SetGlobalInt(discreteLevelId,reduceColor.discreteLevel);
        buffer.SetGlobalFloat(reduceColorGrayScaleId,reduceColor.grayScale);
        Draw(sourceId, BuiltinRenderTextureType.CameraTarget, Pass.ReduceColor);
        buffer.EndSample("ReduceColor");
    }
    
    void DoDitherBayer(int sourceId)
    {
        buffer.BeginSample("DitherBayer");
        PostFXSettings.DitherBayerSettings ditherBayerSettings = settings.DitherBayer;
        buffer.SetGlobalFloat(ditherBayerGrayScaleId,ditherBayerSettings.grayScale);
        SetKeywords(ditherBayerKeywords,(int)ditherBayerSettings.ditherMode - 1);
        Draw(sourceId, BuiltinRenderTextureType.CameraTarget, Pass.DitherBayer);
        buffer.EndSample("DitherBayer");
    }

    void DoHalftone(int sourceId)
    {
        RenderTextureFormat format = useHDR ? RenderTextureFormat.DefaultHDR :  RenderTextureFormat.Default;
        buffer.BeginSample("Halftone");
        PostFXSettings.HalftoneSettings halftoneSettings = settings.Halftone;
        buffer.SetGlobalFloat(halftoneTileSizeInverseId, 1 / (float)halftoneSettings.tileSize);
        buffer.GetTemporaryRT(
            postFXResultId, camera.pixelWidth, camera.pixelHeight, 0,
            FilterMode.Bilinear, format
        );
        Draw(sourceId,postFXResultId, Pass.HalftonePattern);
        buffer.EndSample("Halftone");
    }
    
    public void Setup(ScriptableRenderContext context, Camera camera, PostFXSettings settings, bool useHDR)
    {
        this.context = context;
        this.camera = camera;
        this.settings = camera.cameraType <= CameraType.SceneView ? settings : null;
        this.useHDR = useHDR;
        ApplySceneViewState();
    }

    public void Render(int sourceId)
    {
        if (settings.PostFX.type == PostFXSettings.PostFXType.None)
        {
            buffer.GetTemporaryRT(
                postFXResultId, camera.pixelWidth, camera.pixelHeight, 0,
                FilterMode.Bilinear, useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default
            );
            Draw(sourceId,postFXResultId, Pass.Copy);
            DoToneMapping(postFXResultId);
            buffer.ReleaseTemporaryRT(postFXResultId);
        }
        else if (settings.PostFX.type == PostFXSettings.PostFXType.Bloom)
        {
            if (DoBloom(sourceId))
            {
                DoToneMapping(postFXResultId);
                buffer.ReleaseTemporaryRT(postFXResultId);
            }
        }
        else if (settings.PostFX.type == PostFXSettings.PostFXType.ReduceColor)
        {
            DoReduceColor(sourceId);
        }
        else if (settings.PostFX.type == PostFXSettings.PostFXType.DitherBayer)
        {
            DoDitherBayer(sourceId);
        }
        else if (settings.PostFX.type == PostFXSettings.PostFXType.Halftone)
        {
            DoHalftone(sourceId);
            DoToneMapping(postFXResultId);
            buffer.ReleaseTemporaryRT(postFXResultId);
        }
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void Draw(RenderTargetIdentifier from, RenderTargetIdentifier to, Pass pass)
    {
        buffer.SetGlobalTexture(fxSourceId, from);
        buffer.SetRenderTarget(to,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        buffer.DrawProcedural(
            Matrix4x4.identity, settings.Material,(int)pass,
            MeshTopology.Triangles, 3
            );
    }
    
    void SetKeywords(string[] keywords, int enabledIndex)
    {
        for (int i = 0; i < keywords.Length; i++)
        {
            if (i == enabledIndex)
            {
                buffer.EnableShaderKeyword(keywords[i]);
            }
            else
            {
                buffer.DisableShaderKeyword(keywords[i]);
            }
        }
    }
    
}
