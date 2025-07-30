using UnityEngine;

namespace Playground.Meyz.Core.KnaveFSMSystem.Core
{
    [System.Serializable]
    public class PlayerLocomotion
    {
        [Tooltip("Rotate edeceğimiz karakterin Transform’u")]
        public Transform characterTransform;

        [Tooltip("İzometrik kameran")]
        public Camera mainCamera;

        [Tooltip("Character Controller referansı")]
        public CharacterController characterController;
        
        [Tooltip("MovementData referansı")]
        [SerializeField] private MovementData movementData;

        [Tooltip("Yerçekimi kuvveti (negatif)")]
        public float gravity = -9.81f;

        [Tooltip("Yere bastığında aşağı sabitleme değeri")]
        public float groundedOffset = -0.5f;

        [Tooltip("Zıplama kuvveti")]
        public float jumpForce = 5f;

        [Tooltip("Zemin kontrolü (sadece info amaçlı)")]
        public bool isGrounded;

        [Tooltip("Hareketin ivmelenme katsayısı")]
        public float acceleration = 20f;

        private float currentSpeed;
        
        public Transform aimTarget;       
        public LayerMask aimLayerMask;
        private float rotationSpeed = 10f;

        public float CurrentSpeed => currentSpeed;
        private float aimOffset;
        
        public Vector3 direction;
        private float verticalVelocity = 0f;

        /// <summary>
        /// Karakteri fare işaretçisinin yere izdüşümü yönüne anında çevirir.
        /// İzometrik (ortho ya da perspective) kameralarla uyumludur.
        /// </summary>
        public void RotateToMouse()
        {
            Camera cam = mainCamera != null ? mainCamera : Camera.main;
            if (cam == null || characterTransform == null) return;

            Ray ray = cam.ScreenPointToRay(UnityEngine.Input.mousePosition);
            Plane plane = new Plane(Vector3.up, characterTransform.position);

            if (!plane.Raycast(ray, out float enter)) return;

            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 dir = hitPoint - characterTransform.position;
            dir.y = 0f;

            float minDistance = 2.5f;
            if (dir.sqrMagnitude < minDistance * minDistance)
                return;

            direction = dir;

            // Smooth rotation
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            characterTransform.rotation = Quaternion.Slerp(
                characterTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        /// <summary>
        /// Her frame çağrılmalı. Gravity uygular.
        /// </summary>
        public void ApplyGravity()
        {
            if (characterController == null) return;

            isGrounded = characterController.isGrounded;

            if (isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedOffset;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 gravityMove = new Vector3(0f, verticalVelocity, 0f);
            characterController.Move(gravityMove * Time.deltaTime);
        }
        
        /// <summary>
        /// Hedef hıza doğru yumuşak geçiş yapar ve sonucu döner.
        /// </summary>
        public float CalculateSmoothedSpeed(float targetSpeed, float acceleration)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);
            movementData.currentSpeed = currentSpeed;
            return currentSpeed;
        }
        
        /// <summary>
        /// Aim Targetin pozisyonunu fareye sabitler.
        /// </summary>
        
       public void UpdateAimTarget()
        {
            var (success, position) = GetMousePosition();
            if (success)
            {
                
                var direction = position;
                direction.y += aimOffset;
                aimTarget.position = direction;
            }
        }
        
        
        private (bool success, Vector3 position) GetMousePosition()
        {
            var ray = mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
            {
                if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    aimOffset = 1f;
                }
                else
                {
                    aimOffset = 0f;
                }
                
                return (success: true, position: hitInfo.point);
            }
            else
            {

                return (success: false, position: Vector3.zero);
            }
        }

    }
}
