using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wormNode;
public class SelectNode : Node
{
    [Input] public Object objIn;  
   [OutPut] public Object objOut;

    public override object GetValue(NodePort port)
    {
        return base.GetValue(port);
    }
}
