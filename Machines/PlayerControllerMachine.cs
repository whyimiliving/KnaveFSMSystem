using System.Collections.Generic;
using Playground.Meyz.Core.KnaveFSMSystem.Core;
using Playground.Meyz.Core.KnaveFSMSystem.KnaveDebug;
using Playground.Meyz.Core.KnaveFSMSystem.StateSystem.Interfaces;
using Playground.Meyz.Core.KnaveFSMSystem.Transitions;
using Playground.Meyz.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Playground.Meyz.Core.KnaveFSMSystem.Machines
{
    [RequireComponent(typeof(KnaveMachineDebugger))]
    [AddComponentMenu("Knave/Knave Player Controller")]
    public class PlayerControllerMachine
        : KnaveMachineController<PlayerControllerMachine, ControllerStates>
    {
        [Header("Dependencies")] [SerializeField]
        private InputData inputData;

        [SerializeField] private MovementData movementData;
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController chController;
        [SerializeField] private Transform playerCamera;
        [SerializeField] private Transform bow;

        [Space] [Space] [Header("IK Constraints")] [SerializeField]
        private MultiAimConstraint aimHand;

        [SerializeField] private MultiAimConstraint aimHead;
        [SerializeField] private MultiAimConstraint aimArm;
        [SerializeField] private MultiAimConstraint aimSpine;

        [Space] [Space] [Header("Locomotion Settings")] [SerializeField]
        private PlayerLocomotion playerLocomotion;

        // Private 

        // Public properties
        public InputData InputData => inputData;
        public MovementData MovementData => movementData;
        public Animator Animator => animator;
        public CharacterController ChController => chController;
        public Transform PlayerCamera => playerCamera;
        public PlayerLocomotion PlayerLocomotion => playerLocomotion;
        public MultiAimConstraint AimHand => aimHand;
        public MultiAimConstraint AimHead => aimHead;
        public MultiAimConstraint AimSpine => aimSpine;
        public MultiAimConstraint AimArm => aimArm;
        public Transform bowTransform => bow;


        [HideInInspector] public bool IsDodgeFinished;

        protected override IEnumerable<ITransition<PlayerControllerMachine, ControllerStates>> CreateTransitions()
        {
            // Idle ↔ Walk
            yield return new Transition<PlayerControllerMachine, ControllerStates>(
                "Idle → Walk", ControllerStates.Idle, ControllerStates.Walk,
                ctx => ctx.InputData.InputVector.magnitude > 0.1f, priority: 1);
            
            yield return new Transition<PlayerControllerMachine, ControllerStates>(
                "Walk → Idle", ControllerStates.Walk, ControllerStates.Idle,
                ctx => ctx.InputData.InputVector.magnitude <= 0.1f, priority: 1);

            // 2) Walk ↔ Sprint
            yield return new Transition<PlayerControllerMachine, ControllerStates>(
                "Walk → Sprint", ControllerStates.Walk, ControllerStates.Sprinting,
                ctx => ctx.InputData.running && ctx.InputData.InputVector.sqrMagnitude > 0.1f, priority: 2);
            
            yield return new Transition<PlayerControllerMachine, ControllerStates>(
                "Sprint → Walk", ControllerStates.Sprinting, ControllerStates.Walk,
                ctx => !ctx.InputData.running || ctx.InputData.InputVector.sqrMagnitude < 0.1f, priority: 2);

            // Idle/Walk → AimWalk
            foreach (var from in PlayerStateGroups.EntryAimWalkStates)
            {
                yield return new Transition<PlayerControllerMachine, ControllerStates>(
                    $"{from} → AimWalk",
                    from,
                    ControllerStates.AimWalk,
                    ctx => ctx.InputData.GoAiming,
                    priority: 2
                );
            }

            // AimWalk → Walk / Idle
            yield return new Transition<PlayerControllerMachine, ControllerStates>(
                "AimWalk → Walk", ControllerStates.AimWalk, ControllerStates.Walk,
                ctx => !ctx.InputData.GoAiming && !ctx.Animator.GetBool("isReloading"), priority: 2);
            
            yield return new Transition<PlayerControllerMachine, ControllerStates>(
                "AimWalk → Idle", ControllerStates.AimWalk, ControllerStates.Idle,
                ctx => !ctx.InputData.GoAiming, priority: 2);

            //  AimWalk ↔ AimIdle
            yield return new Transition<PlayerControllerMachine, ControllerStates>(
                "AimWalk → AimIdle", ControllerStates.AimWalk, ControllerStates.AimIdle,
                ctx => ctx.InputData.GoAiming && ctx.InputData.InputVector.magnitude < 0.1f, priority: 3);
            
            yield return new Transition<PlayerControllerMachine, ControllerStates>(
                "AimIdle → AimWalk", ControllerStates.AimIdle, ControllerStates.AimWalk,
                ctx => ctx.InputData.GoAiming && ctx.InputData.InputVector.magnitude > 0.1f, priority: 3);

            // AimIdle → Idle
            yield return new Transition<PlayerControllerMachine, ControllerStates>(
                "AimIdle → Idle", ControllerStates.AimIdle, ControllerStates.Idle,
                ctx => !ctx.InputData.GoAiming, priority: 3);

            // Any → Dodge
            foreach (var from in PlayerStateGroups.DodgeFromStates)
            {
                yield return new Transition<PlayerControllerMachine, ControllerStates>(
                    $"{from} → Dodge",
                    from,
                    ControllerStates.Dodge,
                    ctx => ctx.InputData.ConsumeRoll(),
                    priority: 4
                );
            }

            // 8) Dodge → Idle/Walk/AimIdle
            foreach (var to in PlayerStateGroups.PostDodgeStates)
            {
                yield return new Transition<PlayerControllerMachine, ControllerStates>(
                    $"Dodge → {to}",
                    ControllerStates.Dodge,
                    to,
                    ctx => ctx.IsDodgeFinished,
                    priority: 4
                );
            }

            // 9) Dodge → Sprint
            yield return new Transition<PlayerControllerMachine, ControllerStates>(
                "Dodge → Sprint",
                ControllerStates.Dodge,
                ControllerStates.Sprinting,
                ctx => ctx.IsDodgeFinished
                       && ctx.InputData.running
                       && ctx.InputData.InputVector.sqrMagnitude > 0.1f,
                priority: 4
            );
        }


        public override void Update()
        {
            playerLocomotion.ApplyGravity();
            playerLocomotion.UpdateAimTarget();

            base.Update();
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                MovementData.isLoaded = false;
            }
        }

        public override void LateUpdate()
        {
            inputData.ResetFrameInputs();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying && currentState != null)
            {
                Handles.Label(transform.position + Vector3.up * 7f,
                    $"[Knave] State: {currentState.Tag} \n Speed {movementData.currentSpeed} \n IsLoaded: {MovementData.isLoaded}",
                    new GUIStyle { normal = new GUIStyleState { textColor = Color.cyan } });
            }
        }
#endif
    }
}