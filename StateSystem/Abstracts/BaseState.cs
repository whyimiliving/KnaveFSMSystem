using System;
using Playground.Meyz.Core.KnaveFSMSystem.StateSystem.Interfaces;
using UnityEngine;

namespace Playground.Meyz.Core.KnaveFSMSystem.StateSystem.Abstracts
{
    /// <summary>
    /// ScriptableObject tabanlı, tekrar kullanılabilir base state.
    /// </summary>
    public abstract class BaseState<TContext, TTag> : ScriptableObject, IState<TContext, TTag> where TTag : Enum
    {
        protected TContext Context;
        public abstract TTag Tag { get; }

        public virtual void Initialize(TContext context)
        {
            Context = context;
        }

        public virtual void EnterState()
        {
        }

        public virtual void ExitState()
        {
        }

        public virtual void UpdateState()
        {
        }

        public virtual void FixedUpdateState()
        {
        }

        public virtual void OnDestroyState()
        {
        }

        public virtual void OnAnimationFinished()
        {
        }

        public virtual void LateUpdateState()
        {
        }
    }
}