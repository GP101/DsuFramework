using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dsu.Framework;

namespace Dsu.Framework
{
    public partial class GameObjectPropertyData
    {
        //public int customProperty;

        //public override void Reset()
        //{
        //    isMovable = false;
        //    customProperty = 0;
        //}
        public int counter;
    }

    public static partial class DsuGameObjectExtensions
    {
        //public static int CustomProperty(this GameObject go)
        //{
        //    _AddKey(go);
        //    return _objectDictionary[go.transform].propertyData.customProperty;
        //}
        public static int Counter(this GameObject go)
        {
            _AddKey(go);
            return _objectDictionary[go.transform].propertyData.counter;
        }
    }
}