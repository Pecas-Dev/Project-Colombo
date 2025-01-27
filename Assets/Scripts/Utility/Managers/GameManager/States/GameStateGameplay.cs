using UnityEngine;


namespace ProjectColombo.StateMachine.GameManager
{
    public class GameStateGameplay : GameState
    {
        public GameStateGameplay(GameManagerStateMachine gameManager) : base(gameManager) 
        { 
        }

        public override void Enter()
        {
            m_gameManager.SetGameState(GameManagerStateMachine.GameStateType.Gameplay);
            Debug.Log("Entered Gameplay State");
        }

        public override void Tick(float deltaTime)
        {
        }

        public override void Exit()
        {
            Debug.Log("Exiting Gameplay State");
        }
    }
}
