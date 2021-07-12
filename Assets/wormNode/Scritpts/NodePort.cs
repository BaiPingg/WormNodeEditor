using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace wormNode
{
    [Serializable]
    public class NodePort
    {
        public enum IO { In,Out}
        /// <summary>
        /// 结点指向的变量名字
        /// </summary>
        [SerializeField] private string fieldName;
        [SerializeField] private IO direction;
        [SerializeField] private Node node;
        [SerializeField] private string _typeQualifiedName;
        /// <summary>
        /// 不能使用这个属性
        /// </summary>
        [SerializeField] public List<PortConnection> connections = new List<PortConnection>();
        [SerializeField] private Node.ConnectionType connectionType;
        [SerializeField] private Node.TypeConstraint typeConstraint;
        public bool IsInput { get => direction == IO.In ? true : false; }
        public bool IsOutput { get => direction == IO.Out ? true : false; }
        public bool IsConnected { get { return connections.Count != 0; } }

        public int ConnectionCount { get { return connections.Count; } }
        public IO Direction { get => direction;}
        public string FieldName { get => fieldName; }
        public Node Node { get => node;  }
        public Node.ConnectionType ConnectionType
        {
            get { return connectionType; }
            internal set { connectionType = value; }
        }
        public Node.TypeConstraint TypeConstraint
        {
            get { return typeConstraint; }
            internal set { typeConstraint = value; }
        }
        /// <summary>
        /// 返回第一个不是空的连接的节点
        /// </summary>
        public NodePort Connection
        {
            get
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    if (connections[i] != null) return connections[i].Port;
                }
                return null;
            }
        }
        public List<PortConnection> GetPortConnections()
        {
            return connections;
        }

        public Type ValueType
        {
            get
            {
                if (valueType == null && !string.IsNullOrEmpty(_typeQualifiedName)) valueType = Type.GetType(_typeQualifiedName, false);
                return valueType;
            }
            set
            {
                valueType = value;
                if (value != null) _typeQualifiedName = value.AssemblyQualifiedName;
            }
        }

       

        private Type valueType;

        public NodePort(FieldInfo fieldInfo)
        {
            fieldName = fieldInfo.Name;
            ValueType = fieldInfo.FieldType;
            var attribs = fieldInfo.GetCustomAttributes(false);
            for (int i = 0; i < attribs.Length; i++)
            {
                if (attribs[i] is Node.InputAttribute)
                {
                    direction = IO.In;
                    connectionType = (attribs[i] as Node.InputAttribute).connectionType;
                    typeConstraint = (attribs[i] as Node.InputAttribute).typeConstraint;
                }
                else if (attribs[i] is Node.OutPutAttribute)
                {
                    direction = IO.Out;
                    connectionType = (attribs[i] as Node.InputAttribute).connectionType;
                    typeConstraint = (attribs[i] as Node.OutPutAttribute).typeConstraint;
                }
            }
        }
        /// <summary>
        /// 复制结点但
        /// </summary>
        /// <param name="nodePort"></param>
        /// <param name="node"></param>
        public NodePort(NodePort nodePort, Node node)
        {
            fieldName = nodePort.fieldName;
            ValueType = nodePort.valueType;
            direction = nodePort.direction;
            connectionType = nodePort.connectionType;
            typeConstraint = nodePort.typeConstraint;
            this.node = node;
        }
        public NodePort(string fieldName, Type type, IO direction, Node.ConnectionType connectionType, Node.TypeConstraint typeConstraint, Node node)
        {
            this.fieldName = fieldName;
            this.ValueType = valueType;
            this.direction = direction;
            this.connectionType = connectionType;
            this.typeConstraint = typeConstraint;
            this.node = node;
        }
        /// <summary>
        /// 确保所有连接都不为空
        /// </summary>
        public void VerifyConnections()
        {
            for (int i = connections.Count - 1; i >= 0; i--)
            {
               
                if (connections[i].node != null &&
                    !string.IsNullOrEmpty(connections[i].fieldName) &&
                    connections[i].node.GetPort(connections[i].fieldName) != null)
                    continue;
              
                connections.RemoveAt(i);
            }
        }
        /// <summary>
        /// 获取连接的输出值
        /// </summary>
        /// <returns></returns>
        public object GetOutputValue()
        {
            if (direction == IO.In) return null;
            return node.GetValue(this);
        }
        /// <summary>
        /// 获取连接的第一个不为空结点输出值
        /// </summary>
        /// <returns></returns>
        public object GetInputValue()
        {
            NodePort connectedPort = Connection;
            if (connectedPort == null) return null;
            return connectedPort.GetOutputValue();
        }
        public T GetInputValue<T>()
        {
            object obj = GetInputValue();
            return obj is T ? (T)obj : default(T);
        }
        public object[] GetInputValues()
        {
            object[] objs = new object[ConnectionCount];
            for (int i = 0; i < ConnectionCount; i++)
            {
                NodePort connectedPort = connections[i].Port;
                if (connectedPort == null)
                { //为空剔除
                    connections.RemoveAt(i);
                    i--;
                    continue;
                }
                objs[i] = connectedPort.GetOutputValue();
            }
            return objs;
        }
        public T[] GetInputValues<T>()
        {
            object[] objs = GetInputValues();
            T[] ts = new T[objs.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] is T) ts[i] = (T)objs[i];
            }
            return ts;
        }

        public bool TryGetInputValue<T>(out T value)
        {
            object obj = GetInputValue();
            if (obj is T)
            {
                value = (T)obj;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
        public float GetInputSum(float fallback)
        {
            object[] objs = GetInputValues();
            if (objs.Length == 0) return fallback;
            float result = 0;
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] is float) result += (float)objs[i];
            }
            return result;
        }
        /// <summary>
        /// 是否连接到另一个结点接口
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool IsConnectedTo(NodePort port)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].Port == port) return true;
            }
            return false;
        }

        public void ClearConnections()
        {
            while (connections.Count > 0)
            {
                Disconnect(connections[0].Port);
            }
        }

        /// <summary>
        /// 连接到另外一个结点接口
        /// </summary>
        /// <param name="port"></param>
        public void Connect(NodePort port)
        {
            if (connections == null) connections = new List<PortConnection>();
            if (port == null) { Debug.LogWarning("Cannot connect to null port"); return; }
            if (port == this) { Debug.LogWarning("Cannot connect port to self."); return; }
            if (IsConnectedTo(port)) { Debug.LogWarning("Port already connected. "); return; }
            if (direction == port.direction) { Debug.LogWarning("Cannot connect two " + (direction == IO.In ? "input" : "output") + " connections"); return; }
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(node, "Connect Port");
            UnityEditor.Undo.RecordObject(port.node, "Connect Port");
#endif
            if (port.connectionType == Node.ConnectionType.Override && port.ConnectionCount != 0) { port.ClearConnections(); }
            if (connectionType == Node.ConnectionType.Override && ConnectionCount != 0) { ClearConnections(); }
            connections.Add(new PortConnection(port));
            if (port.connections == null) port.connections = new List<PortConnection>();
            if (!port.IsConnectedTo(this)) port.connections.Add(new PortConnection(this));
            node.OnCreateConnection(this, port);
            port.node.OnCreateConnection(this, port);
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="port"></param>
        public void Disconnect(NodePort port)
        {
            // Remove this ports connection to the other
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                if (connections[i].Port == port)
                {
                    connections.RemoveAt(i);
                }
            }
            if (port != null)
            {
                // Remove the other ports connection to this port
                for (int i = 0; i < port.connections.Count; i++)
                {
                    if (port.connections[i].Port == this)
                    {
                        port.connections.RemoveAt(i);
                    }
                }
            }
            // Trigger OnRemoveConnection
            node.OnRemoveConnection(this);
            if (port != null) port.node.OnRemoveConnection(port);
        }

        public void Disconnect(int i)
        {
            // Remove the other ports connection to this port
            NodePort otherPort = connections[i].Port;
            if (otherPort != null)
            {
                for (int k = 0; k < otherPort.connections.Count; k++)
                {
                    if (otherPort.connections[k].Port == this)
                    {
                        otherPort.connections.RemoveAt(i);
                    }
                }
            }
            // Remove this ports connection to the other
            connections.RemoveAt(i);

            // Trigger OnRemoveConnection
            node.OnRemoveConnection(this);
            if (otherPort != null) otherPort.node.OnRemoveConnection(otherPort);
        }

        public int GetInputSum(int fallback)
        {
            object[] objs = GetInputValues();
            if (objs.Length == 0) return fallback;
            int result = 0;
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] is int) result += (int)objs[i];
            }
            return result;
        }

        //public List<Vector2> GetReroutePoints(int index)
        //{
        //    return connections[index].reroutePoints;
        //}
        
        public void AddConnections(NodePort targetPort)
        {
            int connectionCount = targetPort.ConnectionCount;
            for (int i = 0; i < connectionCount; i++)
            {
                PortConnection connection = targetPort.connections[i];
                NodePort otherPort = connection.Port;
                Connect(otherPort);
            }
        }

        public void MoveConnections(NodePort targetPort)
        {
            int connectionCount = connections.Count;

           
            for (int i = 0; i < connectionCount; i++)
            {
                PortConnection connection = targetPort.connections[i];
                NodePort otherPort = connection.Port;
                Connect(otherPort);
            }
            ClearConnections();
        }

        public void Redirect(List<Node> oldNodes, List<Node> newNodes)
        {
            foreach (PortConnection connection in connections)
            {
                int index = oldNodes.IndexOf(connection.node);
                if (index >= 0) connection.node = newNodes[index];
            }
        }

        public void SwapConnections(NodePort targetPort)
        {
            int aConnectionCount = connections.Count;
            int bConnectionCount = targetPort.connections.Count;

            List<NodePort> portConnections = new List<NodePort>();
            List<NodePort> targetPortConnections = new List<NodePort>();

            // Cache port connections
            for (int i = 0; i < aConnectionCount; i++)
                portConnections.Add(connections[i].Port);

            // Cache target port connections
            for (int i = 0; i < bConnectionCount; i++)
                targetPortConnections.Add(targetPort.connections[i].Port);

            ClearConnections();
            targetPort.ClearConnections();

            // Add port connections to targetPort
            for (int i = 0; i < portConnections.Count; i++)
                targetPort.Connect(portConnections[i]);

            // Add target port connections to this one
            for (int i = 0; i < targetPortConnections.Count; i++)
                Connect(targetPortConnections[i]);

        }

       


    }
    [Serializable]
    public class PortConnection //: Dictionary<string, NodePort>, ISerializationCallbackReceiver
    {
        [SerializeField] public string fieldName;
        [SerializeField] public Node node;
        public NodePort Port { get { return port != null ? port : port = GetPort(); } }

        [NonSerialized] private NodePort port;
        /// <summary> Extra connection path points for organization </summary>
        //[SerializeField] public List<Vector2> reroutePoints = new List<Vector2>();

        public PortConnection(NodePort port)
        {
         
            this.port = port;
            node = port.Node;
            fieldName = port.FieldName;
        }

        /// <summary> Returns the port that this <see cref="PortConnection"/> points to </summary>
        private NodePort GetPort()
        {
            if (node == null || string.IsNullOrEmpty(fieldName)) return null;
            return node.GetPort(fieldName);
        }
    }
}