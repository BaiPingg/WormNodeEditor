using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wormNode
{
    [Serializable]
    public class Graph : ScriptableObject
    {
        [SerializeField]
        public List<Node> nodes= new List<Node>();

        public T AddNode<T>() where T : Node
        {
            return AddNode(typeof(T)) as T;
        }

        public void AddNode(Node node)
        {
            node.graph = this;
            nodes.Add(node);
        }
        public virtual Node AddNode(Type type)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.graph = this;
            nodes.Add(node);
            return node;
        }


        public virtual Node CopyNode(Node original)
        {
           
            Node node = ScriptableObject.Instantiate(original);
            node.graph = this;
            node.ClearConnections();
            nodes.Add(node);
            return node;
        }

        public virtual void RemoveNode(Node node)
        {
            node.ClearConnections();
            nodes.Remove(node);
            if (Application.isPlaying) Destroy(node);
        }


        public virtual void Clear()
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    Destroy(nodes[i]);
                }
            }
            nodes.Clear();
        }

        public virtual Graph Copy()
        {
              Graph graph = Instantiate(this);
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null) continue;
                Node node = Instantiate(nodes[i]) as Node;
                node.graph = graph;
                graph.nodes[i] = node;
            }


            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i] == null) continue;
                foreach (NodePort port in graph.nodes[i].Ports)
                {
                    port.Redirect(nodes, graph.nodes);
                }
            }

            return graph;
        }

        protected virtual void OnDestroy()
        {
            Clear();
        }


    }
}