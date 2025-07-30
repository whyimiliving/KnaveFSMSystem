namespace Playground.Meyz.Core.KnaveFSMSystem.Core
{
    public enum ControllerStates
    {
        None, 
        Idle, 
        Sprinting, 
        AimWalk,
        AimIdle,
        Walk, 
        Dodge, 
        Fall
    }
    
    public enum CombatStates
    {
        None,
        Aiming,
        Shooting
    }

} 