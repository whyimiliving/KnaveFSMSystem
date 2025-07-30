using System;
using System.Collections.Generic;
using Playground.Meyz.Core.KnaveFSMSystem.StateSystem.Interfaces;
using Playground.Meyz.Core.KnaveFSMSystem.Transitions;
using UnityEngine;

namespace Playground.Meyz.Core.KnaveFSMSystem.Core
{
    /// <summary>
    /// MonoBehaviour olarak attach edilebilen generic FSM controller.
    /// </summary>
    public abstract class KnaveMachineController<TContext, TTag> : MonoBehaviour
        where TContext : KnaveMachineController<TContext, TTag>
        where TTag : Enum
    {
        [Header("States")] [SerializeField] protected List<ScriptableObject> stateAssets;

        [Header("Initial State Tag")] [SerializeField]
        protected TTag initialTag;

        protected Dictionary<TTag, IState<TContext, TTag>> stateCache;
        protected IState<TContext, TTag> currentState;
        protected TransitionTable<TContext, TTag> transitionTable;

        protected abstract IEnumerable<ITransition<TContext, TTag>> CreateTransitions();

        protected virtual void Awake()
        {
            CacheStates();
            transitionTable = new TransitionTable<TContext, TTag>(CreateTransitions());
            ChangeState(initialTag, true);
        }

        private void CacheStates()
        {
            stateCache = new Dictionary<TTag, IState<TContext, TTag>>();
            foreach (var asset in stateAssets)
            {
                if (asset is IState<TContext, TTag> so)
                {
                    // ScriptableObject olduğu garanti değil, o yüzden instantiate’dan önce cast değil sonra cast:
                    var runtimeSo = Instantiate(asset) as ScriptableObject;
                    var state = runtimeSo as IState<TContext, TTag>;
                    state.Initialize((TContext)this);
                    stateCache[state.Tag] = state;
                }
                else
                {
                    Debug.LogWarning(
                        $"Atlanıyor: {asset.name} IState<{typeof(TContext).Name},{typeof(TTag).Name}> değil.");
                }
            }
        }

        protected void ChangeState(TTag to, bool invokeEnter = false, Action markUsed = null)
        {
            if (currentState != null && currentState.Tag.Equals(to)) return;
            currentState?.ExitState();
            if (stateCache.TryGetValue(to, out var next))
            {
                currentState = next;
                markUsed?.Invoke();
                if (invokeEnter) currentState.EnterState();
            }
            else
            {
                Debug.LogError($"State {to} cache’te yok!");
            }
        }

        public virtual void Update()
        {
            currentState?.UpdateState();
            var t = transitionTable.GetValidTransition((TContext)this, currentState.Tag);
            if (t != null) ChangeState(t.To, true, () => t.MarkUsed());
        }

        private void FixedUpdate() => currentState?.FixedUpdateState();

        public virtual void LateUpdate()
        {
            currentState?.LateUpdateState();
        }

        public void TriggerOnAnimationFinished()
        {
            currentState?.OnAnimationFinished();
        }

        private void OnDestroy()
        {
            foreach (var s in stateCache.Values) s.OnDestroyState();
            stateCache.Clear();
        }
    }
}