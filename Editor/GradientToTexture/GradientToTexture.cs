using UnityEngine;
using UnityEditor;
using System.IO;

public class GradientToTexture : EditorWindow
{
    Gradient gradient = new Gradient();  // Gradient �̏�����
    GradientPreset loadedPreset;  // �ǂݍ��񂾃v���Z�b�g
    int textureWidth = 256;
    int textureHeight = 16;
    string lastSavedPath = "";  // �Ō�ɕۑ������p�X��ێ�

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

        // Gradient �t�B�[���h
        gradient = EditorGUILayout.GradientField("Gradient", gradient);

        // �v���Z�b�g�̕ۑ��Ɠǂݍ���
        DrawPresetButtons();

        // �e�N�X�`���̃T�C�Y�ݒ�
        textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth);
        textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight);

        // �o�̓t�H�[�}�b�g�̑I��
        selectedFormat = (FileFormat)EditorGUILayout.EnumPopup("File Format", selectedFormat);

        // �ۑ��{�^��
        if (GUILayout.Button("Generate and Save Texture"))
        {
            SaveGradientTexture();
        }

        // �{�^���ԂɃX�y�[�X��ǉ�
        GUILayout.Space(10);  // �����ŃX�y�[�X��ǉ�

        // �����|�`�ۑ��{�^���i�O��ۑ������p�X�ɕۑ��j
        GUI.enabled = !string.IsNullOrEmpty(lastSavedPath);
        if (GUILayout.Button("Save to Last Path"))
        {
            SaveGradientTextureToLastPath();
        }
        GUI.enabled = true;
    }

    private void DrawPresetButtons()
    {
        EditorGUILayout.Space();
        if (GUILayout.Button("Save as Preset"))
        {
            SaveGradientPreset();
        }

        loadedPreset = (GradientPreset)EditorGUILayout.ObjectField("Load Preset", loadedPreset, typeof(GradientPreset), false);
        if (loadedPreset != null && GUILayout.Button("Apply Preset"))
        {
            ApplyGradientPreset();
        }
    }

    // �e�N�X�`���𐶐����ۑ����郁�\�b�h
    private void SaveGradientTexture()
    {
        // �t�@�C���p�X�̐ݒ�
        string extension = selectedFormat == FileFormat.PNG ? "png" : "tga";
        string path = EditorUtility.SaveFilePanel($"Save Gradient Texture as {extension.ToUpper()}", "", $"GradientTexture.{extension}", extension);

        if (!string.IsNullOrEmpty(path))
        {
            lastSavedPath = path;  // �Ō�ɕۑ������p�X���L�^
            SaveTextureToPath(path);
        }
    }

    // �O��ۑ������p�X�Ƀe�N�X�`����ۑ����郁�\�b�h
    private void SaveGradientTextureToLastPath()
    {
        if (!string.IsNullOrEmpty(lastSavedPath))
        {
            SaveTextureToPath(lastSavedPath);
        }
        else
        {
            Debug.LogWarning("No previous save path found.");
        }
    }

    // �e�N�X�`�����w�肵���p�X�ɕۑ����鋤�ʃ��\�b�h
    private void SaveTextureToPath(string path)
    {
        Texture2D texture = GenerateGradientTexture();

        // �I�������`���Ɋ�Â��ăe�N�X�`����ۑ�
        byte[] bytes = (selectedFormat == FileFormat.PNG) ? texture.EncodeToPNG() : texture.EncodeToTGA();
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Texture saved at: {path}");

        AssetDatabase.Refresh();
    }

    // �e�N�X�`���𐶐����郁�\�b�h
    private Texture2D GenerateGradientTexture()
    {
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
        return texture;
    }

    // �v���Z�b�g��ۑ����郁�\�b�h
    private void SaveGradientPreset()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Gradient Preset", "GradientPreset", "asset", "Please enter a file name to save the gradient preset.");
        if (string.IsNullOrEmpty(path)) return;

        GradientPreset preset = ScriptableObject.CreateInstance<GradientPreset>();
        preset.gradient = gradient;
        AssetDatabase.CreateAsset(preset, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Gradient Preset saved at: {path}");
    }

    // �v���Z�b�g��K�p���郁�\�b�h
    private void ApplyGradientPreset()
    {
        if (loadedPreset != null)
        {
            // �v���Z�b�g����O���f�[�V�������擾���ēK�p
            gradient = loadedPreset.gradient;

            // GradientEditor�ɔ��f���邽�߂�Repaint���Ăяo��
            Repaint();
            Debug.Log("Gradient preset applied.");
        }
    }
}
