// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Dsu.Framework
{
    [CustomEditor(typeof(DsuGameEvent), editorForChildClasses: true)]
    public class DsuGameEventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            DsuGameEvent e = target as DsuGameEvent;
            if (GUILayout.Button("Raise"))
                e.Raise();
        }
    }
}
