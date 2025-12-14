using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dsu.Framework
{
    public class DsuGameplayManagerBase : MonoBehaviour
    {
        public void RegisterEvent(ref GameplayEventHandler eventOwner)
        {
            eventOwner += OnGameplayEvent;
        }
        public void UnregisterEvent(ref GameplayEventHandler eventOwner)
        {
            eventOwner -= OnGameplayEvent;
        }

        public virtual void OnGameplayEvent(object sender, GameplayEventArgsBase args)
        {
            Debug.LogError($"DO NOT CALL: GameplayEvent: {args.Event} from {sender}");
        }
    }
}