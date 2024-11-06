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

    [MenuItem("SonchoTools/Material Texture Replacer")]
    public static void ShowWindow()
    {
        GetWindow<MaterialTextureReplacer>("Material Texture Replacer");
    }

    private void OnEnable()
    {
        // Load the saved fallback path from EditorPrefs
        fallbackFolderPath = EditorPrefs.GetString(FallbackFolderPathKey, "Assets/CommonTextures");

        // Initialize ReorderableList
        materialsList = new ReorderableList(materialsToEdit, typeof(Material), true, true, true, true);

        // Define how each element should be drawn
        materialsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            materialsToEdit[index] = (Material)EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                materialsToEdit[index], typeof(Material), false);
        };

        // Set up header for the list
        materialsList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Materials to Edit");
        };
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Material Textures", EditorStyles.boldLabel);
        GUILayout.Label("This tool automatically replaces texture references in materials by searching for textures in the \"Texture\" folder located in the same directory as the material.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        GUILayout.Label("Fallback Folder Path (used if texture is not found in the material's folder)", EditorStyles.boldLabel);

        // Fallback folder path input field
        EditorGUI.BeginChangeCheck();
        fallbackFolderPath = EditorGUILayout.TextField("Fallback Folder Path", fallbackFolderPath);

        // Save the fallback path if changed
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(FallbackFolderPathKey, fallbackFolderPath);
        }

        // Draw the ReorderableList
        materialsList.DoLayoutList();

        // Handle drag-and-drop functionality for adding materials
        HandleDragAndDrop();

        // Replace Textures button
        if (GUILayout.Button("Replace Textures"))
        {
            ReplaceMaterialTextures();
        }
    }

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop Materials Here", EditorStyles.helpBox);

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

            // Get texture folder from material path
            string materialPath = AssetDatabase.GetAssetPath(material);
            string textureFolderPath = GetTextureFolderPath(materialPath);

            if (string.IsNullOrEmpty(textureFolderPath))
            {
                Debug.LogWarning($"Texture folder not found for material {material.name}.");
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
                            Debug.LogWarning($"Texture '{textureName}' not found in folder '{textureFolderPath}'. Searching fallback folder...");
                            texturePath = FindTextureInFolder(textureName, fallbackFolderPath);
                        }

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
