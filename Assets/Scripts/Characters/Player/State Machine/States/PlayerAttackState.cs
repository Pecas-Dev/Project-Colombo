using ProjectColombo.GameInputSystem;
using ProjectColombo.Combat;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;


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
        
        public PlayerAttackState(PlayerStateMachine playerStateMachine, GameGlobals.MusicScale scale) : base(playerStateMachine)
        {
            myScale = scale;
        }

        public override void Enter()
        {
            if (!stateMachine.myStamina.TryConsumeStamina(stateMachine.myStamina.staminaToAttack))
            {
                Debug.Log("no stamina");
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
                return;
            }

            stateMachine.currentComboString += myScale == GameGlobals.MusicScale.MAJOR ? "M" : "m";
            Debug.Log(stateMachine.currentComboString);
            stateMachine.myWeaponAttributes.currentScale = myScale;
            stateMachine.myWeaponAttributes.Telegraphing();
            stateMachine.comboWindowOpen = false;

            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Attack);

            stateMachine.gameInputSO.DisableAllInputsExcept(
                InputActionType.Roll, 
                InputActionType.Block, 
                InputActionType.MajorParry, 
                InputActionType.MinorParry,
                InputActionType.MajorAttack,
                InputActionType.MinorAttack);

            stateMachine.myPlayerAnimator.PlayAttackAnimation(stateMachine.currentComboString, 0.1f, stateMachine.myEntityAttributes.attackSpeed);

            Vector3 zeroVelocity = stateMachine.myRigidbody.linearVelocity;

            zeroVelocity.x = 0f;
            zeroVelocity.z = 0f;

            stateMachine.myRigidbody.linearVelocity = zeroVelocity;

            var targeter = stateMachine.myTargeter;

            currentTarget = GetClosestTarget();
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
            if (stateMachine.gameInputSO.MajorAttackPressed)
            {
                //if (!comboWindowOpened)
                //{
                //    tooEarly = true;
                //    Debug.Log("hit too early");
                //}

                stateMachine.gameInputSO.ResetMajorAttackPressed();
                hitCombo = true;
                nextScale = GameGlobals.MusicScale.MAJOR;
                return;
            }

            if (stateMachine.gameInputSO.MinorAttackPressed)
            {
                //if (!comboWindowOpened)
                //{
                //    tooEarly = true;
                //    Debug.Log("hit too early");
                //}

                stateMachine.gameInputSO.ResetMinorAttackPressed();
                hitCombo = true;
                nextScale = GameGlobals.MusicScale.MINOR;
                return;
            }

            FaceLockedTarget(deltaTime);
            ApplyAttackImpulse();
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

        void FaceLockedTarget(float deltaTime)
        {
            Transform target = null;
            var targeter = stateMachine.myTargeter;

            if (targeter != null && targeter.isTargetingActive && targeter.currentTarget != null)
            {
                target = targeter.currentTarget.transform;
            }
            else if (currentTarget != null)
            {
                target = currentTarget.transform;
            }

            if (target == null) return;

            Vector3 toTarget = target.position - stateMachine.myRigidbody.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.0001f) return;

            toTarget.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(toTarget);

            stateMachine.myRigidbody.rotation = Quaternion.RotateTowards(
                stateMachine.myRigidbody.rotation,
                targetRotation,
                stateMachine.myEntityAttributes.rotationSpeedPlayer * deltaTime
            );
        }

        GameObject GetClosestTarget()
        {
            List<GameObject> enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
            enemies.AddRange(GameObject.FindGameObjectsWithTag("Destroyable")); //also lock to vases and stuff


            if (enemies.Count == 0)
            {
                return null;
            }

            Vector3 myPosition = stateMachine.transform.position;
            GameObject closestTarget = null;
            float closestDistance = 5; //adjust this number to make auto lock range

            foreach (GameObject e in enemies)
            {
                if ((e.transform.position - myPosition).magnitude < closestDistance)
                {
                    closestTarget = e;
                    closestDistance = (e.transform.position - myPosition).magnitude;
                }
            }

            return closestTarget;
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
            }
            else if (currentTarget != null)
            {
                targetPosition = currentTarget.transform.position;
            }
            else
            {
                return;
            }

            targetPosition.y = stateMachine.transform.position.y;
            //turn to target
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - stateMachine.transform.position);

            // Rotate to fit animation
            Quaternion offsetRotation = Quaternion.AngleAxis(-35f, Vector3.up);
            Quaternion finalRotation = offsetRotation * targetRotation;

            stateMachine.myRigidbody.rotation = finalRotation;

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