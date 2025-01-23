using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateAttack : MommottiBaseState
    {
        Vector3 targetDirection;
        public MommottiStateAttack(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.ATTACK);
            Debug.Log("Mommotti entered Attack State");
        }

        public override void Tick(float deltaTime)
        {
            targetDirection = GetPlayerPosition() - stateMachine.transform.position;

            if (!stateMachine.myWeaponAttributes.attack && targetDirection.magnitude < stateMachine.myWeaponAttributes.reach)
            {
                stateMachine.myAnimator.SetTrigger("Attack");
                stateMachine.myWeaponAttributes.attack = true;
            }

            //rotate towards player
            if (Vector3.Angle(stateMachine.transform.forward, targetDirection.normalized) > 1f)
            {
                Quaternion startRotation = stateMachine.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
                stateMachine.myRigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, stateMachine.myEntityAttributes.rotationSpeedPlayer * deltaTime));
            }

            //move to player if to far away
            if (targetDirection.magnitude > stateMachine.myWeaponAttributes.reach)
            {
                float currentSpeed = stateMachine.myEntityAttributes.moveSpeed;
                Vector3 movingDirection = stateMachine.transform.forward;

                stateMachine.myRigidbody.MovePosition((stateMachine.transform.position + (currentSpeed * deltaTime * movingDirection)));
            }
        }

        public override void Exit()
        {
        }

        private Vector3 GetPlayerPosition()
        {
            return GameObject.Find("Player").transform.position;
        }

        public void Hit()
        {
            Debug.Log("Hit event triggered!");
            // Add logic for handling the hit, e.g., dealing damage
        }
    }
}