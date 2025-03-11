using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.StateMachine.Player;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateStagger : MommottiBaseState
    {
        Vector3 alertedPosition;
        float timer;
        public MommottiStateStagger(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            Color skinColor = new(.3f, .3f, .3f);
            stateMachine.myColorfullSkin.material.color = skinColor;

            stateMachine.myAnimator.SetBool("Impact", true);
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;

            if (timer >= stateMachine.myEntityAttributes.stunnedTime)
            {
                stateMachine.SwitchState(new MommottiStateAttack(stateMachine));
            }
        }

        public override void Exit()
        {
            stateMachine.myAnimator.SetBool("Impact", false);
        }
    }
}