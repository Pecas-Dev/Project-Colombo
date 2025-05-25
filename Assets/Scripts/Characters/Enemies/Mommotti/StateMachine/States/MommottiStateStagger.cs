using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateStagger : MommottiBaseState
    {
        float timer;
        public MommottiStateStagger(MommottiStateMachine stateMachine) : base(stateMachine)
        {
            stateMachine.currentState = MommottiStateMachine.MommottiState.STUNNED;
        }

        public override void Enter()
        {
            stateMachine.myAnimator.SetBool("Impact", true);
            CustomEvents.OnDamageDelt += ExtraDamageOnStagger;
        }

        private void ExtraDamageOnStagger(int amount, GameGlobals.MusicScale scale, bool sameScale, Combat.HealthManager healthManager, int combo)
        {
            if (healthManager.gameObject == stateMachine.gameObject)
            {
                int extraDamage = Mathf.RoundToInt(amount * stateMachine.myMommottiAttributes.extraDamageWhenStaggeredPercent / 100f);
                stateMachine.myHealthManager.TakeDamage(extraDamage);
            }
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
            CustomEvents.OnDamageDelt -= ExtraDamageOnStagger;
        }
    }
}