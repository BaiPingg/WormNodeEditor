using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.Text;

namespace wormNode
{
    public class GraphEditorWindow : EditorWindow
    {

        private WormNodeGlobalSettings globalSetting;
        public static Graph targetGraph;
        private Vector2 offset;
        private Vector2 drag;
        private Dictionary<Node,NodeEditor> nodeDic =new Dictionary<Node, NodeEditor>();
      
        public static GraphEditorWindow window;
        public FieldInfoEditor startPort;
        public Action<NodePort, NodePort> OnAddConnection;

        public static void CreateWindow(Graph graph)
        {
             window = (GraphEditorWindow)EditorWindow.GetWindow(typeof(GraphEditorWindow));
            targetGraph = graph;
            window.Show();
        }

        private void OnEnable()
        {
            OnAddConnection += OnAddPortConnection;
        }
        private void OnDisable()
        {
            OnAddConnection -= OnAddPortConnection;
        }
        private void OnDestroy()
        {
            AssetDatabase.SaveAssets();
        }

        private void OnGUI()
        {
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);
           
            
            DrawNode();
            DrawConnection();
            ProcessEvEnt(Event.current);
            ProcessNodeEvents(Event.current);
            if (GUI.changed) Repaint();
        }

        private void ProcessNodeEvents(Event e)
        {
            foreach (var item in nodeDic)
            {
                bool guiChanged = item.Value.ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }

            }
  
        }

        void ProcessEvEnt(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 1)
                    {
                        ProcessContextMenu(e.mousePosition);
                    }
                    if (e.button == 0)
                    {

                    }
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                       
                    }
                    break;
            }
        }

        void DrawNode()
        {
            
            for (int i = 0; i < targetGraph.nodes.Count; i++)
            {
                var node = targetGraph.nodes[i];
                if (nodeDic.ContainsKey(node))
                {
                    nodeDic[node].Draw();
                }
                else
                {
                    var edi = new NodeEditor(node);
                    edi.OnRemoveNode += RemoveNode;
                    nodeDic.Add(node, edi);
                    nodeDic[node].Draw();
                }
               
            }
        }


     
       

     

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }



        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
             var types =  AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(Node)));

            foreach (var item in types)
            {
                genericMenu.AddItem(new GUIContent(item.ToString()), false, () => OnClickAddNode(item, mousePosition));
            }
            genericMenu.ShowAsContext();  
            
        }

        private Node OnClickAddNode(Type type,Vector2 pos)
        {
           
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = node.ToString();
            node.position = pos;
            targetGraph.AddNode(node);
            AssetDatabase.AddObjectToAsset(node, targetGraph);
            //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(targetGraph));
            AssetDatabase.SaveAssets();
            NodeEditor editor = new NodeEditor(node);
            nodeDic.Add(node, editor);
            editor.OnRemoveNode += RemoveNode;
            return node;
        }


        public void RemoveNode(NodeEditor nodeEditor)
        {
          
            nodeDic.Remove(nodeEditor.Node);
            targetGraph.RemoveNode(nodeEditor.Node); 
            AssetDatabase.RemoveObjectFromAsset(nodeEditor.Node);
            AssetDatabase.SaveAssets();
            DestroyImmediate(nodeEditor.Node);

            foreach (var item in targetGraph.nodes)
            {
                item.VerifyConnections();
            }
            GUI.changed = true;
            //EditorUtility.SetDirty(targetGraph);
        }
        void DrawConnection()
        {
            foreach (var item in targetGraph.nodes)
            {
                item.VerifyConnections();
                var nodeEditor = nodeDic[item];
                foreach (var outport in item.Outputs)
                {
                    var portEditor = nodeEditor.outPort[outport.FieldName];
                    var connec = outport.GetPortConnections();
                    for (int i = 0; i < connec.Count; i++)
                    {
                        var port =  connec[i];
                        var nodeEditor2 = nodeDic[port.node];
                        //Debug.LogError(nodeEditor2 == nodeEditor);
                        var portEditor2 = nodeEditor2.inPort[port.fieldName];
                        DrawBezier(portEditor, portEditor2);
                        //Debug.LogError(portEditor.port.FieldName+" " + portEditor2.port.FieldName);

                    }
                }
            }
        }

        

        void DrawBezier(FieldInfoEditor from ,FieldInfoEditor to)
        {
            DrawBezier(from.rect.center, to.rect.center);
           if( GUI.Button(new Rect((from.rect.center + to.rect.center) * 0.5f,new Vector2(14,14)),""))
            {
                //Debug.LogError("fff");
                from.port.Disconnect(to.port);
                to.port.Disconnect(from.port);
                GUI.changed = true;
            }
            //if (Handles.Button((from.rect.center + to.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
            //{
            //    Debug.LogError("fff");
            //    from.port.Disconnect(to.port);
            //    to.port.Disconnect(from.port);
            //    GUI.changed = true;
            //}
        }
        public static void DrawBezier(Vector2 start ,Vector2 end)
        {
            Handles.DrawBezier(start, end, start + Vector2.right * 50f,
                end - Vector2.right * 50f, WormNodeGlobalSettings.GetOrCreateSettings().linetColor, null, 2f);    
        }
        void OnAddPortConnection(NodePort from, NodePort to)
        {
            //Debug.LogError("ff");
            nodeDic[from.Node].obj.ApplyModifiedProperties();
            nodeDic[to.Node].obj.ApplyModifiedProperties();
            nodeDic[to.Node].obj.Update();
            //EditorUtility.SetDirty(targetGraph);
            AssetDatabase.SaveAssets();
        }
    }


   
   

}