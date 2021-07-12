using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace wormNode
{ 
    [CustomEditor(typeof(Graph),true)]
  
    public class GraphCustomEditor :Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Open", GUILayout.Height(40)))
            {
                GraphEditorWindow.CreateWindow(serializedObject.targetObject as Graph);
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Data", "BoldLabel");

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }

    }
}
