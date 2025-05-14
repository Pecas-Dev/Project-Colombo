using ProjectColombo.Enemies.Mommotti;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateAlerted : MommottiBaseState
    {
        Vector3 alertedPosition;
        float timer;
        public MommottiStateAlerted(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            timer = 0;
            alertedPosition = stateMachine.myMommottiAttributes.GetPlayerPosition();
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.ALERTED);
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.myMommottiAttributes.FieldOfViewCheck() || stateMachine.myMommottiAttributes.SoundDetectionCheck())
            {
                alertedPosition = stateMachine.myMommottiAttributes.GetPlayerPosition();

                if (timer > stateMachine.myMommottiAttributes.alertedBufferTime)
                {
                    GameObject[] allMommotti = GameObject.FindGameObjectsWithTag("Enemy");

                    foreach (GameObject m in allMommotti)
                    {
                        var mStateMachine = m.GetComponent<MommottiStateMachine>();
                        if (mStateMachine == null) continue;
                        if (m == stateMachine.gameObject) continue;
                        
                        if ((m.transform.position - stateMachine.transform.position).magnitude < stateMachine.myMommottiAttributes.areaToAlertOthers)
                        {
                            mStateMachine.SwitchState(new MommottiStateChase(mStateMachine));
                        }
                    }

                    stateMachine.SwitchState(new MommottiStateChase(stateMachine));
                }
            }
            else if (timer > stateMachine.myMommottiAttributes.alertedBufferTime)
            {
                stateMachine.SwitchState(new MommottiStatePatrol(stateMachine));
            }

            RotateTowardsTarget(alertedPosition, deltaTime, stateMachine.myEntityAttributes.rotationSpeedPlayer);

            Vector3 targetDirection = (alertedPosition - stateMachine.transform.position).normalized;
            targetDirection.y = 0;

            timer += deltaTime;
        }

        public override void Exit()
        {
        }
    }
}