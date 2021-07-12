using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace wormNode
{
    [CustomEditor(typeof(Node), true)]

    public class GlobalNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Edit graph", GUILayout.Height(40)))
            {
               SerializedProperty graphProp = serializedObject.FindProperty("graph");
               GraphEditorWindow.CreateWindow(graphProp.objectReferenceValue as Graph);
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            GUILayout.Label("Data", "BoldLabel");

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }

    }
}