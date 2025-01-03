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
    private Dictionary<string, List<string>> externalTextures = new Dictionary<string, List<string>>(); // 外部参照のテクスチャ一覧
    private Vector2 externalScrollPosition; // 外部参照リストのスクロール位置

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
        GUILayout.Label("マテリアルと同じディレクトリ内の「Texture」フォルダ内のテクスチャを検索して、マテリアルのテクスチャ参照を自動的に置き換えます。", EditorStyles.wordWrappedLabel);
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

        GUILayout.Space(10);

        // 外部参照のテクスチャを検索するボタン
        if (GUILayout.Button("外部参照のテクスチャを表示"))
        {
            ListExternalTextures();
        }

        // 外部参照の結果を表示
        if (externalTextures.Count > 0)
        {
            GUILayout.Label("外部参照のテクスチャ一覧", EditorStyles.boldLabel);
            externalScrollPosition = EditorGUILayout.BeginScrollView(externalScrollPosition, GUILayout.Height(200));

            foreach (var material in externalTextures)
            {
                GUILayout.Label($"マテリアル: {material.Key}", EditorStyles.boldLabel);
                foreach (var texturePath in material.Value)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(texturePath);

                    if (GUILayout.Button("コピー", GUILayout.Width(50)))
                    {
                        EditorGUIUtility.systemCopyBuffer = texturePath;
                        Debug.Log($"コピーしました: {texturePath}");
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
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
            if (material == null) continue;

            string materialPath = AssetDatabase.GetAssetPath(material);
            string textureFolderPath = GetTextureFolderPath(materialPath);

            Shader shader = material.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);

            for (int i = 0; i < propertyCount; i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = material.GetTexture(propertyName);

                    if (texture == null) continue;

                    string textureName = texture.name;
                    string texturePath = FindTextureInFolder(textureName, textureFolderPath) ??
                                         FindTextureInFolder(textureName, fallbackFolderPath);

                    if (texturePath == null) continue;

                    Texture newTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
                    material.SetTexture(propertyName, newTexture);
                    EditorUtility.SetDirty(material);
                }
            }
        }

        AssetDatabase.SaveAssets();
    }

    private void ListExternalTextures()
    {
        externalTextures.Clear();

        foreach (var material in materialsToEdit)
        {
            if (material == null) continue;

            Shader shader = material.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);
            List<string> externalTexturePaths = new List<string>();

            for (int i = 0; i < propertyCount; i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv) continue;

                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                Texture texture = material.GetTexture(propertyName);

                if (texture == null) continue;

                string texturePath = AssetDatabase.GetAssetPath(texture);
                if (!string.IsNullOrEmpty(texturePath) &&
                    !texturePath.Contains("Assets/Textures") &&
                    !texturePath.Contains("Assets/CommonTextures"))
                {
                    externalTexturePaths.Add(texturePath);
                }
            }

            if (externalTexturePaths.Count > 0)
            {
                externalTextures[material.name] = externalTexturePaths;
            }
        }
    }

    private string GetTextureFolderPath(string materialPath)
    {
        string materialFolderPath = Path.GetDirectoryName(materialPath);
        string parentFolder = Path.GetDirectoryName(materialFolderPath);
        string[] directories = Directory.GetDirectories(parentFolder, "*Texture*", SearchOption.TopDirectoryOnly);

        return directories.Length > 0 ? directories[0] : null;
    }

    private string FindTextureInFolder(string textureName, string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath)) return null;

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

