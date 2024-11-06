using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;
using System.Collections.Generic;

public class MaterialTextureReplacer : EditorWindow
{
    private const string FallbackFolderPathKey = "MaterialTextureReplacer_FallbackFolderPath";
    public string fallbackFolderPath = "Assets/CommonTextures";
    public List<Material> materialsToEdit = new List<Material>();
    private ReorderableList materialsList;
    private Vector2 scrollPosition; // マテリアルリストのスクロール位置

    [MenuItem("SonchoTools/マテリアル テクスチャ置換ツール")]
    public static void ShowWindow()
    {
        GetWindow<MaterialTextureReplacer>("マテリアル テクスチャ置換ツール");
    }

    private void OnEnable()
    {
        // 前回保存したフォールバックパスをEditorPrefsから読み込む
        fallbackFolderPath = EditorPrefs.GetString(FallbackFolderPathKey, "Assets/CommonTextures");

        // ReorderableListを初期化
        materialsList = new ReorderableList(materialsToEdit, typeof(Material), true, true, true, true);

        // 各要素の描画方法を定義
        materialsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            materialsToEdit[index] = (Material)EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                materialsToEdit[index], typeof(Material), false);
        };

        // リストのヘッダーを設定
        materialsList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "編集するマテリアル一覧");
        };
    }

    private void OnGUI()
    {
        GUILayout.Label("マテリアル テクスチャ置換", EditorStyles.boldLabel);
        GUILayout.Label("このツールは、マテリアルと同じディレクトリ内の「Texture」フォルダ内のテクスチャを検索して、マテリアルのテクスチャ参照を自動的に置き換えます。", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        GUILayout.Label("フォールバックフォルダパス（マテリアルのフォルダ内にテクスチャが見つからない場合に使用されます）", EditorStyles.boldLabel);

        // フォールバックフォルダパスの入力フィールド
        EditorGUI.BeginChangeCheck();
        fallbackFolderPath = EditorGUILayout.TextField("フォールバックフォルダパス", fallbackFolderPath);

        // フォールバックパスが変更された場合に保存
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(FallbackFolderPathKey, fallbackFolderPath);
        }

        // マテリアルリストのスクロールビュー開始
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        materialsList.DoLayoutList();
        EditorGUILayout.EndScrollView();

        // ドラッグ＆ドロップ機能を処理
        HandleDragAndDrop();

        // テクスチャ置換ボタン
        if (GUILayout.Button("テクスチャを置き換える"))
        {
            ReplaceMaterialTextures();
        }
    }

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "ここにマテリアルをドラッグ＆ドロップ", EditorStyles.helpBox);

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is Material material)
                        {
                            if (!materialsToEdit.Contains(material))
                            {
                                materialsToEdit.Add(material);
                            }
                        }
                    }

                    evt.Use();
                }
            }
        }
    }

    private void ReplaceMaterialTextures()
    {
        if (materialsToEdit == null || materialsToEdit.Count == 0)
        {
            Debug.LogError("少なくとも1つのマテリアルを指定してください。");
            return;
        }

        foreach (var material in materialsToEdit)
        {
            if (material == null)
            {
                continue;
            }

            Debug.Log($"マテリアルを処理中: {material.name}");

            // マテリアルパスからテクスチャフォルダを取得
            string materialPath = AssetDatabase.GetAssetPath(material);
            string textureFolderPath = GetTextureFolderPath(materialPath);

            if (string.IsNullOrEmpty(textureFolderPath))
            {
                Debug.LogWarning($"マテリアル {material.name} に対応するテクスチャフォルダが見つかりません。");
                continue;
            }

            Shader shader = material.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);

            for (int i = 0; i < propertyCount; i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = material.GetTexture(propertyName);

                    if (texture != null)
                    {
                        string textureName = texture.name;
                        string texturePath = FindTextureInFolder(textureName, textureFolderPath);

                        if (string.IsNullOrEmpty(texturePath))
                        {
                            Debug.LogWarning($"フォルダ '{textureFolderPath}' にテクスチャ '{textureName}' が見つかりませんでした。フォールバックフォルダを検索します...");
                            texturePath = FindTextureInFolder(textureName, fallbackFolderPath);
                        }

                        if (!string.IsNullOrEmpty(texturePath))
                        {
                            Texture newTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
                            material.SetTexture(propertyName, newTexture);
                            Debug.Log($"マテリアル {material.name} のテクスチャ '{textureName}' を '{texturePath}' に置き換えました。");
                        }
                        else
                        {
                            Debug.LogWarning($"マテリアル {material.name} のテクスチャ '{textureName}' が、フォルダ '{textureFolderPath}' および '{fallbackFolderPath}' のいずれにも見つかりませんでした。");
                        }
                    }
                }
            }

            EditorUtility.SetDirty(material);
        }

        AssetDatabase.SaveAssets();
    }

    private string GetTextureFolderPath(string materialPath)
    {
        string materialFolderPath = Path.GetDirectoryName(materialPath);
        string parentFolder = Path.GetDirectoryName(materialFolderPath);
        string[] directories = Directory.GetDirectories(parentFolder, "*Texture*", SearchOption.TopDirectoryOnly);

        if (directories.Length > 0)
        {
            return directories[0];
        }

        return null;
    }

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
