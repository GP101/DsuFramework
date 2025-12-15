/// @file   RuntimeGameDataManagerBase.cs
/// @date   20251215_jintaeks
using System.Collections.Generic;
using UnityEngine;

namespace Dsu.Framework
{
    public class RuntimeGameDataManagerBase : MonoBehaviour
    {
        // data stamp for each group
        private static Dictionary<int, int> _dataStamps = new Dictionary<int, int>();
        private static Dictionary<int, int> _actionDataStamps = new Dictionary<int, int>();

        // trace modified groups
        private static HashSet<int> _dirtyGroups = new HashSet<int>();

        public delegate void DataUpdatedAction(int groupId);
        public static event DataUpdatedAction OnDataUpdated;

        // default group ID
        private const int DefaultGroupId = 0;

        // for backward compatibility(uses Group ID 0)
        public static void RefreshData()
        {
            RefreshData(DefaultGroupId);
        }

        public static int GetDataStamp()
        {
            return GetDataStamp(DefaultGroupId);
        }

        protected static void _UpdateDataStamp()
        {
            _UpdateDataStamp(DefaultGroupId);
        }

        // update specific group ID
        public static void RefreshData(int groupId)
        {
            _UpdateDataStamp(groupId);
        }

        protected static void _UpdateDataStamp(int groupId)
        {
            if (!_dataStamps.ContainsKey(groupId))
                _dataStamps[groupId] = 0;

            _dataStamps[groupId]++;

            if (_dataStamps[groupId] <= 0)
                _dataStamps[groupId] = 1;

            _dirtyGroups.Add(groupId);
        }

        public static int GetDataStamp(int groupId)
        {
            if (!_dataStamps.ContainsKey(groupId))
                _dataStamps[groupId] = 0;

            return _dataStamps[groupId];
        }

        // process only modified group for each frame
        protected virtual void Update()
        {
            if (_dirtyGroups.Count == 0)
                return;

            foreach (int groupId in _dirtyGroups) {
                if (!_actionDataStamps.ContainsKey(groupId))
                    _actionDataStamps[groupId] = 0;

                if (_actionDataStamps[groupId] != _dataStamps[groupId]) {
                    _actionDataStamps[groupId] = _dataStamps[groupId];
                    OnDataUpdated?.Invoke(groupId);
                }
            }

            _dirtyGroups.Clear(); // clear dirty group when done
        }
    }
}
