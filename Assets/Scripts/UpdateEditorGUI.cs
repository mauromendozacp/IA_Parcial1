using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapController))]
public class UpdateEditorGUI : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapController mapController = (MapController)target;

        if (GUILayout.Button("Update Map"))
        {
            mapController.UpdateMap();
        }
    }
}
