using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace wormNode
{
    public class FieldInfoEditor
    {

        private SerializedObject obj;
        private string fieldInfo;
        public Rect rect;
        public bool isDraw;
        public NodePort port;
        public bool isConnecting;
        public FieldInfoEditor(NodePort port, string fieldInfo, Rect rect)
        {
            this.port = port;
            this.obj = new SerializedObject(port.Node);
            this.fieldInfo = fieldInfo;
            this.rect = rect;
            isDraw = true;
        }

        public void Draw()
        {
            if (port.IsInput)
            {
                GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().colorInPort;
            }
            else
            {
                GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().colorOutPort;
            }
          
            GUI.DrawTexture(rect, EditorResources.dot);
            GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().colorPortCircle;
            if (!port.IsConnected && !isConnecting)
            {
                GUI.DrawTexture(rect, EditorResources.dotOuter);
            }

           
        }
        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (rect.Contains(e.mousePosition))
                        {
                          
                            isConnecting = true;
                            GraphEditorWindow.window.startPort = this;
                            GUI.changed = true;
                            //Debug.LogError(GraphEditorWindow.window.startPort.port.FieldName.ToString());
                        }
                        else
                        {
                            GUI.changed = true;
                            isConnecting = false;
                            //GraphEditorWindow.window.startPort = null;
                        }
                    }

                    break;
                case EventType.MouseUp:

                    //Debug.LogError(GraphEditorWindow.window.startPort == null);
                    if (GraphEditorWindow.window.startPort != null && rect.Contains(e.mousePosition))
                    {
                        //Debug.LogError(port.FieldName);
                        if (GraphEditorWindow.window.startPort.port.IsConnected)
                        {
                            if (GraphEditorWindow.window.startPort.port.ConnectionType == Node.ConnectionType.Multiple && port.IsConnected ==false)
                            {
                                GraphEditorWindow.window.startPort.port.Connect(port);
                                GraphEditorWindow.window.OnAddConnection(GraphEditorWindow.window.startPort.port, port);
                            }
                        }
                        else
                        {
                           
                            GraphEditorWindow.window.startPort.port.Connect(port);
                            GraphEditorWindow.window.OnAddConnection(GraphEditorWindow.window.startPort.port, port);
                        }
                    }
                    GUI.changed = true;
                    isConnecting = false;
                    //GraphEditorWindow.window.startPort = null;
                    break;
               
                case EventType.MouseDrag:
                    if (e.button == 0 && isConnecting)
                    {
                        //Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;    

            }
            //Debug.LogError(rect);
            //Debug.LogError(e.mousePosition);
            if  (!isConnecting && rect.Contains(e.mousePosition))
            {
               
                ShowValueBox(e.mousePosition);
            }
            if (isConnecting && port.IsOutput)
            {
                DrawDrawBezier(e.mousePosition);
            }
            return false;
        }


        void ShowValueBox(Vector2 pos)
        {
            GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().colorbg;
            var re = new Rect(pos.x - 80, pos.y - 20, 80, 20);
            GUI.color = WormNodeGlobalSettings.GetOrCreateSettings().contentColor;
            if (port.Node.GetValue(port) != null)
            {
                var str = port.Node.GetValue(port).ToString();
                GUI.Box(re, "val=" + str);
            }
          
           
           
          
        }

        void DrawDrawBezier(Vector2 pos)
        {
          
            GraphEditorWindow.DrawBezier(rect.center, pos);
        }
    }
}