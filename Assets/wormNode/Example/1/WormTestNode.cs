using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wormNode;

public class WormTestNode :Node
{
    [Input(ShowBackingValue.Always, ConnectionType.Override)] public float a;
    [Input] public float b;


    [OutPut] public float result;
    [OutPut] public float sum;


	[ShowInEditor]public MathType mathType = MathType.Add;
	public enum MathType { Add, Subtract, Multiply, Divide }



    public override object GetValue(NodePort port)
    {
        float a = GetInputValue<float>("a", this.a);
        float b = GetInputValue<float>("b", this.b);
        if (port.FieldName == "result")
            switch (mathType)
            {
                case MathType.Add: default: return a + b;
                case MathType.Subtract: return a - b;
                case MathType.Multiply: return a * b;
                case MathType.Divide: return a / b;
            }
        else if (port.FieldName == "sum") return a + b;
        else if (port.FieldName == "a") return a;
        else if (port.FieldName == "b") return b;
        else return 0f;
    }
}

