using UnityEngine;
using UnityEditor;

public class RandomDuplicator : MonoBehaviour
{
    [MenuItem("Tools/Duplicate with Random Offset %#d")]
    static void DuplicateWithRandomness()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        Vector3 position = selected.transform.position;

        // Separate ranges for each axis
        float posRangeX = 1.5f;
        float posRangeY = 0.05f;
        float posRangeZ = 1.5f;

        float rotRange = 360f;

        // Per-axis random offset
        Vector3 randomOffset = new Vector3(
            Random.Range(-posRangeX, posRangeX),
            Random.Range(-posRangeY, posRangeY),
            Random.Range(-posRangeZ, posRangeZ)
        );

        // Full 3D rotation
        Vector3 randomRotation = new Vector3(
            Random.Range(-rotRange, rotRange),
            Random.Range(-rotRange, rotRange),
            Random.Range(-rotRange, rotRange)
        );

        GameObject newObj = PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(selected)) as GameObject;
        if (newObj == null)
        {
            newObj = Object.Instantiate(selected);
        }

        newObj.transform.position = position + randomOffset;
        newObj.transform.rotation = Quaternion.Euler(selected.transform.eulerAngles + randomRotation);
        newObj.name = selected.name + "_copy";

        Undo.RegisterCreatedObjectUndo(newObj, "Random Duplicate");

        Debug.Log($"Duplicated with offset: {randomOffset}, rotation: {randomRotation}");
    }
}
