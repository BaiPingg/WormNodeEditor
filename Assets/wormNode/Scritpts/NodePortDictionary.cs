using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wormNode
{
    [System.Serializable]
    public class NodePortDictionary : Dictionary<string, NodePort>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<string> keys = new List<string>();
        [SerializeField] private List<NodePort> values = new List<NodePort>();

        /// <summary>
        /// 拆解（序列化）
        /// </summary>
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<string, NodePort> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }
        /// <summary>
        /// 反向组装（反序列化）
        /// </summary>
        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception("there are " + keys.Count + " keys and " + values.Count + " values after deserialization. Make sure that both key and value types are serializable.");

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }
}