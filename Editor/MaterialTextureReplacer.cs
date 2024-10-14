using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialTextureReplacer : EditorWindow
{
    // ���ʃt�H���_�̃p�X
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

        // ���ʃt�H���_�̃p�X�����
        fallbackFolderPath = EditorGUILayout.TextField("Fallback Folder Path", fallbackFolderPath);

        // �ύX����}�e���A���𕡐��I��
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

            // �}�e���A���̃p�X����Texture�t�H���_���擾
            string materialPath = AssetDatabase.GetAssetPath(material);
            string textureFolderPath = GetTextureFolderPath(materialPath);

            if (string.IsNullOrEmpty(textureFolderPath))
            {
                Debug.LogWarning($"Texture folder not found for material {material.name}.");
                continue;
            }

            // �}�e���A���̃v���p�e�B���擾
            Shader shader = material.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);

            for (int i = 0; i < propertyCount; i++)
            {
                // �e�N�X�`���v���p�e�B������
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = material.GetTexture(propertyName);

                    if (texture != null)
                    {
                        string textureName = texture.name;

                        // Material�Ɠ��K�w��Texture�t�H���_���猟��
                        string texturePath = FindTextureInFolder(textureName, textureFolderPath);

                        // �w��t�H���_�ɑ��݂��Ȃ���΁A���ʃt�H���_�Ō���
                        if (string.IsNullOrEmpty(texturePath))
                        {
                            Debug.LogWarning($"Texture '{textureName}' not found in folder '{textureFolderPath}'. Searching fallback folder...");
                            texturePath = FindTextureInFolder(textureName, fallbackFolderPath);
                        }

                        // ������Βu������
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

            // �}�e���A����ۑ�
            EditorUtility.SetDirty(material);
        }

        AssetDatabase.SaveAssets();
    }

    // �}�e���A���̃p�X����Texture�t�H���_���擾
    private string GetTextureFolderPath(string materialPath)
    {
        // �}�e���A���̃t�H���_�̐e�t�H���_���擾
        string materialFolderPath = Path.GetDirectoryName(materialPath);

        // �����K�w��"Texture"�Ƃ������O���܂ރt�H���_��T��
        string parentFolder = Path.GetDirectoryName(materialFolderPath);
        string[] directories = Directory.GetDirectories(parentFolder, "*Texture*", SearchOption.TopDirectoryOnly);

        // ���������ŏ���Texture�t�H���_��Ԃ�
        if (directories.Length > 0)
        {
            return directories[0];
        }

        return null;
    }

    // �w��t�H���_����e�N�X�`����T��
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
