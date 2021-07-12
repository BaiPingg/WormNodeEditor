using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wormNode;

public class StartNode : Node
{
    [OutPut(connectionType = ConnectionType.Override)] public object start;
    public override object GetValue(NodePort port)
    {
        return null;
    }
}
