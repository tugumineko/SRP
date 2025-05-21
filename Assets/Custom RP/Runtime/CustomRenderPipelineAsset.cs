using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public enum RenderPath
{
    Forward,
    Deferred
};

[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField] public bool
        allowHDR = true,
        useDynamicBatching = true,
        useGPUInstancing = true,
        useSRPBatcher = true,
        useLightsPerObject = true;
    
    [SerializeField]
    private ShadowSettings _shadowSettings = default;
    
    [SerializeField] 
    private PostFXSettings _postFXSettings = default;
    
    public enum ColorLUTResolution { _16 = 16, _32 = 32, _64 = 64 }
    
    [SerializeField]
    private ColorLUTResolution _colorLUTResolution = ColorLUTResolution._32;
    
    [SerializeField]
    private RenderPath _renderPath = RenderPath.Forward;
    
    [SerializeField]
    private DeferredRPSettings _deferredSettings = new DeferredRPSettings();
    
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(this);
    }
    
    public ShadowSettings shadowSettings => _shadowSettings;
    public PostFXSettings postFXSettings => _postFXSettings;
    public ColorLUTResolution colorLUTResolution => _colorLUTResolution;
    public RenderPath renderPath => _renderPath;
    public DeferredRPSettings deferredSettings => _deferredSettings;
    
}

