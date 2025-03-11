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
        ShopKeeper shopKeeper;

        public PlayerShopState(PlayerStateMachine playerStateMachine, ShopKeeper shop) : base(playerStateMachine)
        {
            shopKeeper = shop;
        }

        public override void Enter()
        {
            m_playerStateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Shop);

            Currency playerInventory = m_playerStateMachine.GetComponentInChildren<Currency>();

            if (playerInventory.visible)
            {
                playerInventory.ToggleVisability();
            }
        }

        public override void Tick(float deltaTime)
        {
            Vector3 targetDirection = shopKeeper.transform.position - m_playerStateMachine.transform.position;

            //rotate towards shopkeeper
            if (Vector3.Angle(m_playerStateMachine.transform.forward, targetDirection.normalized) > 1f)
            {
                Quaternion startRotation = m_playerStateMachine.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
                m_playerStateMachine.PlayerRigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, m_playerStateMachine.EntityAttributes.rotationSpeedPlayer * deltaTime));
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