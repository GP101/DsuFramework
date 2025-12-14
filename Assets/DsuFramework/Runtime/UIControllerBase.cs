using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Dsu.Framework
{
    public class UIControllerBase : MonoBehaviour
    {
        private int _uiDataStamp = 0;


        protected virtual private void OnEnable()
        {
            RuntimeGameDataManagerBase.OnDataUpdated += OnDataUpdate;
        }

        protected virtual void OnDisable()
        {
            RuntimeGameDataManagerBase.OnDataUpdated -= OnDataUpdate;
        }

        private void OnDataUpdate(int groupId)
        {
            // observer pattern
            int uiDataStamp = RuntimeGameDataManagerBase.GetDataStamp();
            if (uiDataStamp != _uiDataStamp) {
                _uiDataStamp = uiDataStamp; // update UI data stamp
                UpdateData(groupId);
            }
        }

        protected virtual void UpdateData(int groupId)
        {
        }
    }//class UIControllerBase
}