using System;

namespace Playground.Meyz.Core.KnaveFSMSystem.StateSystem.Interfaces
{
    /// <summary>
    /// TContext: State’in çalışacağı controller tipi
    /// TTag:    State tag’lerini ifade eden enum tipi
    /// </summary>
    public interface IState<TContext, TTag> where TTag : Enum
    {
        TTag Tag { get; }
        void Initialize(TContext context);
        void EnterState();
        void ExitState();
        void UpdateState();
        void FixedUpdateState();
        void OnDestroyState();
        void LateUpdateState();
        void OnAnimationFinished();
    }
}