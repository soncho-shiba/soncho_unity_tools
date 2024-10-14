using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialTextureReplacer : EditorWindow
{
    // 置き換えるフォルダのパス
    public string folderPath = "Assets/Textures";
    // 共通フォルダのパス
    public string fallbackFolderPath = "Assets/CommonTextures";
    public Material materialToEdit;

    [MenuItem("SonchoTools/Material Texture Replacer")]
    public static void ShowWindow()
    {
        GetWindow<MaterialTextureReplacer>("Material Texture Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Material Textures", EditorStyles.boldLabel);

        // フォルダのパスを入力
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);
        fallbackFolderPath = EditorGUILayout.TextField("Fallback Folder Path", fallbackFolderPath);

        // 変更するマテリアルを選択
        materialToEdit = (Material)EditorGUILayout.ObjectField("Material", materialToEdit, typeof(Material), false);

        if (GUILayout.Button("Replace Textures"))
        {
            ReplaceMaterialTextures();
        }
    }

    private void ReplaceMaterialTextures()
    {
        if (materialToEdit == null || string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("Please specify a material and folder path.");
            return;
        }

        // マテリアルのプロパティを取得
        Shader shader = materialToEdit.shader;
        int propertyCount = ShaderUtil.GetPropertyCount(shader);

        for (int i = 0; i < propertyCount; i++)
        {
            // テクスチャプロパティを検索
            if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                Texture texture = materialToEdit.GetTexture(propertyName);

                if (texture != null)
                {
                    string textureName = texture.name;

                    // 最初に指定フォルダで検索
                    string texturePath = FindTextureInFolder(textureName, folderPath);

                    // 指定フォルダに存在しなければ、共通フォルダで検索
                    if (string.IsNullOrEmpty(texturePath))
                    {
                        Debug.LogWarning($"Texture '{textureName}' not found in folder '{folderPath}'. Searching fallback folder...");
                        texturePath = FindTextureInFolder(textureName, fallbackFolderPath);
                    }

                    // 見つかれば置き換え
                    if (!string.IsNullOrEmpty(texturePath))
                    {
                        Texture newTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
                        materialToEdit.SetTexture(propertyName, newTexture);
                        Debug.Log($"Replaced texture: {textureName} with {texturePath}");
                    }
                    else
                    {
                        Debug.LogWarning($"Texture '{textureName}' not found in both folders '{folderPath}' and '{fallbackFolderPath}'.");
                    }
                }
            }
        }

        // マテリアルを保存
        EditorUtility.SetDirty(materialToEdit);
        AssetDatabase.SaveAssets();
    }

    private string FindTextureInFolder(string textureName, string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            if (Path.GetFileNameWithoutExtension(file) == textureName)
            {
                return file;
            }
        }

        return null;
    }
}
