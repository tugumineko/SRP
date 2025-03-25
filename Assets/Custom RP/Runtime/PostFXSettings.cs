using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/Custom Post FX Settings")]
public class PostFXSettings :ScriptableObject
{
    [SerializeField]
    Shader shader = default;
    
    
    [System.NonSerialized]
    private Material material;
    
    public Material Material
    {
        get
        {
            if (material == null && shader != null)
            {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideAndDontSave;
            } 
            return material;
        }
    }

    //----------------------------------------
    
    [System.Serializable]
    public struct BloomSettings
    {
        [Range(0f, 16f)] 
        public int maxIterations;

        [Min(1f)]
        public int downscaleLimit;

        public bool bicubicUpsampling;

        [Min(0f)]
        public float threshold;

        [Range(0f, 1f)] 
        public float thresholdKnee;

        [Min(0f)]
        public float intensity;

    }

    [SerializeField]
    BloomSettings bloom = default;

    public BloomSettings Bloom => bloom;
    
    //-----------------------------------------
    
    [System.Serializable]
    public struct ReduceColorSettings
    {
        [Range(1f, 256f)]
        public int discreteLevel;

        [Range(0f, 1f)]
        public float grayScale;
    }
    
    [SerializeField]
    ReduceColorSettings reduceColor = default;
    
    public ReduceColorSettings ReduceColor => reduceColor;
    
    //-------------------------------------------------------

    public enum DitherMode
    {
        Bayer2x2,
        Bayer4x4,
        Bayer8x8
    };
    
    [System.Serializable]
    public struct DitherBayerSettings
    {
        [Range(0f, 1f)]
        public float grayScale;
        
        public DitherMode ditherMode;
    }

    [SerializeField]
    private DitherBayerSettings ditherBayer = new DitherBayerSettings
    {
        ditherMode = DitherMode.Bayer2x2
    };
    
    public  DitherBayerSettings DitherBayer => ditherBayer;

}
