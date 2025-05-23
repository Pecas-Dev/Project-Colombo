using ProjectColombo.Combat;
using ProjectColombo.GameInputSystem;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using UnityEngine;
public class PlayerRollState : PlayerBaseState
{
    int playerLayer = LayerMask.NameToLayer("Player");
    int weaponLayer = LayerMask.NameToLayer("Weapon");
    int destroyableLayer = LayerMask.NameToLayer("Destroyable");

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

        CheckForDestroyablesDuringRoll();
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
        Physics.IgnoreLayerCollision(playerLayer, destroyableLayer, true);
        Physics.IgnoreLayerCollision(playerLayer, weaponLayer, true);
    }

    private void ResetIgnoreLayers()
    {
        Physics.IgnoreLayerCollision(playerLayer, destroyableLayer, false);
        Physics.IgnoreLayerCollision(playerLayer, weaponLayer, false);
    }

    private void CheckForDestroyablesDuringRoll()
    {
        float radius = 1.0f; // tweak based on character size
        Collider[] hits = Physics.OverlapSphere(stateMachine.transform.position, radius, LayerMask.GetMask("Destroyable"));

        foreach (Collider hit in hits)
        {
            GameObject go = hit.gameObject;
            if (go.CompareTag("Destroyable"))
            {
                HealthManager health = go.GetComponent<HealthManager>();
                if (health != null)
                {
                    health.TakeDamage(1000);
                    hit.enabled = false; // optional, prevents re-hitting
                }
            }
        }
    }

}
