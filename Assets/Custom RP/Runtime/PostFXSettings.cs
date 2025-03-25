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

    // Bloom Start-----------------------------
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
    
    //Bloom End--------------------------------
    
    //ReduceColor Start---------------------------------

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
    
    //ReduceColor End-----------------------------------
    
}
