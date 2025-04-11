using ProjectColombo.GameInputSystem;
using ProjectColombo.Combat;
using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerAttackState : PlayerBaseState
    {
        bool comboWindowOpened = false;
        GameGlobals.MusicScale myScale = GameGlobals.MusicScale.NONE;

        bool tooEarly = false;
        bool hitCombo = false;
        GameGlobals.MusicScale nextScale = GameGlobals.MusicScale.NONE;
        
        public PlayerAttackState(PlayerStateMachine playerStateMachine, GameGlobals.MusicScale scale) : base(playerStateMachine)
        {
            myScale = scale;
        }

        public override void Enter()
        {
            if (!stateMachine.myStamina.TryConsumeStamina(stateMachine.myStamina.staminaConfig.ComboStaminaCost))
            {
                Debug.Log("no stamina");
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
                return;
            }

            stateMachine.currentComboString += myScale == GameGlobals.MusicScale.MAJOR ? "M" : "m";
            Debug.Log(stateMachine.currentComboString);
            stateMachine.myWeaponAttributes.currentScale = myScale;
            stateMachine.comboWindowOpen = false;

            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Attack);

            stateMachine.gameInputSO.DisableAllInputsExcept(
                InputActionType.Roll, 
                InputActionType.Block, 
                InputActionType.MajorParry, 
                InputActionType.MinorParry,
                InputActionType.MajorAttack,
                InputActionType.MinorAttack);

            stateMachine.myPlayerAnimator.PlayAttackAnimation(stateMachine.currentComboString, 0.3f);

            Vector3 zeroVelocity = stateMachine.myRigidbody.linearVelocity;

            zeroVelocity.x = 0f;
            zeroVelocity.z = 0f;

            stateMachine.myRigidbody.linearVelocity = zeroVelocity;

            var targeter = stateMachine.myTargeter;

            ApplyAttackImpulse();
        }

        public override void Tick(float deltaTime)
        {
            HandleStateSwitchFromInput(); //extracted inputs to base state to remove repetition, 

            if (stateMachine.myPlayerAnimator.FinishedAttack())
            {
                stateMachine.currentComboString = "";
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
                return;
            }

            //check if the combo window has opened
            if (stateMachine.comboWindowOpen)
            {
                if (!comboWindowOpened) comboWindowOpened = true;
            }

            //if combo window is closed again and the conditions are right switch to next attack
            if (!stateMachine.comboWindowOpen && comboWindowOpened)
            {
                CheckComboSwitch();
            }


            //handle inputs
            if (stateMachine.gameInputSO.MajorAttackPressed)
            {
                if (!comboWindowOpened)
                {
                    tooEarly = true;
                    Debug.Log("hit too early");
                }

                stateMachine.gameInputSO.ResetMajorAttackPressed();
                hitCombo = true;
                nextScale = GameGlobals.MusicScale.MAJOR;
            }

            if (stateMachine.gameInputSO.MinorAttackPressed)
            {
                if (!comboWindowOpened)
                {
                    tooEarly = true;
                    Debug.Log("hit too early");
                }

                stateMachine.gameInputSO.ResetMinorAttackPressed();
                hitCombo = true;
                nextScale = GameGlobals.MusicScale.MINOR;
            }

            FaceLockedTarget(deltaTime);
            ApplyAttackImpulse();
        }

        public override void Exit()
        {
            stateMachine.myWeaponAttributes.currentScale = GameGlobals.MusicScale.NONE;
            stateMachine.gameInputSO.EnableInput(InputActionType.MajorAttack);
            stateMachine.gameInputSO.EnableInput(InputActionType.MinorAttack);
            stateMachine.gameInputSO.EnableAllInputs();
        }

        void CheckComboSwitch()
        {
            if (hitCombo && !tooEarly)
            {
                stateMachine.SwitchState(new PlayerAttackState(stateMachine, nextScale));
                return;
            }
        }

        void FaceLockedTarget(float deltaTime)
        {
            var targeter = stateMachine.myTargeter;

            if (targeter == null || !targeter.isTargetingActive || targeter.currentTarget == null)
            {
                return;
            }

            Vector3 toTarget = targeter.currentTarget.transform.position - stateMachine.myRigidbody.position;

            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            toTarget.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(toTarget);

            stateMachine.myRigidbody.rotation = Quaternion.RotateTowards(stateMachine.myRigidbody.rotation, targetRotation, stateMachine.myEntityAttributes.rotationSpeedPlayer * deltaTime);
        }

        Vector3 LookForClosestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemies.Length == 0)
            {
                return Vector3.zero;
            }

            Vector3 myPosition = stateMachine.transform.position;
            Vector3 closestPosition = myPosition;
            float closestDistance = Mathf.Infinity;

            foreach (GameObject e in enemies)
            {
                if ((e.transform.position - myPosition).magnitude < closestDistance)
                {
                    closestPosition = e.transform.position;
                    closestDistance = (e.transform.position - myPosition).magnitude;
                }
            }

            return closestPosition;
        }

        void ApplyAttackImpulse()
        {
            Vector3 targetPosition;
            var targeter = stateMachine.myTargeter;

            // These could come from weapon attributes
            WeaponAttributes playerWeapon = stateMachine.GetComponentInChildren<WeaponAttributes>();
            float activationDistance = playerWeapon.distanceToActivateForwardImpulse;   // player should be close already
            float maxDistance = playerWeapon.maxDistanceAfterImpulse;                   // furthest away
            float minDistance = playerWeapon.minDistanceAfterImpulse;                   // closest

            // Check if the player is targeting an enemy
            if (targeter != null && targeter.isTargetingActive && targeter.currentTarget != null)
            {
                targetPosition = targeter.currentTarget.transform.position;
                targetPosition.y = stateMachine.myRigidbody.position.y;
            }
            else
            {
                targetPosition = LookForClosestEnemy();

                if (targetPosition == Vector3.zero) return;

                targetPosition.y = stateMachine.myRigidbody.position.y;
            }

            //turn to target
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - stateMachine.transform.position);
            stateMachine.myRigidbody.rotation = targetRotation;

            // Calculate the direction to the target
            Vector3 directionToTarget = targetPosition - stateMachine.myRigidbody.position;
            directionToTarget.y = 0f;

            float distanceToTarget = directionToTarget.magnitude;

            // If the player is too far from the target, do nothing
            if (distanceToTarget > activationDistance) return;

            // Move towards the target if the distance is greater than the desired target distance
            if (distanceToTarget > maxDistance)
            {
                directionToTarget.Normalize();

                float speed = stateMachine.myEntityAttributes.attackImpulseForce;
                stateMachine.myRigidbody.MovePosition(stateMachine.myRigidbody.position + directionToTarget * speed * Time.deltaTime);
            }

            // Move away from the target if the player is too close
            else if (distanceToTarget < minDistance)
            {
                Vector3 directionAwayFromTarget = -directionToTarget;
                directionAwayFromTarget.Normalize();

                float retreatSpeed = stateMachine.myEntityAttributes.attackImpulseForce;
                stateMachine.myRigidbody.MovePosition(stateMachine.myRigidbody.position + directionAwayFromTarget * retreatSpeed * Time.deltaTime);
            }
        }
    }
}