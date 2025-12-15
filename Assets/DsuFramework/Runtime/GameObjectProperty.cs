/// @file   GameObjectProperty.cs
/// @date   20251215_jintaeks
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dsu.Framework
{
    public class GameObjectProperty : MonoBehaviour
    {
        public GameObjectPropertyData propertyData = new GameObjectPropertyData();

        public void Reset()
        {
            propertyData.Reset();
        }
    }
}