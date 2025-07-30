using System.Collections;
using System.Reflection;
using Playground.Meyz.Core.KnaveFSMSystem.Core;
using Playground.Meyz.Core.KnaveFSMSystem.Machines;
using Playground.Meyz.Core.KnaveFSMSystem.StateSystem.Interfaces;
using Playground.Meyz.Core.KnaveFSMSystem.Transitions;
using UnityEngine;

namespace Playground.Meyz.Core.KnaveFSMSystem.KnaveDebug
{
    [RequireComponent(typeof(PlayerControllerMachine))]
    public class KnaveMachineDebugger : MonoBehaviour
    {
        private PlayerControllerMachine machine;
        private bool showDebug = false;
        private Vector2 scroll;

        private FieldInfo stateCacheField;
        private FieldInfo currentStateField;
        private FieldInfo transitionTableField;

        private void Awake()
        {
            machine = GetComponent<PlayerControllerMachine>();
            // BaseType artık KnaveMachineController<PlayerControllerMachine,ControllerStates>
            var baseType = machine.GetType().BaseType;
            stateCacheField      = baseType.GetField("stateCache",       BindingFlags.NonPublic | BindingFlags.Instance);
            currentStateField    = baseType.GetField("currentState",     BindingFlags.NonPublic | BindingFlags.Instance);
            transitionTableField = baseType.GetField("transitionTable",  BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F1))
                showDebug = !showDebug;
        }

        private void OnGUI()
        {
            if (!showDebug) return;

            GUILayout.BeginArea(new Rect(10, 10, 500, Screen.height - 20), GUI.skin.box);
            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.Label("<b>FSM DEBUG</b>", GetRichLabel(18));
            GUILayout.Space(5);

            // — Mevcut State —
            var currentObj = currentStateField.GetValue(machine);
            if (currentObj is IState<PlayerControllerMachine, ControllerStates> current)
                GUILayout.Label($"Current State: <color=cyan>{current.Tag}</color>", GetRichLabel(14));
            else
                GUILayout.Label("Current State: <color=red>NULL</color>", GetRichLabel(14));

            GUILayout.Space(8);

            // — State Cache —
            GUILayout.Label("<b>All States in Cache:</b>", GetRichLabel(12));
            var cache = stateCacheField.GetValue(machine) as IDictionary;
            foreach (DictionaryEntry kv in cache)
                GUILayout.Label($"• [{kv.Key}] → {kv.Value.GetType().Name}");

            GUILayout.Space(8);

            // — Transition Table —
            GUILayout.Label("<b>Transitions:</b>", GetRichLabel(12));
            var ttObj = transitionTableField.GetValue(machine);
            var lookupField = ttObj.GetType().GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance);
            var lookup = lookupField.GetValue(ttObj) as IDictionary;
            foreach (DictionaryEntry entry in lookup)
            {
                GUILayout.Label($"→ From <b>{entry.Key}</b>:", GetRichLabel(11));
                foreach (var item in (IEnumerable)entry.Value)
                {
                    if (item is ITransition<PlayerControllerMachine, ControllerStates> trans)
                    {
                        var concrete = item as Transition<PlayerControllerMachine, ControllerStates>;
                        string blocked = concrete.IsBlocked ? " (Blocked)" : "";
                        GUILayout.Label($"   - {trans.Name}: {trans.From} → {trans.To} [Prio:{concrete.Priority}]{blocked}");
                    }
                }
            }

            GUILayout.Space(8);

            // — Next Valid Transition —
            if (currentObj is IState<PlayerControllerMachine, ControllerStates> curState)
            {
                var tt = transitionTableField.GetValue(machine) as TransitionTable<PlayerControllerMachine, ControllerStates>;
                var next = tt.GetValidTransition(machine, curState.Tag);
                if (next != null)
                    GUILayout.Label($"Next Valid: <color=yellow>{next.Name}</color> → {next.To}", GetRichLabel(13));
                else
                    GUILayout.Label("Next Valid: <color=grey>None</color>", GetRichLabel(13));
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private GUIStyle GetRichLabel(int size)
        {
            return new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = size,
                wordWrap = true
            };
        }
    }
}
