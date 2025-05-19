using ProjectColombo.StateMachine.Player;
using System.Collections;
using UnityEngine;
using ProjectColombo.GameInputSystem;
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

        stateMachine.myStamina.TryConsumeStamina(stateMachine.myStamina.staminaToRoll);

        //snap to direction
        if (stateMachine.gameInputSO.MovementInput.magnitude > 0.01f)
        {
            Vector2 moveInput = stateMachine.gameInputSO.MovementInput;
            Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            // Convert input to isometric space
            Vector3 isometricDirection = TransformDirectionToIsometric(moveDirection);

            if (isometricDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(isometricDirection);
                stateMachine.myRigidbody.rotation = targetRotation;
            }
        }

        stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Roll);
        SetIgnoreLayers();

        stateMachine.gameInputSO.DisableAllInputsExcept(InputActionType.Pause, InputActionType.Movement);
        stateMachine.myPlayerAnimator.TriggerRoll();

        stateMachine.StartCoroutine(ApplyPushForce());
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

        if (stateMachine.gameInputSO.MovementInput.magnitude < 0.01f)
        {
            stateMachine.gameInputSO.ResetMovementInput();
        }

        stateMachine.StartCoroutine(RollCooldown());
    }

    private IEnumerator RollCooldown()
    {
        yield return new WaitForSeconds(rollCooldown);
        CanQueueRoll = true;
    }

    IEnumerator ApplyPushForce()
    {
        yield return new WaitForFixedUpdate();

        //reset velocities
        stateMachine.myRigidbody.linearVelocity = Vector3.zero;
        stateMachine.myRigidbody.angularVelocity = Vector3.zero;

        // Apply clean impulse for the roll
        float impulseForce = stateMachine.myEntityAttributes.rollImpulseForce;
        stateMachine.myRigidbody.AddForce(stateMachine.transform.forward * impulseForce, ForceMode.Impulse);
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
