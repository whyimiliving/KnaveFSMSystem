using System;
using Playground.Meyz.Core.KnaveFSMSystem.StateSystem.Interfaces;
using UnityEngine;

namespace Playground.Meyz.Core.KnaveFSMSystem.Transitions
{
    [Serializable]
    public class Transition<TContext, TTag> : ITransition<TContext, TTag>
        where TTag : Enum
    {
        public Func<TContext,bool> Condition { get; private set; }
        public string Name      { get; private set; }
        public TTag   From      { get; private set; }
        public TTag   To        { get; private set; }
        public int    Priority  { get; private set; }
        public float  Cooldown  { get; private set; }
        public bool   IsBlocked { get; private set; }

        private float lastUsedTime = -999f;

        public Transition(
            string name,
            TTag from,
            TTag to,
            Func<TContext,bool> condition,
            int priority   = 0,
            float cooldown = 0f,
            bool isBlocked = false
        )
        {
            Name      = name;
            From      = from;
            To        = to;
            Condition = condition;
            Priority  = priority;
            Cooldown  = cooldown;
            IsBlocked = isBlocked;
        }

        public bool CanUse(TContext context)
        {
            if (IsBlocked) return false;
            if (Time.time < lastUsedTime + Cooldown) return false;
            return Condition(context);
        }

        public void MarkUsed() => lastUsedTime = Time.time;
    }
}