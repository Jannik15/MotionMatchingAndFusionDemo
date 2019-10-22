using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloatReference
{
    public bool UseUnique = false;
    public float UniqueValue;

    public FloatVariable Variable;

    public float value{
        get { return UseUnique ? UniqueValue : Variable.value;}

        set {
            if(UseUnique)
                UniqueValue = value;
            else
                Variable.value = value;
        }
    } /* => UseUnique ? UniqueValue : variable.value; */
}
