using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateChase : MommottiBaseState
    {
        Vector3 targetDirection;
        public MommottiStateChase(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            targetDirection = GetPlayerPosition() - stateMachine.transform.position;
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.CHASE);
            Debug.Log("Mommotti entered Chase State");
        }

        public override void Tick(float deltaTime)
        {
            targetDirection = GetPlayerPosition() - stateMachine.transform.position;

            if (targetDirection.magnitude < stateMachine.myWeaponAttributes.reach)
            {
                stateMachine.SwitchState(new MommottiStateAttack(stateMachine));
            }

            //rotate towards player
            if (Vector3.Angle(stateMachine.transform.forward, targetDirection.normalized) > 1f)
            {
                Quaternion startRotation = stateMachine.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
                stateMachine.myRigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, stateMachine.myEntityAttributes.rotationSpeedPlayer * deltaTime));
            }

            //adjust speed depending on distance -> fast when far away, slow when close
            float speedFactor = 1 + targetDirection.magnitude / stateMachine.myMommottiAttributes.circleDistance;
            float currentSpeed = stateMachine.myEntityAttributes.moveSpeed * speedFactor;
            Vector3 movingDirection = stateMachine.transform.forward; //generally move forward

            if (targetDirection.magnitude < stateMachine.myMommottiAttributes.circleDistance) //if too close and not attacking go back
            {
                movingDirection = -stateMachine.transform.forward; 
            }
            else if (targetDirection.magnitude < stateMachine.myMommottiAttributes.circleDistance + stateMachine.myMommottiAttributes.circleTolerance) //if in tolerance zone circle
            {
                currentSpeed += 0.5f;
                movingDirection = stateMachine.transform.right;
            }

            stateMachine.myRigidbody.MovePosition((stateMachine.transform.position + (currentSpeed * deltaTime * movingDirection)));
        }

        public override void Exit()
        {
        }

        private Vector3 GetPlayerPosition()
        {
            return GameObject.Find("Player").transform.position;
        }
    }
}