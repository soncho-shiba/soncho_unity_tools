using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialTextureReplacer : EditorWindow
{
    // �u��������t�H���_�̃p�X
    public string folderPath = "Assets/Textures";
    // ���ʃt�H���_�̃p�X
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

        // �t�H���_�̃p�X�����
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);
        fallbackFolderPath = EditorGUILayout.TextField("Fallback Folder Path", fallbackFolderPath);

        // �ύX����}�e���A����I��
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

        // �}�e���A���̃v���p�e�B���擾
        Shader shader = materialToEdit.shader;
        int propertyCount = ShaderUtil.GetPropertyCount(shader);

        for (int i = 0; i < propertyCount; i++)
        {
            // �e�N�X�`���v���p�e�B������
            if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                Texture texture = materialToEdit.GetTexture(propertyName);

                if (texture != null)
                {
                    string textureName = texture.name;

                    // �ŏ��Ɏw��t�H���_�Ō���
                    string texturePath = FindTextureInFolder(textureName, folderPath);

                    // �w��t�H���_�ɑ��݂��Ȃ���΁A���ʃt�H���_�Ō���
                    if (string.IsNullOrEmpty(texturePath))
                    {
                        Debug.LogWarning($"Texture '{textureName}' not found in folder '{folderPath}'. Searching fallback folder...");
                        texturePath = FindTextureInFolder(textureName, fallbackFolderPath);
                    }

                    // ������Βu������
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

        // �}�e���A����ۑ�
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
