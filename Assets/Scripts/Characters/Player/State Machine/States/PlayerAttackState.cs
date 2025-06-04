using ProjectColombo.GameInputSystem;
using ProjectColombo.Combat;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Collections;
using ProjectColombo.GameManagement.Events;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerAttackState : PlayerBaseState
    {
        bool comboWindowOpened = false;
        GameGlobals.MusicScale myScale = GameGlobals.MusicScale.NONE;

        //bool tooEarly = false; removed for now but might come back
        bool hitCombo = false;
        GameGlobals.MusicScale nextScale = GameGlobals.MusicScale.NONE;

        GameObject currentTarget;
        Vector3 targetPosition;

      
        public PlayerAttackState(PlayerStateMachine playerStateMachine, GameGlobals.MusicScale scale) : base(playerStateMachine)
        {
            myScale = scale;
        }

        public override void Enter()
        {
            //stateMachine.myStamina.TryConsumeStamina(stateMachine.myStamina.staminaToAttack);
            stateMachine.myStamina.TryConsumeStaminaWithFeedback(stateMachine.myStamina.staminaToAttack);
            Vector2 moveInput = stateMachine.gameInputSO.GetVector2Input(GameInputSystem.PlayerInputAction.Movement);

            stateMachine.myWeaponAttributes.currentScale = myScale;

            if (myScale == GameGlobals.MusicScale.MINOR)
            {
                CustomEvents.MinorAttackPerformed(myScale);
                Debug.Log("Minor attack performed - lightbar event triggered");
            }
            else if (myScale == GameGlobals.MusicScale.MAJOR)
            {
                CustomEvents.MajorAttackPerformed(myScale);
                Debug.Log("Major attack performed - lightbar event triggered");
            }

            //snap to direction
            if (moveInput.magnitude > 0.01f)
            {
                Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

                // Convert input to isometric space
                Vector3 isometricDirection = TransformDirectionToIsometric(moveDirection);

                if (isometricDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(isometricDirection);
                    stateMachine.myRigidbody.MoveRotation(targetRotation);
                }
            }

            stateMachine.currentComboString += myScale == GameGlobals.MusicScale.MAJOR ? "M" : "m";
            stateMachine.myWeaponAttributes.currentScale = myScale;
            stateMachine.comboWindowOpen = false;

            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Attack);

            stateMachine.gameInputSO.DisableAllInputs();
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.Movement);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.Block);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.Roll);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.MajorAttack);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.MinorAttack);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.MajorParry);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.MinorParry);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.Pause);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.UseCharmAbility);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.UsePotion);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.UseSpecialAbility);

            stateMachine.myPlayerAnimator.PlayAttackAnimation(stateMachine.currentComboString, 0.1f, stateMachine.myEntityAttributes.attackSpeed);

            Vector3 zeroVelocity = stateMachine.myRigidbody.linearVelocity;

            zeroVelocity.x = 0f;
            zeroVelocity.z = 0f;

            stateMachine.myRigidbody.linearVelocity = zeroVelocity;

            var targeter = stateMachine.myTargeter;

            if (targeter.HasManualTarget())
            {
                currentTarget = targeter.GetManualTarget(); // Manual lock-on takes priority
            }
            else
            {
                currentTarget = GetClosestTarget(); // Fallback to auto-target
            }

            targetPosition = currentTarget?.transform.position ??
                (stateMachine.transform.position + stateMachine.transform.forward * stateMachine.myWeaponAttributes.distanceToActivateForwardImpulse);
        }

        public override void Tick(float deltaTime)
        {
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
            if (stateMachine.gameInputSO.GetInputPressed(PlayerInputAction.MajorAttack))
            {
                //if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToAttack)) return;
                if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToAttack))
                {
                    stateMachine.myStamina.TryConsumeStaminaWithFeedback(stateMachine.myStamina.staminaToAttack);
                    return;
                }

                //if (!comboWindowOpened)
                //{
                //    tooEarly = true;
                //    Debug.Log("hit too early");
                //}

                hitCombo = true;
                nextScale = GameGlobals.MusicScale.MAJOR;
                return;
            }

            if (stateMachine.gameInputSO.GetInputPressed(PlayerInputAction.MinorAttack))
            {
                //if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToAttack)) return;
                if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToAttack))
                {
                    stateMachine.myStamina.TryConsumeStaminaWithFeedback(stateMachine.myStamina.staminaToAttack);
                    return;
                }

                //if (!comboWindowOpened)
                //{
                //    tooEarly = true;
                //    Debug.Log("hit too early");
                //}

                hitCombo = true;
                nextScale = GameGlobals.MusicScale.MINOR;
                return;
            }

            if (currentTarget != null)
            {
                targetPosition = currentTarget.transform.position;
            }
            else
            {
                targetPosition = stateMachine.transform.position + stateMachine.transform.forward;
            }

            //FaceLockedTarget(deltaTime);
            TurnToTarget();

            StopImpulseIfCloseEnough();
            HandleStateSwitchFromInput(); //extracted inputs to base state to remove repetition, 
        }

        public override void Exit()
        {
            //Debug.Log("exit attack state");
            stateMachine.myWeaponAttributes.currentScale = GameGlobals.MusicScale.NONE;

            if (stateMachine.comboWindowOpen)
            {
                stateMachine.CloseComboWindow();
            }

            stateMachine.myWeaponAttributes.DisableWeaponHitbox();

            stateMachine.gameInputSO.EnableAllInputs();
        }

        void CheckComboSwitch()
        {
            if (hitCombo)// && !tooEarly)
            {
                stateMachine.SwitchState(new PlayerAttackState(stateMachine, nextScale));
                return;
            }
        }

        GameObject GetClosestTarget()
        {
            Vector3 myPosition = stateMachine.transform.position;
            Vector3 myForward = stateMachine.transform.forward;
            float closestDistance = 5f; // max auto-lock range
            float maxViewAngle = 60f; // adjustable angle of view (half-angle, like a cone)
            GameObject closestTarget = null;

            // Helper function to check if within view
            bool IsInView(Vector3 targetPosition)
            {
                Vector3 directionToTarget = (targetPosition - myPosition).normalized;
                float angle = Vector3.Angle(myForward, directionToTarget);
                return angle <= maxViewAngle;
            }

            // 1. Check for enemies first
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(enemy.transform.position, myPosition);
                if (distance < closestDistance && IsInView(enemy.transform.position))
                {
                    closestTarget = enemy;
                    closestDistance = distance;
                }
            }

            // 2. If no enemy found in range, check destroyables
            if (closestTarget == null)
            {
                GameObject[] destroyables = GameObject.FindGameObjectsWithTag("Destroyable");

                foreach (GameObject obj in destroyables)
                {
                    float distance = Vector3.Distance(obj.transform.position, myPosition);
                    if (distance < closestDistance && IsInView(obj.transform.position))
                    {
                        closestTarget = obj;
                        closestDistance = distance;
                    }
                }
            }

            // 3. Return the closest valid target or null
            return closestTarget;
        }


        void TurnToTarget()
        {
            Vector3 direction = targetPosition - stateMachine.transform.position;
            direction.y = 0f; // Only consider horizontal direction

            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            stateMachine.myRigidbody.MoveRotation(Quaternion.RotateTowards(
                stateMachine.myRigidbody.rotation,
                targetRotation,
                stateMachine.myEntityAttributes.rotationSpeedPlayer * Time.deltaTime
            ));
        }



        public void OnAttackImpulse(float impulseForce)
        {
            stateMachine.StartCoroutine(ApplyAttackImpulse(impulseForce));
        }

        IEnumerator ApplyAttackImpulse(float impulseForce)
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            //reset velocities
            stateMachine.myRigidbody.linearVelocity = Vector3.zero;
            stateMachine.myRigidbody.angularVelocity = Vector3.zero;

            WeaponAttributes playerWeapon = stateMachine.myWeaponAttributes;
            float maxDistance = playerWeapon.maxDistanceAfterImpulse;
            float minDistance = playerWeapon.minDistanceAfterImpulse;

            Vector3 directionToTarget = targetPosition - stateMachine.myRigidbody.position;
            directionToTarget.y = 0f;

            float distanceToTarget = directionToTarget.magnitude;

            //float impulseForce = stateMachine.myEntityAttributes.attackImpulseForce;

            if (distanceToTarget > maxDistance)
            {
                directionToTarget.Normalize();
                stateMachine.myRigidbody.AddForce(directionToTarget * impulseForce, ForceMode.Impulse);
            }
            else if (distanceToTarget < minDistance)
            {
                directionToTarget.Normalize();
                stateMachine.myRigidbody.AddForce(-directionToTarget * impulseForce, ForceMode.Impulse);
            }
        }

        void StopImpulseIfCloseEnough()
        {
            if (currentTarget == null) return;

            float minDistance = stateMachine.myWeaponAttributes.minDistanceAfterImpulse;
            float maxDistance = stateMachine.myWeaponAttributes.maxDistanceAfterImpulse;
            Vector3 toTarget = currentTarget.transform.position - stateMachine.myRigidbody.position;
            toTarget.y = 0f;

            if (toTarget.magnitude > minDistance && toTarget.magnitude < maxDistance)
            {
                Vector3 velocity = stateMachine.myRigidbody.linearVelocity;
                velocity.x = 0f;
                velocity.z = 0f;
                stateMachine.myRigidbody.linearVelocity = velocity;
            }
        }


    }
}