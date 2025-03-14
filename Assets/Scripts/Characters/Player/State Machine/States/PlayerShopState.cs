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
            m_playerStateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Shop);
            Debug.Log("player entered shop state");

            Currency playerInventory = m_playerStateMachine.GetComponentInChildren<Currency>();

            if (playerInventory.visible)
            {
                playerInventory.ToggleVisability();
            }
        }

        public override void Tick(float deltaTime)
        {
            Vector3 targetDirection = shopKeeper - m_playerStateMachine.transform.position;
            targetDirection.y = 0; 

            if (Vector3.Dot(m_playerStateMachine.transform.forward, targetDirection.normalized) < 0.95f)
            {
                Quaternion startRotation = m_playerStateMachine.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);

                m_playerStateMachine.PlayerRigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, m_playerStateMachine.EntityAttributes.rotationSpeedPlayer));
            }
        }



        public override void Exit()
        {
            Currency playerInventory = m_playerStateMachine.GetComponentInChildren<Currency>();

            if (!playerInventory.visible)
            {
                playerInventory.ToggleVisability();
            }
        }
    }
}