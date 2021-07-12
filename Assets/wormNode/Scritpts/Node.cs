using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wormNode
{
    [Serializable]
    public class Node: ScriptableObject
    {
        /// <summary>
        /// 结点接口是否可以指向多个结点
        /// </summary>
        public enum ConnectionType
        {
            Multiple,
            Override,
        }
        /// <summary>
        /// 显示返回值editor
        /// </summary>
        public enum ShowBackingValue
        {
            Never,
            Unconnected,
            Always
        }
        public enum TypeConstraint
        {
            None,
            ///允许输入值可以转为输出值的情况
            Inherited,
            //相似
            Strict,
            ///允许输出值可以转为输入值的情况
            InheritedInverse,
        }

        public IEnumerable<NodePort> Ports { get { foreach (NodePort port in ports.Values) yield return port; } }
      
        public IEnumerable<NodePort> Outputs { get { foreach (NodePort port in Ports) { if (port.IsOutput) yield return port; } } }
        public IEnumerable<NodePort> Inputs { get { foreach (NodePort port in Ports) { if (port.IsInput) yield return port; } } }
        [SerializeField] public Graph graph;
        [SerializeField] public Vector2 position;
        [SerializeField] private NodePortDictionary ports = new NodePortDictionary();
        private void OnEnable()
        {
            Init();
        }
        /// <summary>检查并清理掉空的</summary>
        public void VerifyConnections()
        {
            foreach (NodePort port in Ports)
                port.VerifyConnections();
        }

        /// <summary> 初始化，在onenable 调用 </summary>
        protected virtual void Init() { }

        public void ClearConnections()
        { }



        public NodePort AddDynamicInput(Type type, Node.ConnectionType connectionType = Node.ConnectionType.Multiple, Node.TypeConstraint typeConstraint = TypeConstraint.None, string fieldName = null)
        {
            return AddDynamicPort(type, NodePort.IO.In, connectionType, typeConstraint, fieldName);
        }

        public NodePort AddDynamicOutput(Type type, Node.ConnectionType connectionType = Node.ConnectionType.Multiple, Node.TypeConstraint typeConstraint = TypeConstraint.None, string fieldName = null)
        {
            return AddDynamicPort(type, NodePort.IO.Out, connectionType, typeConstraint, fieldName);
        }

        private NodePort AddDynamicPort(Type type, NodePort.IO direction, Node.ConnectionType connectionType = Node.ConnectionType.Multiple, Node.TypeConstraint typeConstraint = TypeConstraint.None, string fieldName = null)
        {
            if (fieldName == null)
            {
                fieldName = "dynamicInput_0";
                int i = 0;
                while (HasPort(fieldName)) fieldName = "dynamicInput_" + (++i);
            }
            else if (HasPort(fieldName))
            {
                Debug.LogWarning("Port '" + fieldName + "' already exists in " + name, this);
                return ports[fieldName];
            }
            NodePort port = new NodePort(fieldName, type, direction, connectionType, typeConstraint, this);
            ports.Add(fieldName, port);
            return port;
        }

        /// <summary>
        /// 根据变量名获得out结点
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public NodePort GetOutputPort(string fieldName)
        {
            NodePort port = GetPort(fieldName);
            if (port == null || port.Direction != NodePort.IO.Out)
                return null;
            else
                return port;
        }
        /// <summary>
        /// 根据变量名获得in结点
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public NodePort GetInputPort(string fieldName)
        {
            NodePort port = GetPort(fieldName);
            if (port == null || port.Direction != NodePort.IO.In)
                return null;
            else
                return port;
        }
        /// <summary>
        /// 根据变量名获得结点
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public NodePort GetPort(string fieldName)
        {
            NodePort port;
            if (ports.TryGetValue(fieldName, out port)) return port;
            else return null;
        }
        public bool HasPort(string fieldName)
        {
            return ports.ContainsKey(fieldName);
        }

        /// <summary>
        /// 获取变量名的结点的对应的输入值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public T GetInputValue<T>(string fieldName, T fallback = default(T))
        {
            NodePort port = GetPort(fieldName);
            if (port != null && port.IsConnected)
            return port.GetInputValue<T>();
            else return fallback;
        }

        /// <summary>
        /// 获取变量名的结点的对应的输入值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public T[] GetInputValues<T>(string fieldName, params T[] fallback)
        {
            NodePort port = GetPort(fieldName);
            if (port != null && port.IsConnected) return port.GetInputValues<T>();
            else return fallback;
        }

        /// <summary>
        /// 必须重写
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public virtual object GetValue(NodePort port)
        {
            Debug.LogWarning("No GetValue(NodePort port) override defined for " + GetType());
            return null;
        }

        public virtual void OnCreateConnection(NodePort from, NodePort to) { }

        public virtual void OnRemoveConnection(NodePort port) { }

        #region Attribute

        [AttributeUsage(AttributeTargets.Field)]
        public class InputAttribute : Attribute
        {
            public ShowBackingValue backingValue;
            public ConnectionType connectionType;
            public bool dynamicPortList;
            public TypeConstraint typeConstraint;
            public InputAttribute(ShowBackingValue backingValue = ShowBackingValue.Unconnected, ConnectionType connectionType = ConnectionType.Override, TypeConstraint typeConstraint = TypeConstraint.None, bool dynamicPortList = false)
            {
                this.backingValue = backingValue;
                this.connectionType = connectionType;
                this.dynamicPortList = dynamicPortList;
                this.typeConstraint = typeConstraint;
            }

        }
        [AttributeUsage(AttributeTargets.Field)]
        public class OutPutAttribute : Attribute
        {
            public ShowBackingValue backingValue;
            public ConnectionType connectionType;
            public bool dynamicPortList;
            public TypeConstraint typeConstraint;

            public OutPutAttribute(ShowBackingValue backingValue = ShowBackingValue.Never, ConnectionType connectionType = ConnectionType.Multiple, TypeConstraint typeConstraint = TypeConstraint.None, bool dynamicPortList = false)
            {
                this.backingValue = backingValue;
                this.connectionType = connectionType;
                this.dynamicPortList = dynamicPortList;
                this.typeConstraint = typeConstraint;
            }
        }
        [AttributeUsage(AttributeTargets.Field)]
        public class ShowInEditorAttribute : Attribute
        {
           
        }
        #endregion
    }
}