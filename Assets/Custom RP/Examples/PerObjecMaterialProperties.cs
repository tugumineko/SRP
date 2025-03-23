using UnityEngine;

[DisallowMultipleComponent]
public class PerObjecMaterialProperties : MonoBehaviour
{
    static int baseColorId =  Shader.PropertyToID("_BaseColor");
    static int cutoffId =  Shader.PropertyToID("_Cutoff");
    static int metallicId  =  Shader.PropertyToID("_Metallic");
    static int occlusionId =  Shader.PropertyToID("_Occlusion");
    static int smoothnessId =  Shader.PropertyToID("_Smoothness");
    static int fresnelId =  Shader.PropertyToID("_Fresnel");
    static int emissionColorId =  Shader.PropertyToID("_EmissionColor");
    static int detailAlbedoId =  Shader.PropertyToID("_DetailAlbedo");
    static int detailSmoothnessId =  Shader.PropertyToID("_DetailSmoothness");
    private static MaterialPropertyBlock block;
    
    [SerializeField]
    Color baseColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    float cutoff = 0.5f;

    [SerializeField, Range(0f, 1f)]
    float metallic = 0.0f;
    
    [SerializeField, Range(0f, 1f)]
    float occlusion = 1.0f;
    
    [SerializeField, Range(0f, 1f)]
    float smoothness = 1.0f;

    [SerializeField, Range(0f, 1f)]
    float fresnel = 1f;
    
    [SerializeField, Range(0f, 1f)]
    float detailAlbedo = 1.0f;

    [SerializeField, Range(0f, 1f)]
    private float detailSmoothness = 1.0f;
    
    [SerializeField, ColorUsage(false, true)]
    private Color emissionColor = Color.black;

    
    void Awake()
    {
        OnValidate();
    }
    
    void OnValidate()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetColor(baseColorId, baseColor);
        block.SetFloat(cutoffId, cutoff);
        block.SetFloat(metallicId, metallic);
        block.SetFloat(occlusionId, occlusion);
        block.SetFloat(smoothnessId, smoothness);
        block.SetFloat(fresnelId, fresnel);
        block.SetColor(emissionColorId, emissionColor);
        block.SetFloat(detailAlbedoId, detailAlbedo);
        block.SetFloat(detailSmoothnessId, detailSmoothness);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }
}
