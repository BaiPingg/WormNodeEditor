using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace wormNode
{
    public class NodeEditor
    {
        public Rect rect;
      
        public bool isDragged;
        public bool isSelected;

        public GUIStyle style;
        public GUIStyle defaultNodeStyle;
        public GUIStyle selectedNodeStyle;
        public Action<NodeEditor> OnRemoveNode;
        private Node node;
        public Node Node { get { return node; } }
        public Dictionary<string,FieldInfoEditor > inPort = new Dictionary<string, FieldInfoEditor>();
        public Dictionary<string, FieldInfoEditor> outPort = new Dictionary<string, FieldInfoEditor>();
        private float height;
        public SerializedObject obj;
        private Rect insideBoxRect;
        public NodeEditor(Node node)
        {
            this.node = node;
            defaultNodeStyle = EditorResources.styles.normalNodeStyle;
            selectedNodeStyle = EditorResources.styles.highLightNodeStyle;
            style = defaultNodeStyle;
            height = WormNodeGlobalSettings.GetOrCreateSettings().columnHeight;
             FieldInfo[] fields = node.GetType().GetFields();
           
            for (int i = 0; i < fields.Length; i++)
            {
                var inputAtt = fields[i].GetCustomAttribute(typeof(Node.InputAttribute)) as Node.InputAttribute;
                if (inputAtt != null)
                {
                    var name = fields[i].ToString().Split(' ')[1];
                    node.AddDynamicInput(inputAtt.GetType(), inputAtt.connectionType, inputAtt.typeConstraint, name);
                
                   
                }
                var outputAtt = fields[i].GetCustomAttribute(typeof(Node.OutPutAttribute)) as Node.OutPutAttribute;
                if (outputAtt != null)
                {
                    var name = fields[i].ToString().Split(' ')[1];
                    node.AddDynamicOutput(outputAtt.GetType(), outputAtt.connectionType, outputAtt.typeConstraint,name);
                   
                }
            }
            rect = ComputeRect();

            for (int i = 0; i < node.Inputs.ToArray().Length; i++)
            {
                var name = node.Inputs.ToArray()[i].FieldName;
               inPort.Add(name, new FieldInfoEditor(node.Inputs.ToArray()[i], name, new Rect(node.position.x - 125, node.position.y + height * (i+1), 100, 30)));

            }
            for (int i = 0; i < node.Outputs.ToArray().Length; i++)
            {
                var name = node.Outputs.ToArray()[i].FieldName;
                outPort.Add(name, new FieldInfoEditor(node.Outputs.ToArray()[i], name, new Rect(node.position.x + rect.width + 125, node.position.y + height * (i + 1), 100, 30)));

            }


            obj = new SerializedObject(node);
        }

        public void Drag(Vector2 delta)
        {
            rect.position += delta;
            node.position += delta;
            foreach (var item in inPort)
            {
                item.Value.rect.position += delta;
            }
            
        }

        public void Draw()
        {
            GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().colorbg;
            GUI.Box(rect, "",style);
            GUILayout.BeginArea(new Rect(rect.x+16, rect.y+16, rect.width-32,rect.height-32));
            GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().contentColor;
            GUILayout.Label(node.GetType().ToString(), EditorResources.styles.titleStyle);
            GUILayout.Space(5);
            
            
            foreach (var item in node.Inputs)
            {
                
                if (item.IsConnected)
                {
                    GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().contentColor;
                    EditorResources.styles.contentStyle.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label(item.FieldName, EditorResources.styles.contentStyle);
                }
                else
                {
                    GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().contentColor;
                    EditorGUILayout.PropertyField(obj.FindProperty(item.FieldName), new GUIContent(item.FieldName));
                }
              

                var re = GUILayoutUtility.GetLastRect();
                inPort[item.FieldName].rect = new Rect(rect.x +re.x, rect.y+16+2 + re.y, 12, 12);
                
            }
           
          
            GUILayout.Space(5);
            foreach (var item in node.Outputs)
            {
                GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().contentColor;
                EditorResources.styles.contentStyle.alignment = TextAnchor.MiddleRight;
                GUILayout.Label(item.FieldName, EditorResources.styles.contentStyle);
                var re = GUILayoutUtility.GetLastRect();
                outPort[item.FieldName].rect = new Rect(rect.x + re.x+re.width +20 , rect.y + 16 + 2 + re.y, 12, 12);
            }

            FieldInfo[] fields = node.GetType().GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var outputAtt = fields[i].GetCustomAttribute(typeof(Node.ShowInEditorAttribute)) as Node.ShowInEditorAttribute;
                if (outputAtt != null)
                {
                    var name = fields[i].ToString().Split(' ')[1];
                    EditorGUILayout.PropertyField(obj.FindProperty(name), new GUIContent(name));
                   
                }
            }
         
          
            GUILayout.EndArea();


            for (int i = 0; i < inPort.Count; i++)
            {
                var tr = inPort.ElementAt(i);
               
                tr.Value.Draw();

            }
            for (int i = 0; i < outPort.Count; i++)
            {
                var tr = outPort.ElementAt(i);
              
                tr.Value.Draw();

            }
            //obj.ApplyModifiedPropertiesWithoutUndo();
            //obj.\
            
           
            obj.ApplyModifiedProperties();

        }

        public bool ProcessEvents(Event e)
        {
            for (int i = 0; i < inPort.Count; i++)
            {
                var tr = inPort.ElementAt(i);
                tr.Value.ProcessEvents(e);
            }
            for (int i = 0; i < outPort.Count; i++)
            {
                var tr = outPort.ElementAt(i);
                tr.Value.ProcessEvents(e);
            }

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (rect.Contains(e.mousePosition))
                        {
                            isDragged = true;
                            GUI.changed = true;
                            isSelected = true;
                            style = selectedNodeStyle;
                        }
                        else
                        {
                            GUI.changed = true;
                            isSelected = false;
                            style = defaultNodeStyle;
                        }
                    }

                    if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged)
                    {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
                   
            }

            return false;
        }

        private void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
            genericMenu.ShowAsContext();
          
        }

        private void OnClickRemoveNode()
        {
            if (OnRemoveNode != null)
            {
                OnRemoveNode(this);
            }
        }
        Rect ComputeRect()
        {
            var re = new Rect();
            re.position = node.position;
            re.width = WormNodeGlobalSettings.GetOrCreateSettings().nodeBaseWidth;
            re.height = height * (inPort.Count+outPort.Count) + 150;
             insideBoxRect = new Rect(150, 0, re.width - 300, re.height);
            return re;
        }

        private void DrawInPort(Rect rect, SerializedObject obj, string fieldName)
        {
            var str = fieldName.Split(' ');
          
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().colorInPort;
            GUILayout.Box(EditorResources.dot);
            GUI.color = Color.white;
            GUILayout.Label(str[1]);
            //EditorGUILayout.PropertyField(obj.FindProperty(str[1]), new GUIContent(str[1]),GUILayout.Width(200));
            obj.ApplyModifiedProperties();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

        }

        void DrawInFiels( Rect rect)
        {
            //GUI.Box(rect, "");
            GUILayout.BeginArea(rect);
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            for (int i = 0; i < inPort.Count; i++)
            {
                var item = inPort.ElementAt(i);
                GUILayout.BeginHorizontal(GUILayout.Height(height));
                GUILayout.Space(10);
                GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().colorInPort;
                //GUILayout.Box("", EditorResources.styles.inPortStyle, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
                if (GUILayout.Button("", EditorResources.styles.inPortStyle, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10)))
                {
                    Debug.LogError("ffff");
                }
                GUI.color = Color.black;
                GUILayout.Label( item.Key);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        void DrawOutFiels( Rect rect)
        {
            //GUI.Box(rect, "");
            GUILayout.BeginArea(rect);
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            var sty = new GUIStyle();
            sty.alignment = TextAnchor.MiddleRight;
            for (int i = 0; i < outPort.Count; i++)
            {
                var item = outPort.ElementAt(i);
                GUILayout.BeginHorizontal(GUILayout.Height(height));
                GUI.color = Color.black;
                GUILayout.Label(item.Key, sty);
                GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().colorOutPort;
                GUILayout.Box("", EditorResources.styles.outPortStyle, GUILayout.MaxHeight(10),GUILayout.MaxWidth(10));
                GUILayout.Space(10);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}