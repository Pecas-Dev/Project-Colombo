using UnityEngine;


namespace ProjectColombo.StateMachine.GameManage
{
    public class GameStatePause : GameState
    {
        public GameStatePause(GameManagerStateMachine gameManager) : base(gameManager) 
        { 
        }

        public override void Enter()
        {
            m_gameManager.SetGameState(GameManagerStateMachine.GameStateType.Pause);
            Debug.Log("Entered Pause State");

            Time.timeScale = 0f;
        }

        public override void Tick(float deltaTime)
        {
        }

        public override void Exit()
        {
            Time.timeScale = 1f;
            Debug.Log("Exiting Pause State");
        }
    }
}
