using System;

namespace Playground.Meyz.Core.KnaveFSMSystem.StateSystem.Interfaces
{
    /// <summary>
    /// Tek bir state --> state geçişini tanımlar
    /// </summary>
    public interface ITransition<TContext, TTag> where TTag : Enum
    {
        string Name { get; }
        TTag From { get; }
        TTag To { get; }
        bool CanUse(TContext context);
        void MarkUsed();
    }
}