using UnityEngine;

namespace Playground.Meyz.Core.KnaveFSMSystem.Core
{
    [CreateAssetMenu(fileName = "MovementData", menuName = "Knave/Data/Movement Data")]
    public class MovementData : ScriptableObject
    {
        [Header("Hareket Parametreleri")]
        public float currentSpeed;
        public float movementSpeed;
        public float aimSpeed;
        public float runSpeed;
        public float acceleration;
        public float dodgeSpeed;
        public float dodgeDuration;
        public float dodgeDistance;
        public Vector3 movementDirection;
    
        [Header("Zemin Parametreleri")]
        public float drag;
        public float playerHeight;
        public LayerMask groundLayer;
        public bool isLoaded;
    
        [Header("Kamera Parametreleri")]
        public float xSensitivity;
        public float ySensitivity;
    
    }
}