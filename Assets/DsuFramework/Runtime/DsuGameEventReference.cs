using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dsu.Framework
{
    [Serializable]
    public class DsuGameEventReference
    {
        [SerializeField]
        private DsuGameEvent _dsuEvent;
        [SerializeField]
        private UnityEvent<int, Transform, Transform> _response;

        public void RegisterEvent()
        {
            _dsuEvent.RegisterEvent(_response);
        }

        public void UnregisterEvent()
        {
            _dsuEvent.UnregisterEvent(_response);
        }

        public void RegisterAction(UnityAction<int, Transform, Transform> callback)
        {
            _dsuEvent.RegisterAction(callback);
        }

        public void UnregisterAction(UnityAction<int, Transform, Transform> callback)
        {
            _dsuEvent.UnregisterAction(callback);
        }
    }
}
