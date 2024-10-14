using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialTextureReplacer : EditorWindow
{
    // 共通フォルダのパス
    public string fallbackFolderPath = "Assets/CommonTextures";
    public Material[] materialsToEdit;

    [MenuItem("SonchoTools/Material Texture Replacer")]
    public static void ShowWindow()
    {
        GetWindow<MaterialTextureReplacer>("Material Texture Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Material Textures", EditorStyles.boldLabel);

        // 共通フォルダのパスを入力
        fallbackFolderPath = EditorGUILayout.TextField("Fallback Folder Path", fallbackFolderPath);

        // 変更するマテリアルを複数選択
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty materialsProperty = serializedObject.FindProperty("materialsToEdit");
        EditorGUILayout.PropertyField(materialsProperty, true);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Replace Textures"))
        {
            ReplaceMaterialTextures();
        }
    }

    private void ReplaceMaterialTextures()
    {
        if (materialsToEdit == null || materialsToEdit.Length == 0)
        {
            Debug.LogError("Please specify at least one material.");
            return;
        }

        foreach (var material in materialsToEdit)
        {
            if (material == null)
            {
                continue;
            }

            Debug.Log($"Processing material: {material.name}");

            // マテリアルのパスからTextureフォルダを取得
            string materialPath = AssetDatabase.GetAssetPath(material);
            string textureFolderPath = GetTextureFolderPath(materialPath);

            if (string.IsNullOrEmpty(textureFolderPath))
            {
                Debug.LogWarning($"Texture folder not found for material {material.name}.");
                continue;
            }

            // マテリアルのプロパティを取得
            Shader shader = material.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);

            for (int i = 0; i < propertyCount; i++)
            {
                // テクスチャプロパティを検索
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = material.GetTexture(propertyName);

                    if (texture != null)
                    {
                        string textureName = texture.name;

                        // Materialと同階層のTextureフォルダから検索
                        string texturePath = FindTextureInFolder(textureName, textureFolderPath);

                        // 指定フォルダに存在しなければ、共通フォルダで検索
                        if (string.IsNullOrEmpty(texturePath))
                        {
                            Debug.LogWarning($"Texture '{textureName}' not found in folder '{textureFolderPath}'. Searching fallback folder...");
                            texturePath = FindTextureInFolder(textureName, fallbackFolderPath);
                        }

                        // 見つかれば置き換え
                        if (!string.IsNullOrEmpty(texturePath))
                        {
                            Texture newTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
                            material.SetTexture(propertyName, newTexture);
                            Debug.Log($"Replaced texture: {textureName} with {texturePath} in material {material.name}");
                        }
                        else
                        {
                            Debug.LogWarning($"Texture '{textureName}' not found in both folders '{textureFolderPath}' and '{fallbackFolderPath}' for material {material.name}.");
                        }
                    }
                }
            }

            // マテリアルを保存
            EditorUtility.SetDirty(material);
        }

        AssetDatabase.SaveAssets();
    }

    // マテリアルのパスからTextureフォルダを取得
    private string GetTextureFolderPath(string materialPath)
    {
        // マテリアルのフォルダの親フォルダを取得
        string materialFolderPath = Path.GetDirectoryName(materialPath);

        // 同じ階層に"Texture"という名前を含むフォルダを探す
        string parentFolder = Path.GetDirectoryName(materialFolderPath);
        string[] directories = Directory.GetDirectories(parentFolder, "*Texture*", SearchOption.TopDirectoryOnly);

        // 見つかった最初のTextureフォルダを返す
        if (directories.Length > 0)
        {
            return directories[0];
        }

        return null;
    }

    // 指定フォルダからテクスチャを探す
    private string FindTextureInFolder(string textureName, string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            return null;
        }

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
