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

    private PostFXSettings settings;

    enum Pass
    {
        BloomHorizontal,
        BloomVertical,
        BloomCombine,
        BloomPrefilter,
        Copy,
        DitherBayer,
        ReduceColor
    }
    
    int bloomPrefilterId = Shader.PropertyToID("_BloomPrefilter");
    int bloomThresholdId = Shader.PropertyToID("_BloomThreshold");
    int bloomIntensityId = Shader.PropertyToID("_BloomIntensity");
    int bloomBicubicUpsamplingId = Shader.PropertyToID("_BloomBicubicUpsampling");
    int fxSourceId = Shader.PropertyToID("_PostFXSource");
    int fxSource2Id = Shader.PropertyToID("_PostFXSource2");
    
    int discreteLevelId  =  Shader.PropertyToID("_DiscreteLevel");
    int reduceColorGrayScaleId = Shader.PropertyToID("_ReduceColorGrayScale");
    
    int ditherBayerGrayScaleId = Shader.PropertyToID("_DitherBayerGrayScale");
    
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

    void DoBloom(int sourceId)
    {
        buffer.BeginSample("Bloom");
        PostFXSettings.BloomSettings bloom = settings.Bloom;
        int width = camera.pixelWidth /2 , height = camera.pixelHeight /2 ;
        if (
            bloom.maxIterations == 0 || bloom.intensity <= 0 ||
            height < bloom.downscaleLimit * 2 || width < bloom.downscaleLimit * 2
        )
        {
            Draw(sourceId, BuiltinRenderTextureType.CameraTarget,Pass.Copy);
            buffer.EndSample("Bloom");
            return;
        }

        Vector4 threshold;
        threshold.x = Mathf.GammaToLinearSpace(bloom.threshold);
        threshold.y = threshold.x * bloom.thresholdKnee;
        threshold.z = 2f * threshold.y;
        threshold.w = 0.25f / (threshold.y + 0.00001f);
        threshold.y -= threshold.x;
        buffer.SetGlobalVector(bloomThresholdId, threshold);
        RenderTextureFormat format = RenderTextureFormat.Default;
        buffer.GetTemporaryRT(
            bloomPrefilterId, width, height, 0, FilterMode.Bilinear, format
        );
        Draw(sourceId, bloomPrefilterId, Pass.BloomPrefilter);
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
        buffer.SetGlobalFloat(bloomIntensityId,1f);
        if (i > 1)
        {
            buffer.ReleaseTemporaryRT(fromId - 1);
            toId -= 5;
            for (i -= 1; i > 0; i--)
            {
                buffer.SetGlobalTexture(fxSource2Id, toId + 1);
                Draw(fromId, toId, Pass.BloomCombine);
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
        buffer.SetGlobalFloat(bloomIntensityId, bloom.intensity);
        buffer.SetGlobalTexture(fxSource2Id,sourceId);
        Draw(fromId,BuiltinRenderTextureType.CameraTarget, Pass.BloomCombine);
        buffer.ReleaseTemporaryRT(fromId);
        buffer.EndSample("Bloom");
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
    
    public void Setup(ScriptableRenderContext context, Camera camera, PostFXSettings settings)
    {
        this.context = context;
        this.camera = camera;
        this.settings = camera.cameraType <= CameraType.SceneView ? settings : null;
        ApplySceneViewState();
    }

    public void Render(int sourceId)
    {
        //DoBloom(sourceId);
        //DoReduceColor(sourceId);
        DoDitherBayer(sourceId);
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
