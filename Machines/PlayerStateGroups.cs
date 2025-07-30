using Playground.Meyz.Core.KnaveFSMSystem.Core;

namespace Playground.Meyz.Core.KnaveFSMSystem.Machines
{
    public static class PlayerStateGroups
    {
        // Dodge’a geçiş yapılabilecek tüm from-state’leri
        public static readonly ControllerStates[] DodgeFromStates =
        {
            ControllerStates.Idle,
            ControllerStates.Walk,
            ControllerStates.AimIdle,
            ControllerStates.AimWalk,
            ControllerStates.Sprinting
        };

        // Dodge’tan sonra aynı koşulla dönülecek hedef state’ler
        public static readonly ControllerStates[] PostDodgeStates =
        {
            ControllerStates.Idle,
            ControllerStates.Walk,
            ControllerStates.AimIdle
        };

        // Idle/Walk → AimWalk girişi
        public static readonly ControllerStates[] EntryAimWalkStates =
        {
            ControllerStates.Idle,
            ControllerStates.Walk
        };
    }
}