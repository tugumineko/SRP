using UnityEngine;

[System.Serializable]
public class DeferredRPSettings
{
    public enum FrameBufferOutputDebug
    {
        Off,
        Albedo,
        Normal,
        Position,
        Metallic,
        Roughness,
        VisibleLightCount,
        Depth
    }

    public enum TileLightCullingAlgorithm
    {
        AABB,
        SideFace
    };

    public const int maxLightCountPerTile = 32;

    public const int tileBlockSize = 16;

    [SerializeField]
    private FrameBufferOutputDebug _frameBufferOutputDebug = FrameBufferOutputDebug.Off;

    public bool lightShaderByComputeShader = true;

    public TileLightCullingAlgorithm tileLightCullingAlgorithm = TileLightCullingAlgorithm.AABB;

    public bool enableDepthSliceForLightCulling = true;
    
    public FrameBufferOutputDebug frameBufferOutputDebug =>  _frameBufferOutputDebug;
}