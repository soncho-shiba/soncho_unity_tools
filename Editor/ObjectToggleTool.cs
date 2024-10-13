using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ObjectToggleTool : EditorWindow
{
    private List<GameObject> objectList = new List<GameObject>();
    private int currentIndex = -1;

    // �E�B���h�E�����j���[����J��
    [MenuItem("SonchoTools/Object Toggle Tool")]
    public static void ShowWindow()
    {
        GetWindow<ObjectToggleTool>("Object Toggle Tool");
    }

    // �G�f�B�^�E�B���h�E��GUI�`��
    private void OnGUI()
    {
        GUILayout.Label("�I�u�W�F�N�g���X�g", EditorStyles.boldLabel);

        // �Q�[���I�u�W�F�N�g���h���b�O���h���b�v����G���A
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("�q�G�����L�[����Q�[���I�u�W�F�N�g���h���b�O���h���b�v���Ă�������");
        EditorGUILayout.EndVertical();

        // �h���b�v�C�x���g����
        var dropArea = GUILayoutUtility.GetLastRect();
        HandleDragAndDrop(dropArea);

        // ���X�g�ɂ���I�u�W�F�N�g��\������
        for (int i = 0; i < objectList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            objectList[i] = (GameObject)EditorGUILayout.ObjectField(objectList[i], typeof(GameObject), true);

            // ���X�g����폜����{�^��
            if (GUILayout.Button("�폜", GUILayout.Width(60)))
            {
                objectList.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        // �u���̃I�u�W�F�N�g���A�N�e�B�u�ɂ���v�{�^��
        if (GUILayout.Button("���̃I�u�W�F�N�g���A�N�e�B�u�ɂ���"))
        {
            ToggleNextObject();
        }

        // ���X�g����̂Ƃ��̌x��
        if (objectList.Count == 0)
        {
            EditorGUILayout.HelpBox("���X�g�ɃQ�[���I�u�W�F�N�g��ǉ����Ă�������", MessageType.Warning);
        }
    }

    // �h���b�O���h���b�v���ꂽ�I�u�W�F�N�g�����X�g�ɒǉ�����
    private void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;
        if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (var draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is GameObject gameObject)
                    {
                        if (!objectList.Contains(gameObject))
                        {
                            objectList.Add(gameObject);
                        }
                    }
                }
            }
        }
    }

    // ���̃I�u�W�F�N�g���A�N�e�B�u�ɂ��A�����A�N�e�B�u�ɂ���
    private void ToggleNextObject()
    {
        if (objectList.Count == 0)
        {
            Debug.LogError("���X�g�ɃI�u�W�F�N�g������܂���B");
            return;
        }

        // ���݂̃A�N�e�B�u�I�u�W�F�N�g���A�N�e�B�u�ɂ���
        if (currentIndex >= 0 && currentIndex < objectList.Count)
        {
            objectList[currentIndex].SetActive(false);
        }

        // ���̃I�u�W�F�N�g���A�N�e�B�u�ɂ���
        currentIndex = (currentIndex + 1) % objectList.Count;
        if (objectList[currentIndex] != null)
        {
            objectList[currentIndex].SetActive(true);

            // �A�N�e�B�u�ɂ����I�u�W�F�N�g���q�G�����L�[�őI����Ԃɂ���
            Selection.activeObject = objectList[currentIndex];
        }
    }
}
