using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassTypeName : PropertyAttribute
{
    public Type type;
    
    public ClassTypeName(Type type)
    {
        this.type = type;
    }
}
