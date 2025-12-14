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
    public class DsuIntReference
    {
        public bool UseConstant = true;
        public int ConstantValue;
        public DsuIntVariable Variable;

        public DsuIntReference()
        { }

        public DsuIntReference(int value)
        {
            UseConstant = true;
            ConstantValue = value;
        }

        public int Value
        {
            get { return UseConstant ? ConstantValue : Variable.Value; }
        }

        public static implicit operator int(DsuIntReference reference)
        {
            return reference.Value;
        }
    }
}