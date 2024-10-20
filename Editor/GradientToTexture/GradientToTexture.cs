using UnityEngine;
using UnityEditor;
using System.IO;

public class GradientToTexture : EditorWindow
{
    Gradient gradient = new Gradient();  // Gradient の初期化
    GradientPreset loadedPreset;  // 読み込んだプリセット
    int textureWidth = 256;
    int textureHeight = 16;
    string lastSavedPath = "";  // 最後に保存したパスを保持

    // 出力フォーマット選択用の列挙型
    enum FileFormat { PNG, TGA }
    FileFormat selectedFormat = FileFormat.PNG;  // デフォルトはPNG

    [MenuItem("SonchoTools/Gradient to Texture")]
    public static void ShowWindow()
    {
        GetWindow<GradientToTexture>("Gradient to Texture");
    }

    private void OnGUI()
    {
        GUILayout.Label("Gradient to Texture", EditorStyles.boldLabel);

        // Gradient フィールド
        gradient = EditorGUILayout.GradientField("Gradient", gradient);

        // プリセットの保存と読み込み
        DrawPresetButtons();

        // テクスチャのサイズ設定
        textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth);
        textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight);

        // 出力フォーマットの選択
        selectedFormat = (FileFormat)EditorGUILayout.EnumPopup("File Format", selectedFormat);

        // 保存ボタン
        if (GUILayout.Button("Generate and Save Texture"))
        {
            SaveGradientTexture();
        }

        // ボタン間にスペースを追加
        GUILayout.Space(10);  // ここでスペースを追加

        // ワンポチ保存ボタン（前回保存したパスに保存）
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

    // テクスチャを生成し保存するメソッド
    private void SaveGradientTexture()
    {
        // ファイルパスの設定
        string extension = selectedFormat == FileFormat.PNG ? "png" : "tga";
        string path = EditorUtility.SaveFilePanel($"Save Gradient Texture as {extension.ToUpper()}", "", $"GradientTexture.{extension}", extension);

        if (!string.IsNullOrEmpty(path))
        {
            lastSavedPath = path;  // 最後に保存したパスを記録
            SaveTextureToPath(path);
        }
    }

    // 前回保存したパスにテクスチャを保存するメソッド
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

    // テクスチャを指定したパスに保存する共通メソッド
    private void SaveTextureToPath(string path)
    {
        Texture2D texture = GenerateGradientTexture();

        // 選択した形式に基づいてテクスチャを保存
        byte[] bytes = (selectedFormat == FileFormat.PNG) ? texture.EncodeToPNG() : texture.EncodeToTGA();
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Texture saved at: {path}");

        AssetDatabase.Refresh();
    }

    // テクスチャを生成するメソッド
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

    // プリセットを保存するメソッド
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

    // プリセットを適用するメソッド
    private void ApplyGradientPreset()
    {
        if (loadedPreset != null)
        {
            // プリセットからグラデーションを取得して適用
            gradient = loadedPreset.gradient;

            // GradientEditorに反映するためにRepaintを呼び出す
            Repaint();
            Debug.Log("Gradient preset applied.");
        }
    }
}
