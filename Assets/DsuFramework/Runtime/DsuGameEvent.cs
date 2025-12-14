using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Dsu.Framework
{
    [CreateAssetMenu(fileName = "DsuGameEvent", menuName = "Dsu/DsuGameEvent", order = 1)]
    public class DsuGameEvent : ScriptableObject
    {
        /// <summary>
        /// The list of listeners that this event will notify if it is raised.
        /// </summary>
        private readonly List<DsuGameEventListener> eventListeners = new List<DsuGameEventListener>();
        private readonly List<UnityEvent<int, Transform, Transform>> unityEventList = new List<UnityEvent<int, Transform, Transform>>();
        private readonly List<UnityAction<int, Transform, Transform>> unityActionList = new List<UnityAction<int, Transform, Transform>>();

        public void Raise(int iParam, Transform first, Transform second)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventRaised(iParam, first, second);
            for (int i = unityEventList.Count - 1; i >= 0; i--)
                unityEventList[i].Invoke(iParam, first, second);
            for (int i = unityActionList.Count - 1; i >= 0; i--)
                unityActionList[i].Invoke(iParam, first, second);
        }

        public void Raise(int iParam)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventRaised(iParam, null, null);
            for (int i = unityEventList.Count - 1; i >= 0; i--)
                unityEventList[i].Invoke(iParam, null, null);
            for (int i = unityActionList.Count - 1; i >= 0; i--)
                unityActionList[i].Invoke(iParam, null, null);
        }

        public void Raise()
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventRaised(0, null, null);
            for (int i = unityEventList.Count - 1; i >= 0; i--)
                unityEventList[i].Invoke(0, null, null);
            for (int i = unityActionList.Count - 1; i >= 0; i--)
                unityActionList[i].Invoke(0, null, null);
        }

        public void RegisterListener(DsuGameEventListener listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        public void UnregisterListener(DsuGameEventListener listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }

        public void RegisterEvent(UnityEvent<int, Transform, Transform> callback)
        {
            if (!unityEventList.Contains(callback))
                unityEventList.Add(callback);
        }

        public void UnregisterEvent(UnityEvent<int, Transform, Transform> callback)
        {
            if (unityEventList.Contains(callback))
                unityEventList.Remove(callback);
        }

        public void RegisterAction(UnityAction<int, Transform, Transform> callback)
        {
            if (!unityActionList.Contains(callback))
                unityActionList.Add(callback);
        }

        public void UnregisterAction(UnityAction<int, Transform, Transform> callback)
        {
            if (unityActionList.Contains(callback))
                unityActionList.Remove(callback);
        }
    }
}
