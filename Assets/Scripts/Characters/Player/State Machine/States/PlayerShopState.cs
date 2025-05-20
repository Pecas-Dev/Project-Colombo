using ProjectColombo.GameInputSystem;
using ProjectColombo.Combat;

using UnityEngine;
using ProjectColombo.UI.HUD;
using ProjectColombo.Inventory;
using ProjectColombo.Shop;

namespace ProjectColombo.StateMachine.Player
{
    public class PlayerShopState : PlayerBaseState
    {
        Vector3 shopKeeper;

        public PlayerShopState(PlayerStateMachine playerStateMachine, GameObject shop) : base(playerStateMachine)
        {
            shopKeeper = shop.transform.position;
        }

        public override void Enter()
        {
            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Shop);
            stateMachine.myRigidbody.linearVelocity = Vector3.zero;
            stateMachine.myPlayerAnimator.UpdateAnimator(0, false, false);
            stateMachine.myPlayerAnimator.PlayMovementAnimation();
            Debug.Log("player entered shop state");

            //InventoryHUD playerInventory = stateMachine.GetComponentInChildren<InventoryHUD>();

            //if (playerInventory.visible)
            //{
            //    playerInventory.ToggleVisability();
            //}
        }

        public override void Tick(float deltaTime)
        {
            Vector3 targetDirection = shopKeeper - stateMachine.transform.position;
            targetDirection.y = 0; 

            if (Vector3.Dot(stateMachine.transform.forward, targetDirection.normalized) < 0.95f)
            {
                Quaternion startRotation = stateMachine.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);

                stateMachine.myRigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, stateMachine.myEntityAttributes.rotationSpeedPlayer));
            }
        }



        public override void Exit()
        {
            stateMachine.gameInputSO.EnableAllInputs();

            if (stateMachine.gameInputSO.MovementInput.magnitude < 0.01f)
            {
                stateMachine.gameInputSO.ResetMovementInput();
            }
            //InventoryHUD playerInventory = stateMachine.GetComponentInChildren<InventoryHUD>();

            //if (!playerInventory.visible)
            //{
            //    playerInventory.ToggleVisability();
            //}
        }
    }
}