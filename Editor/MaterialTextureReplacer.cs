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
    private Vector2 scrollPosition; // �}�e���A�����X�g�̃X�N���[���ʒu

    [MenuItem("SonchoTools/�}�e���A�� �e�N�X�`���u���c�[��")]
    public static void ShowWindow()
    {
        GetWindow<MaterialTextureReplacer>("�}�e���A�� �e�N�X�`���u���c�[��");
    }

    private void OnEnable()
    {
        // �O��ۑ������t�H�[���o�b�N�p�X��EditorPrefs����ǂݍ���
        fallbackFolderPath = EditorPrefs.GetString(FallbackFolderPathKey, "Assets/CommonTextures");

        // ReorderableList��������
        materialsList = new ReorderableList(materialsToEdit, typeof(Material), true, true, true, true);

        // �e�v�f�̕`����@���`
        materialsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            materialsToEdit[index] = (Material)EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                materialsToEdit[index], typeof(Material), false);
        };

        // ���X�g�̃w�b�_�[��ݒ�
        materialsList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "�ҏW����}�e���A���ꗗ");
        };
    }

    private void OnGUI()
    {
        GUILayout.Label("�}�e���A�� �e�N�X�`���u��", EditorStyles.boldLabel);
        GUILayout.Label("���̃c�[���́A�}�e���A���Ɠ����f�B���N�g�����́uTexture�v�t�H���_���̃e�N�X�`�����������āA�}�e���A���̃e�N�X�`���Q�Ƃ������I�ɒu�������܂��B", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        GUILayout.Label("�t�H�[���o�b�N�t�H���_�p�X�i�}�e���A���̃t�H���_���Ƀe�N�X�`����������Ȃ��ꍇ�Ɏg�p����܂��j", EditorStyles.boldLabel);

        // �t�H�[���o�b�N�t�H���_�p�X�̓��̓t�B�[���h
        EditorGUI.BeginChangeCheck();
        fallbackFolderPath = EditorGUILayout.TextField("�t�H�[���o�b�N�t�H���_�p�X", fallbackFolderPath);

        // �t�H�[���o�b�N�p�X���ύX���ꂽ�ꍇ�ɕۑ�
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(FallbackFolderPathKey, fallbackFolderPath);
        }

        // �}�e���A�����X�g�̃X�N���[���r���[�J�n
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        materialsList.DoLayoutList();
        EditorGUILayout.EndScrollView();

        // �h���b�O���h���b�v�@�\������
        HandleDragAndDrop();

        // �e�N�X�`���u���{�^��
        if (GUILayout.Button("�e�N�X�`����u��������"))
        {
            ReplaceMaterialTextures();
        }
    }

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "�����Ƀ}�e���A�����h���b�O���h���b�v", EditorStyles.helpBox);

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
            Debug.LogError("���Ȃ��Ƃ�1�̃}�e���A�����w�肵�Ă��������B");
            return;
        }

        foreach (var material in materialsToEdit)
        {
            if (material == null)
            {
                continue;
            }

            Debug.Log($"�}�e���A����������: {material.name}");

            // �}�e���A���p�X����e�N�X�`���t�H���_���擾
            string materialPath = AssetDatabase.GetAssetPath(material);
            string textureFolderPath = GetTextureFolderPath(materialPath);

            if (string.IsNullOrEmpty(textureFolderPath))
            {
                Debug.LogWarning($"�}�e���A�� {material.name} �ɑΉ�����e�N�X�`���t�H���_��������܂���B");
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
                            Debug.LogWarning($"�t�H���_ '{textureFolderPath}' �Ƀe�N�X�`�� '{textureName}' ��������܂���ł����B�t�H�[���o�b�N�t�H���_���������܂�...");
                            texturePath = FindTextureInFolder(textureName, fallbackFolderPath);
                        }

                        if (!string.IsNullOrEmpty(texturePath))
                        {
                            Texture newTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
                            material.SetTexture(propertyName, newTexture);
                            Debug.Log($"�}�e���A�� {material.name} �̃e�N�X�`�� '{textureName}' �� '{texturePath}' �ɒu�������܂����B");
                        }
                        else
                        {
                            Debug.LogWarning($"�}�e���A�� {material.name} �̃e�N�X�`�� '{textureName}' ���A�t�H���_ '{textureFolderPath}' ����� '{fallbackFolderPath}' �̂�����ɂ�������܂���ł����B");
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
