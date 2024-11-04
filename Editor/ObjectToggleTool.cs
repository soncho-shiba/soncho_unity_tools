using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ObjectToggleTool : EditorWindow
{
    private List<GameObject> objectList = new List<GameObject>();
    private int currentIndex = -1;

    [MenuItem("SonchoTools/Object Toggle Tool")]
    public static void ShowWindow()
    {
        GetWindow<ObjectToggleTool>("Object Toggle Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("オブジェクトリスト", EditorStyles.boldLabel);

        // ドラッグ＆ドロップエリア
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("ヒエラルキーからゲームオブジェクトをドラッグ＆ドロップしてください");
        EditorGUILayout.EndVertical();

        var dropArea = GUILayoutUtility.GetLastRect();
        HandleDragAndDrop(dropArea);

        // ボタンエリア
        if (GUILayout.Button("次のオブジェクトをアクティブにする"))
        {
            ToggleNextObject();
        }
        GUILayout.Space(20);
        if (GUILayout.Button("すべてのオブジェクトをアクティブにする"))
        {
            ActivateAllObjects();
        }
        GUILayout.Space(10);

        if (GUILayout.Button("リストをリセット"))
        {
            ResetObjectList();
        }
        GUILayout.Space(10);

        if (objectList.Count == 0)
        {
            EditorGUILayout.HelpBox("リストにゲームオブジェクトを追加してください", MessageType.Warning);
        }

        // オブジェクト表示と削除ボタンをボタンの下に配置
        for (int i = 0; i < objectList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            objectList[i] = (GameObject)EditorGUILayout.ObjectField(objectList[i], typeof(GameObject), true);
            if (GUILayout.Button("削除", GUILayout.Width(60)))
            {
                objectList.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

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
                    if (draggedObject is GameObject gameObject && !objectList.Contains(gameObject))
                    {
                        objectList.Add(gameObject);
                    }
                }
            }
        }
    }

    private void ToggleNextObject()
    {
        if (objectList.Count == 0)
        {
            Debug.LogError("リストにオブジェクトがありません。");
            return;
        }

        if (currentIndex >= 0 && currentIndex < objectList.Count)
        {
            objectList[currentIndex].SetActive(false);
        }

        currentIndex = (currentIndex + 1) % objectList.Count;
        if (objectList[currentIndex] != null)
        {
            objectList[currentIndex].SetActive(true);
            Selection.activeObject = objectList[currentIndex];
        }
    }

    private void ActivateAllObjects()
    {
        foreach (var obj in objectList)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    private void ResetObjectList()
    {
        objectList.Clear();
        currentIndex = -1;
    }
}
