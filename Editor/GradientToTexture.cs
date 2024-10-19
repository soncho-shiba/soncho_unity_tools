using UnityEngine;
using UnityEditor;
using System.IO;

public class GradientToTexture : EditorWindow
{
    Gradient gradient = new Gradient();  // Gradient を初期化
    int textureWidth = 256;
    int textureHeight = 16;

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

        // Gradient フィールドの初期化を確認
        if (gradient == null)
        {
            gradient = new Gradient();
        }

        // Gradient フィールド
        gradient = EditorGUILayout.GradientField("Gradient", gradient);

        // テクスチャのサイズ設定
        textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth);
        textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight);

        // 出力フォーマットの選択
        selectedFormat = (FileFormat)EditorGUILayout.EnumPopup("File Format", selectedFormat);

        if (GUILayout.Button("Generate and Save Texture"))
        {
            SaveGradientTexture();
        }
    }

    private void SaveGradientTexture()
    {
        // テクスチャ生成
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

        // 出力形式に応じたファイル名と拡張子を設定
        string extension = selectedFormat == FileFormat.PNG ? "png" : "tga";
        string path = EditorUtility.SaveFilePanel($"Save Gradient Texture as {extension.ToUpper()}", "", $"GradientTexture.{extension}", extension);

        if (!string.IsNullOrEmpty(path))
        {
            // PNGかTGAの選択に応じてエンコード
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

        // テクスチャのインポート設定
        AssetDatabase.Refresh();
    }
}
