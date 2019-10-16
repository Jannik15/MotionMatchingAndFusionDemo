using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloatReference
{
    public bool UseConstant = true;
    public float ConstantValue;

    public FloatVariable variable;

    public float value 
    {
        get
        {
            if (!UseConstant) return variable.value;
            if (ConstantValue.Equals(0))
                return ConstantValue = variable.value;
            return ConstantValue;
        }
    }
}
