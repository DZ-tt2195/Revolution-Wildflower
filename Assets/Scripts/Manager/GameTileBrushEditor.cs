using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;

[CustomEditor(typeof(GameTileBrush))]
public class GameTileBrushEditor : GridBrushEditorBase
{
    private SerializedProperty TileGrid;
    /*private void OnEnable()
    {
        TileGrid = serializedObject.FindProperty("_tileGrid");
        MonoBehaviour gridMono = TileGrid.objectReferenceValue as MonoBehaviour;
        GameObject[] targets = new GameObject[gridMono.transform.childCount];
        for (var i = 0; i < targets.Length; i++)
        {
            targets[i] = gridMono.transform.GetChild(0).gameObject;
        }
        Debug.Log(targets.Length);
        validTargets.AddRange(targets);
    }

    public override void OnPaintInspectorGUI()
    {
        
        base.OnPaintInspectorGUI();
    }*/
}
