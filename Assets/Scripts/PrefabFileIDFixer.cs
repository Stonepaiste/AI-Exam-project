using UnityEngine;
using UnityEditor;

public class PrefabFileIDFixer : EditorWindow
{
    [MenuItem("Tools/Prefab FileID Fixer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabFileIDFixer>("Prefab FileID Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Fix FileID Conflicts in Scene Prefabs", EditorStyles.boldLabel);

        if (GUILayout.Button("Replace Prefab Instances"))
        {
            ReplaceAllPrefabsInScene();
        }
    }

    private void ReplaceAllPrefabsInScene()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int replacedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(obj))
            {
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj);
                if (prefab != null)
                {
                    // Copy dependencies: store transform
                    Transform oldTransform = obj.transform;
                    Vector3 position = oldTransform.position;
                    Quaternion rotation = oldTransform.rotation;
                    Vector3 scale = oldTransform.localScale;

                    // Replace with a new prefab instance
                    GameObject newInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, obj.scene);

                    // Restore transform
                    newInstance.transform.position = position;
                    newInstance.transform.rotation = rotation;
                    newInstance.transform.localScale = scale;

                    Undo.RegisterCreatedObjectUndo(newInstance, "Replace prefab instance");
                    Undo.DestroyObjectImmediate(obj);
                    replacedCount++;
                }
            }
        }

        Debug.Log($"Prefab FileID Fixer: Replaced {replacedCount} prefab instance(s) in the scene.");
    }
}