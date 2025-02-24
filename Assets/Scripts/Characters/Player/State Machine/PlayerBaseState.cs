using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public abstract class PlayerBaseState : State
    {
        protected PlayerStateMachine m_playerStateMachine;

        public PlayerBaseState(PlayerStateMachine playerStateMachine)
        {
            this.m_playerStateMachine = playerStateMachine;
        }

        protected void ApplyAirPhysics(float deltaTime)
        {
            Rigidbody rigidbody = m_playerStateMachine.PlayerRigidbody;

            Vector3 velocity = rigidbody.linearVelocity;

            if (!IsGrounded())
            {
                velocity = PreventWallStick(velocity, deltaTime);
                velocity.y += Physics.gravity.y + deltaTime;
            }

            rigidbody.linearVelocity = velocity;
        }

        protected bool IsGrounded()
        {
            float rayLength = 0.2f;
            Vector3 rayOrigin = m_playerStateMachine.PlayerRigidbody.position + Vector3.up * 0.1f;

            return Physics.Raycast(rayOrigin, Vector3.down, rayLength);
        }

        protected Vector3 PreventWallStick(Vector3 velocity, float deltaTime)
        {
            Vector3 horizontalVel = new Vector3(velocity.x, 0f, velocity.z);

            float speed = horizontalVel.magnitude;

            if (speed < 0.001f)
            {
                return velocity; 
            }

            float castDistance = speed * deltaTime + 0.2f; 

            CapsuleCollider capsule = m_playerStateMachine.PlayerRigidbody.GetComponent<CapsuleCollider>();

            if (capsule == null)
            {
                return velocity;
            }

            float radius = capsule.radius * 0.95f;
            float midHeight = (capsule.height * 0.5f) - capsule.radius;

            Vector3 castOrigin = m_playerStateMachine.PlayerRigidbody.position + Vector3.up * midHeight;
            Vector3 dir = horizontalVel.normalized;

            if (Physics.SphereCast(castOrigin, radius, dir, out RaycastHit hit, castDistance))
            {
                float upDot = Vector3.Dot(hit.normal, Vector3.up);

                bool isWallLike = upDot < 0.6f; 

                if (isWallLike)
                {
                    velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
                }
            }

            return velocity;
        }
    }
}

