using System.Collections.Generic;
using UnityEngine;

namespace Dsu.Framework
{
    public class RuntimeGameDataManagerBase : MonoBehaviour
    {
        // 그룹별 데이터 스탬프
        private static Dictionary<int, int> _dataStamps = new Dictionary<int, int>();
        private static Dictionary<int, int> _actionDataStamps = new Dictionary<int, int>();

        // 변경된 그룹만 추적
        private static HashSet<int> _dirtyGroups = new HashSet<int>();

        public delegate void DataUpdatedAction(int groupId);
        public static event DataUpdatedAction OnDataUpdated;

        // 기본 그룹 상수
        private const int DefaultGroupId = 0;

        // 하위 호환용 (기본 그룹 0 사용)
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

        // 그룹별 메서드
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

        // 매 프레임 변경된 그룹만 처리
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

            _dirtyGroups.Clear(); // 처리 완료 후 초기화
        }
    }
}
