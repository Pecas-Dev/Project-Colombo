using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public abstract class PlayerBaseState : State
    {
        protected PlayerStateMachine stateMachine;

        public PlayerBaseState(PlayerStateMachine playerStateMachine)
        {
            this.stateMachine = playerStateMachine;
        }

        protected bool IsGrounded()
        {
            //float rayLength = 0.2f;
            //Vector3 rayOrigin = m_playerStateMachine.PlayerRigidbody.position + Vector3.up * 0.1f;

            //return Physics.Raycast(rayOrigin, Vector3.down, rayLength);

            float rayLength = 0.2f;

            Vector3 rayOrigin = stateMachine.myRigidbody.position + Vector3.up * 0.1f;

            bool centerGrounded = Physics.Raycast(rayOrigin, Vector3.down, rayLength);

            Vector3 forward = stateMachine.myRigidbody.transform.forward * 0.3f;
            Vector3 right = stateMachine.myRigidbody.transform.right * 0.3f;

            bool forwardGrounded = Physics.Raycast(rayOrigin + forward, Vector3.down, rayLength);
            bool backGrounded = Physics.Raycast(rayOrigin - forward, Vector3.down, rayLength);
            bool rightGrounded = Physics.Raycast(rayOrigin + right, Vector3.down, rayLength);
            bool leftGrounded = Physics.Raycast(rayOrigin - right, Vector3.down, rayLength);

            return centerGrounded || forwardGrounded || backGrounded || rightGrounded || leftGrounded;
        }

        protected Vector3 PreventWallStick(Vector3 velocity, float deltaTime)
        {
            if (stateMachine.isInRoll)
            {
                return velocity;
            }

            Vector3 horizontalVel = new Vector3(velocity.x, 0f, velocity.z);

            float speed = horizontalVel.magnitude;

            if (speed < 0.001f)
            {
                return velocity;
            }

            CapsuleCollider capsule = stateMachine.myRigidbody.GetComponent<CapsuleCollider>();

            if (capsule == null)
            {
                return velocity;
            }

            float radius = capsule.radius * 0.95f;
            float midHeight = (capsule.height * 0.5f) - capsule.radius;

            Vector3 castOrigin = stateMachine.myRigidbody.position + Vector3.up * midHeight;
            Vector3 dir = horizontalVel.normalized;

            float castDistance = speed * deltaTime + 0.2f;

            if (Physics.SphereCast(castOrigin, radius, dir, out RaycastHit hit, castDistance))
            {
                float upDot = Vector3.Dot(hit.normal, Vector3.up);

                bool isWallLike = upDot < 0.7f;

                if (isWallLike)
                {
                    Vector3 projectedVelocity = Vector3.ProjectOnPlane(velocity, hit.normal);

                    float gravityFactor = 0.95f; //0.9 - 1.0 
                    if (velocity.y < 0)
                    {
                        projectedVelocity.y = Mathf.Min(projectedVelocity.y, velocity.y * gravityFactor);
                    }

                    return projectedVelocity;
                }
            }

            return velocity;
        }

        protected void ApplyAirPhysics(float deltaTime)
        {
            Rigidbody rigidbody = stateMachine.myRigidbody;

            Vector3 velocity = rigidbody.linearVelocity;

            if (!IsGrounded())
            {
                velocity = PreventWallStick(velocity, deltaTime);

                velocity.y += Physics.gravity.y * deltaTime;

                velocity.y = Mathf.Max(velocity.y, -20f);
            }

            rigidbody.linearVelocity = velocity;
        }

        protected void HandleAirPhysicsIfNeeded(float deltaTime)
        {
            if (!IsGrounded())
            {
                ApplyAirPhysics(deltaTime);
            }
        }

        protected void HandleStateSwitchFromInput()
        {
            //set attack states
            if (stateMachine.gameInputSO.MajorAttackPressed)
            {
                stateMachine.gameInputSO.ResetMajorAttackPressed();
                stateMachine.SwitchState(new PlayerAttackState(stateMachine, 0));
                return;
            }

            if (stateMachine.gameInputSO.MinorAttackPressed)
            {
                stateMachine.gameInputSO.ResetMinorAttackPressed();
                stateMachine.SwitchState(new PlayerAttackState(stateMachine, 0));
                return;
            }

            //set defense states
            if (stateMachine.gameInputSO.RollPressed)
            {
                stateMachine.gameInputSO.ResetRollPressed();
                stateMachine.SwitchState(new PlayerRollState(stateMachine));
                return;
            }

            if (stateMachine.gameInputSO.BlockPressed)
            {
                stateMachine.SwitchState(new PlayerBlockState(stateMachine));
                return;
            }

            //set parry states
            if (stateMachine.gameInputSO.MajorParryPressed)
            {
                stateMachine.gameInputSO.ResetMajorParryPressed();
                stateMachine.SwitchState(new PlayerParryState(stateMachine, GameGlobals.MusicScale.MAJOR));
                return;
            }

            if (stateMachine.gameInputSO.MinorParryPressed)
            {
                stateMachine.gameInputSO.ResetMinorParryPressed();
                stateMachine.SwitchState(new PlayerParryState(stateMachine, GameGlobals.MusicScale.MINOR));
                return;
            }
        }
    }
}

