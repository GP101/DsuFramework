// Gameplay.cs
using System;
using UnityEngine;

namespace Dsu.Framework
{
    public class GameplayEvent
    {
        public int Id { get; private set; }

        public GameplayEvent(object id_)
        {
            Id = (int)id_;
        }

        public override string ToString() => Id.ToString();
        public static readonly GameplayEvent NullGameplayEvent = new GameplayEvent(0);
    }//class GameplayEvent

    public abstract class GameplayEventArgsBase : EventArgs
    {
        public GameplayEvent Event { get; private set; }

        protected GameplayEventArgsBase(GameplayEvent evt)
        {
            Event = evt;
        }
    }//class GameplayEventArgsBase

    public class GameplayEventArgs : GameplayEventArgsBase
    {
        public GameplayEventArgs(GameplayEvent evt) : base(evt) { }
    }//class GameplayEventArgs

    public class CollisionEventArgs : GameplayEventArgsBase
    {
        public GameObject Source { get; private set; }
        public string SourceType { get; private set; }
        public GameObject Target { get; private set; }
        public string TargetType { get; private set; }

        public CollisionEventArgs(GameplayEvent evt, GameObject source, string sourceType, GameObject target, string targetType)
            : base(evt)
        {
            Source = source;
            SourceType = sourceType;
            Target = target;
            TargetType = targetType;
        }
    }//class CollisionEventArgs

    public delegate void GameplayEventHandler(object sender, GameplayEventArgsBase args);
}//namespace Dsu.Framework
