using UnityEngine;
using UnityEditor;
using System.IO;

public class GradientToTexture : EditorWindow
{
    Gradient gradient = new Gradient();  // Gradient ��������
    int textureWidth = 256;
    int textureHeight = 16;

    [MenuItem("SonchoTools/Gradient to Texture")]
    public static void ShowWindow()
    {
        GetWindow<GradientToTexture>("Gradient to Texture");
    }

    private void OnGUI()
    {
        GUILayout.Label("Gradient to Texture", EditorStyles.boldLabel);

        // Gradient �t�B�[���h�̏��������m�F
        if (gradient == null)
        {
            gradient = new Gradient();
        }

        // Gradient �t�B�[���h
        gradient = EditorGUILayout.GradientField("Gradient", gradient);

        // �e�N�X�`���̃T�C�Y�ݒ�
        textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth);
        textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight);

        if (GUILayout.Button("Generate and Save Texture"))
        {
            SaveGradientTexture();
        }
    }

    private void SaveGradientTexture()
    {
        // �e�N�X�`������
        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        for (int x = 0; x < textureWidth; x++)
        {
            Color color = gradient.Evaluate((float)x / (textureWidth - 1));
            for (int y = 0; y < textureHeight; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();

        // �摜�̕ۑ�
        byte[] bytes = texture.EncodeToPNG();
        string path = EditorUtility.SaveFilePanel("Save Gradient Texture", "", "GradientTexture.png", "png");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            Debug.Log("Texture saved at: " + path);
        }

        // �e�N�X�`���̃C���|�[�g�ݒ�
        AssetDatabase.Refresh();
    }
}
