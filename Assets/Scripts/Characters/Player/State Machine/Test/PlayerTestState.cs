using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerTestState : PlayerBaseState
    {
        float timer = 0f;

        public PlayerTestState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
        }

        public override void Enter()
        {
            m_playerStateMachine.GameInput.TargetPressedEvent += OnTargetPressed;
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;

            Debug.Log(timer);
        }

        public override void Exit()
        {
            m_playerStateMachine.GameInput.TargetPressedEvent -= OnTargetPressed;
        }

        void OnTargetPressed()
        {
            m_playerStateMachine.SwitchState(new PlayerTestState(m_playerStateMachine));
        }

    }
}
