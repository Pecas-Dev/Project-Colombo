using ProjectColombo.StateMachine.Player;
using System.Collections;
using UnityEngine;

public class PlayerRollState : PlayerBaseState
{
    int playerLayer = LayerMask.NameToLayer("Player");
    int weaponLayer = LayerMask.NameToLayer("Weapon");

    float rollCooldown = 0.125f;


    public static bool CanQueueRoll = true;

    public PlayerRollState(PlayerStateMachine stateMachine) : base(stateMachine) 
    { 
    
    }

    public override void Enter()
    {
        if (!CanQueueRoll)
        {
            stateMachine.SwitchState(new PlayerMovementState(stateMachine));
            return;
        }

        CanQueueRoll = false;
        stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Roll);
        SetIgnoreLayers();

        stateMachine.gameInputSO.DisableAllInputsExcept(ProjectColombo.GameInputSystem.InputActionType.Pause);
        stateMachine.myPlayerAnimator.TriggerRoll();


        float impulseForce = stateMachine.myEntityAttributes.rollImpulseForce;
        stateMachine.myRigidbody.AddForce(stateMachine.transform.forward * impulseForce, ForceMode.Impulse);
    }

    public override void Tick(float deltaTime)
    {
        if (!stateMachine.myPlayerAnimator.IsInRoll)
        {
            stateMachine.SwitchState(new PlayerMovementState(stateMachine));
        }
    }

    public override void Exit()
    {
        ResetIgnoreLayers();
        stateMachine.gameInputSO.EnableAllInputs();
        stateMachine.StartCoroutine(RollCooldown());
    }

    private IEnumerator RollCooldown()
    {
        yield return new WaitForSeconds(rollCooldown);
        CanQueueRoll = true;
    }

    private void SetIgnoreLayers()
    {
        Physics.IgnoreLayerCollision(playerLayer, weaponLayer, true);
    }

    private void ResetIgnoreLayers()
    {
        Physics.IgnoreLayerCollision(playerLayer, weaponLayer, false);
    }
}
