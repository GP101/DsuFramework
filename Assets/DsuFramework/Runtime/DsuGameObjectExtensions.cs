/// @file   DsuGameObjectExtensions.cs
/// @date   20251215_jintaeks
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dsu.Framework
{
    public class GameObjectPropertyBase
    {
        public virtual void Reset() { }
    }

    [Serializable]
    public partial class GameObjectPropertyData : GameObjectPropertyBase
    {
        public bool isMovable; // sample property
    }//public partial class GameObjectPropertyData

    public static partial class DsuGameObjectExtensions
    {
        private static Dictionary<Transform, GameObjectProperty> _objectDictionary = new Dictionary<Transform, GameObjectProperty>();

        private static void _AddKey(GameObject go)
        {
            if (_objectDictionary.ContainsKey(go.transform) == false) {
                GameObjectProperty property = go.transform.GetComponent<GameObjectProperty>();
                if (property == null) {
                    //property = new GameObjectProperty();
                    property = go.AddComponent<GameObjectProperty>();
                }
                _objectDictionary.Add(go.transform, property);
            }
        }
        public static bool GetMovable(this GameObject go)
        {
            _AddKey(go);
            return _objectDictionary[go.transform].propertyData.isMovable;
        }
        public static void SetMovable(this GameObject go, bool isMovable)
        {
            _AddKey(go);
            _objectDictionary[go.transform].propertyData.isMovable = isMovable;
        }
        public static int GetDepth(this GameObject go, Transform ancestor)
        {
            int depth = 0;
            Transform temp = go.transform;

            while (temp != null) {
                if (temp == ancestor) {
                    return depth;
                }
                temp = temp.parent;
                depth++;
            }

            // ancestor was not found in the hierarchy
            return -1;
        }
        public static Transform GetAncestor(this GameObject go, int n)
        {
            if (go == null || n < 0)
                return null;

            Transform current = go.transform;
            for (int i = 0; i < n; i++) {
                if (current.parent == null)
                    return null;

                current = current.parent;
            }

            return current;
        }
        // return -1 if not found
        public static int IsAncestor(this GameObject go, Transform ancestor)
        {
            if (go == null || ancestor == null)
                return -1;

            int depth = 0;

            Transform current = go.transform;

            while (current != null) {
                if (current == ancestor)
                    return depth;

                depth += 1;
                current = current.parent;
            }

            return -1;
        }
    }//static partial class DsuGameObjectExtensions
}