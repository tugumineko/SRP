using System;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer 
{
    private ScriptableRenderContext context;
    private Camera camera;
    private bool useHDR;
    private const String bufferName = "Render Camera";
    private CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };
    private CullingResults cullingResults;
    private Lighting lighting = new Lighting();
    PostFXStack postFXStack = new  PostFXStack();
    
    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId LitShaderTagId = new ShaderTagId("CustomLit");
    
    static int frameBufferId = Shader.PropertyToID("_CameraFrameBuffer");
    
    //---------------------------------------------------------------
    
    private DeferredLightConfigurator _deferredLightConfigurator = new DeferredLightConfigurator();
    
    
    public void Render(ScriptableRenderContext context, Camera camera,bool allowHDR,bool useDynamicBatching,bool useGPUInstancing,bool useLightsPerObject,ShadowSettings shadowSettings ,PostFXSettings postFXSettings,int colorLUTResolution)
    {
        this.context = context;
        this.camera = camera;
        
        PrepareBuffer();
        PrepareForSceneWindow();
        
        if (!Cull(shadowSettings.maxDistance))
        {
            return;
        }
        
        //---------------------------------------------

    }

    void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        if (postFXStack.isActive)
        {
            if (flags > CameraClearFlags.Color)
            {
                flags = CameraClearFlags.Color;
            }
            buffer.GetTemporaryRT(
                frameBufferId,camera.pixelWidth,camera.pixelHeight,
                32,FilterMode.Bilinear,RenderTextureFormat.Default
                );
            buffer.SetRenderTarget(
                frameBufferId,
                RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store
                );
        }
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth,
                                 flags <= CameraClearFlags.Color, 
                                 flags == CameraClearFlags.Color ? 
                                        camera.backgroundColor.linear : Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    bool Cull(float maxShadowDistance)
    {
        ScriptableCullingParameters p;
        if (camera.TryGetCullingParameters(out p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = context.Cull(ref p);
            OnPostCameraCulling(ref cullingResults);
            return true;
        }
        return false;
    }

    void OnPostCameraCulling(ref CullingResults cullingResults)
    {
        var lightData = _deferredLightConfigurator.Prepare(ref cullingResults);
    }
    
    void DrawVisibleGeometry(bool useDynamicBatching,bool useGPUInstancing, bool useLightsPerObject)
    {
        PerObjectData lightsPerObjectFlags =
            useLightsPerObject ? PerObjectData.LightData | PerObjectData.LightIndices : PerObjectData.None;
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId,sortingSettings)
        {
            enableInstancing = useGPUInstancing,
            enableDynamicBatching = useDynamicBatching,
            perObjectData = PerObjectData.ReflectionProbes | 
                            PerObjectData.Lightmaps | PerObjectData.ShadowMask |
                            PerObjectData.LightProbe | PerObjectData.OcclusionProbe |
                            PerObjectData.LightProbeProxyVolume |
                            PerObjectData.OcclusionProbeProxyVolume |
                            lightsPerObjectFlags
        };
        drawingSettings.SetShaderPassName(1, LitShaderTagId);
        var filterSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);

        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filterSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
    }

    void Cleanup()
    {
        lighting.Cleanup();
        if (postFXStack.isActive)
        {
            buffer.ReleaseTemporaryRT(frameBufferId);
        }
    }
}
