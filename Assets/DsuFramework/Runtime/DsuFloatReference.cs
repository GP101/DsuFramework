// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using System;

namespace Dsu.Framework
{
    [Serializable]
    public class DsuFloatReference
    {
        public bool UseConstant = true;
        public float ConstantValue;
        public DsuFloatVariable Variable;

        public DsuFloatReference()
        { }

        public DsuFloatReference(float value)
        {
            UseConstant = true;
            ConstantValue = value;
        }

        public float Value
        {
            get { return UseConstant ? ConstantValue : Variable.Value; }
        }

        public static implicit operator float(DsuFloatReference reference)
        {
            return reference.Value;
        }
    }
}