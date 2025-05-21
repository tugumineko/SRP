using UnityEngine;
using UnityEngine.Rendering;

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
    public ShadowSettings shadowSettings = default;
    
    [SerializeField] 
    public PostFXSettings postFXSettings = default;
    
    public enum ColorLUTResolution { _16 = 16, _32 = 32, _64 = 64 }
    
    [SerializeField]
    public ColorLUTResolution colorLUTResolution = ColorLUTResolution._32;
    
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(this);
    }
    
}

