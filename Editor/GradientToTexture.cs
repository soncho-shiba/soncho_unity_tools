using UnityEngine;
using UnityEditor;
using System.IO;

public class GradientToTexture : EditorWindow
{
    Gradient gradient = new Gradient();  // Gradient ��������
    int textureWidth = 256;
    int textureHeight = 16;

    // �o�̓t�H�[�}�b�g�I��p�̗񋓌^
    enum FileFormat { PNG, TGA }
    FileFormat selectedFormat = FileFormat.PNG;  // �f�t�H���g��PNG

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

        // �o�̓t�H�[�}�b�g�̑I��
        selectedFormat = (FileFormat)EditorGUILayout.EnumPopup("File Format", selectedFormat);

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

        // �o�͌`���ɉ������t�@�C�����Ɗg���q��ݒ�
        string extension = selectedFormat == FileFormat.PNG ? "png" : "tga";
        string path = EditorUtility.SaveFilePanel($"Save Gradient Texture as {extension.ToUpper()}", "", $"GradientTexture.{extension}", extension);

        if (!string.IsNullOrEmpty(path))
        {
            // PNG��TGA�̑I���ɉ����ăG���R�[�h
            byte[] bytes;
            if (selectedFormat == FileFormat.PNG)
            {
                bytes = texture.EncodeToPNG();
            }
            else
            {
                bytes = texture.EncodeToTGA();
            }

            File.WriteAllBytes(path, bytes);
            Debug.Log($"Texture saved at: {path}");
        }

        // �e�N�X�`���̃C���|�[�g�ݒ�
        AssetDatabase.Refresh();
    }
}
