using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wormNode;
public class DialogueNode : Node
{
    [Input] public string previouscontent;
    [OutPut] public string content;
    [ShowInEditor]public string currentContent;
    public override object GetValue(NodePort port)
    {
        content = currentContent;
        string a = GetInputValue<string>("previouscontent", this.previouscontent);
        if (port.FieldName == "content")
            return content;
        else if (port.FieldName == "previouscontent")
        {
            return a;
        }
        return null;

    }
}
