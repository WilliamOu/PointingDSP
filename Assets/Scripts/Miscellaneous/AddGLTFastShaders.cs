using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AddGLTFastShaders : MonoBehaviour
{
    [SerializeField] private ShaderVariantCollection shaderVariantCollection;

    private void Awake()
    {
        AddShaders();
    }

    private void AddShaders()
    {
        if (shaderVariantCollection == null)
        {
            Debug.LogError("Shader Variant Collection is not assigned.");
            return;
        }

        Shader pbrMetallicRoughness = Shader.Find("glTF/PbrMetallicRoughness");
        Shader pbrSpecularGlossiness = Shader.Find("glTF/PbrSpecularGlossiness");
        Shader unlit = Shader.Find("glTF/Unlit");

        if (pbrMetallicRoughness != null && pbrSpecularGlossiness != null && unlit != null)
        {
            AddShaderVariants(pbrMetallicRoughness);
            AddShaderVariants(pbrSpecularGlossiness);
            // AddShaderVariants(unlit);

            Debug.Log("Shaders added to the Shader Variant Collection.");
        }
        else
        {
            Debug.LogError("Shaders not found.");
        }
    }

    private void AddShaderVariants(Shader shader)
    {
        AddShaderVariant(shader, PassType.ForwardBase);
        AddShaderVariant(shader, PassType.ForwardAdd);
        AddShaderVariant(shader, PassType.ShadowCaster);
        AddShaderVariant(shader, PassType.Deferred);
        AddShaderVariant(shader, PassType.Meta);
    }

    private void AddShaderVariant(Shader shader, PassType passType)
    {
        try
        {
            shaderVariantCollection.Add(new ShaderVariantCollection.ShaderVariant(shader, passType));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to add shader variant: {shader.name} with pass type: {passType}. Exception: {e.Message}");
        }
    }
}
