using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ObjectToggleTool : EditorWindow
{
    private List<GameObject> objectList = new List<GameObject>();
    private int currentIndex = -1;

    // ウィンドウをメニューから開く
    [MenuItem("SonchoTools/Object Toggle Tool")]
    public static void ShowWindow()
    {
        GetWindow<ObjectToggleTool>("Object Toggle Tool");
    }

    // エディタウィンドウのGUI描画
    private void OnGUI()
    {
        GUILayout.Label("オブジェクトリスト", EditorStyles.boldLabel);

        // ゲームオブジェクトをドラッグ＆ドロップするエリア
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("ヒエラルキーからゲームオブジェクトをドラッグ＆ドロップしてください");
        EditorGUILayout.EndVertical();

        // ドロップイベント処理
        var dropArea = GUILayoutUtility.GetLastRect();
        HandleDragAndDrop(dropArea);

        // リストにあるオブジェクトを表示する
        for (int i = 0; i < objectList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            objectList[i] = (GameObject)EditorGUILayout.ObjectField(objectList[i], typeof(GameObject), true);

            // リストから削除するボタン
            if (GUILayout.Button("削除", GUILayout.Width(60)))
            {
                objectList.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        // 「次のオブジェクトをアクティブにする」ボタン
        if (GUILayout.Button("次のオブジェクトをアクティブにする"))
        {
            ToggleNextObject();
        }

        // リストが空のときの警告
        if (objectList.Count == 0)
        {
            EditorGUILayout.HelpBox("リストにゲームオブジェクトを追加してください", MessageType.Warning);
        }
    }

    // ドラッグ＆ドロップされたオブジェクトをリストに追加する
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

    // 次のオブジェクトをアクティブにし、他を非アクティブにする
    private void ToggleNextObject()
    {
        if (objectList.Count == 0)
        {
            Debug.LogError("リストにオブジェクトがありません。");
            return;
        }

        // 現在のアクティブオブジェクトを非アクティブにする
        if (currentIndex >= 0 && currentIndex < objectList.Count)
        {
            objectList[currentIndex].SetActive(false);
        }

        // 次のオブジェクトをアクティブにする
        currentIndex = (currentIndex + 1) % objectList.Count;
        if (objectList[currentIndex] != null)
        {
            objectList[currentIndex].SetActive(true);

            // アクティブにしたオブジェクトをヒエラルキーで選択状態にする
            Selection.activeObject = objectList[currentIndex];
        }
    }
}
