using UnityEngine;


namespace ProjectColombo.StateMachine.GameManager
{
    public class GameStateGameOver : GameState
    {
        public GameStateGameOver(GameManagerStateMachine gameManager) : base(gameManager) 
        { 
        }

        public override void Enter()
        {
            m_gameManager.SetGameState(GameManagerStateMachine.GameStateType.GameOver);
            Debug.Log("Entered Game Over State");
        }

        public override void Tick(float deltaTime)
        {
        }

        public override void Exit()
        {
            Debug.Log("Exiting Game Over State");
        }
    }
}
