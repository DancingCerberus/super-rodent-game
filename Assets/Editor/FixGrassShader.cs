using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class FixGrassShader
{
    static FixGrassShader()
    {
        EditorApplication.delayCall += Run;
    }

    static void Run()
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GrassGround.mat");
        if (m == null) return;
        var s = Shader.Find("Universal Render Pipeline/Unlit");
        if (s == null || m.shader == s) return;
        m.shader = s;
        // keep texture
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/grass.png");
        if (tex != null) m.SetTexture("_BaseMap", tex);
        EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
        AssetDatabase.DeleteAsset("Assets/Editor/FixGrassShader.cs");
    }
}
