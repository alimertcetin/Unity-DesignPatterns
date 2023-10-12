using UnityEngine;

namespace XIV.DesignPatterns.Common.PlayerMovement
{
    public class PlayerMovementHandler : MonoBehaviour
    {
        [SerializeField] CharacterController controller;
        [SerializeField] float moveSpeed = 8f;
        [SerializeField] float sprintSpeed = 10f;
        [SerializeField] float speedChangeRate = 10f;
        [SerializeField] float groundedOffset = -0.15f;
        [SerializeField] float GroundedRadius = 0.5f;
        [SerializeField] LayerMask GroundLayers;
        [SerializeField] float fallTimeout = 0.15f;
        [SerializeField] float jumpTimeout = 0.1f;
        [SerializeField] float jumpHeight = 1.2f;
        [SerializeField] float gravity = -15f;
        
        Vector3 velocity;
        Vector3 previousPosition;
        bool grounded;
        float verticalVelocity;
        float fallTimeoutDelta;
        float jumpTimeoutDelta;
        const float MAX_VERTICAL_VELOCITY = 50f;

        void Update()
        {
            GroundedCheck();
            JumpAndGravity();
            Move();
        }

        void GroundedCheck()
        {
            // set sphere position, with offset
            var position = transform.position;
            Vector3 spherePosition = new Vector3(position.x, position.y - groundedOffset, position.z);
            grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        void JumpAndGravity()
        {
            if (grounded)
            {
                // reset the fall timeout timer
                fallTimeoutDelta = fallTimeout;
                // stop our velocity dropping infinitely when grounded
                if (verticalVelocity < 0.0f)
                {
                    verticalVelocity = -2f;
                }

                // Jump
                if (Input.GetKey(KeyCode.Space) && jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }

                // jump timeout
                if (jumpTimeoutDelta >= 0.0f)
                {
                    jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                jumpTimeoutDelta = jumpTimeout;

                // fall timeout
                if (fallTimeoutDelta >= 0.0f)
                {
                    fallTimeoutDelta -= Time.deltaTime;
                }

                // // if we are not grounded, do not jump
                // _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (verticalVelocity < MAX_VERTICAL_VELOCITY)
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }

        void Move()
        {
            CalculateVelocity();

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
            var horizontalInput = Input.GetAxisRaw("Horizontal");
            var verticalInput = Input.GetAxisRaw("Vertical");
            var movementInput = new Vector2(horizontalInput, verticalInput);
            
            if (movementInput == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(velocity.x, 0.0f, velocity.z).magnitude;

            const float SPEED_OFFSET = 0.1f;
            float inputMagnitude = movementInput.magnitude;

            float speed;
            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - SPEED_OFFSET || currentHorizontalSpeed > targetSpeed + SPEED_OFFSET)
            {
                speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);

                // round speed to 3 decimal places
                speed = Mathf.Round(speed * 1000f) / 1000f;
            }
            else
            {
                speed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(movementInput.x, 0.0f, movementInput.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (movementInput != Vector2.zero)
            {
                // move
                inputDirection = transform.right * movementInput.x + transform.forward * movementInput.y;
            }

            // move the player
            var xzMovement = inputDirection.normalized * speed;
            var yMovement = new Vector3(0.0f, verticalVelocity, 0.0f);
            var targetPosition = xzMovement + yMovement;
            controller.Move(targetPosition * Time.deltaTime);
        }

        void CalculateVelocity()
        {
            var currentPosition = transform.position;
            velocity = currentPosition - previousPosition;
            previousPosition = currentPosition;
        }
    }
}
