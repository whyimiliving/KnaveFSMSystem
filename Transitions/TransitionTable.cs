using System;
using System.Collections.Generic;
using System.Linq;
using Playground.Meyz.Core.KnaveFSMSystem.StateSystem.Interfaces;

namespace Playground.Meyz.Core.KnaveFSMSystem.Transitions
{
    /// <summary>
    /// Önceden sıralanmış ve GC'yi minimize eden TransitionTable.
    /// </summary>
    public class TransitionTable<TContext, TTag> where TTag : Enum
    {
        Dictionary<TTag, ITransition<TContext, TTag>[]> lookup;
        ITransition<TContext, TTag>[] globalTransitions;

        public TransitionTable(IEnumerable<ITransition<TContext, TTag>> transitions)
        {
            var perState = new Dictionary<TTag, List<ITransition<TContext, TTag>>>();
            var globals = new List<ITransition<TContext, TTag>>();

            foreach (var t in transitions)
            {
                if (t.From.Equals(default(TTag)))
                    globals.Add(t);
                else
                {
                    if (!perState.TryGetValue(t.From, out var list))
                        perState[t.From] = list = new List<ITransition<TContext, TTag>>();
                    list.Add(t);
                }
            }
            
            lookup = perState.ToDictionary(
                kv => kv.Key,
                kv => kv.Value
                    .OrderByDescending(x => ((Transition<TContext, TTag>)x).Priority)
                    .ToArray()
            );
            
            globalTransitions = globals
                .OrderByDescending(x => ((Transition<TContext, TTag>)x).Priority)
                .ToArray();
        }

        public ITransition<TContext, TTag> GetValidTransition(
            TContext context,
            TTag currentTag
        )
        {

            if (lookup.TryGetValue(currentTag, out var list))
                foreach (var t in list)
                    if (t.CanUse(context))
                        return t;
            
            foreach (var t in globalTransitions)
                if (t.CanUse(context))
                    return t;

            return null;
        }
    }
}