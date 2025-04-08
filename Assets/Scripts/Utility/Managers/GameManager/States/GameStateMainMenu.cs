using UnityEngine;
using UnityEngine.SceneManagement;


namespace ProjectColombo.StateMachine.GameManage
{
    public class GameStateMainMenu : GameState
    {
        public GameStateMainMenu(GameManagerStateMachine gameManager) : base(gameManager) 
        { 
        }

        public override void Enter()
        {
            m_gameManager.SetGameState(GameManagerStateMachine.GameStateType.MainMenu);

            SceneManager.LoadScene("2_MainMenu");

            Debug.Log("Entered Main Menu State");
        }

        public override void Tick(float deltaTime)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                SceneManager.LoadScene("PrototypePlayer");
                m_gameManager.SwitchState(new GameStateGameplay(m_gameManager));
            }
        }

        public override void Exit()
        {
            Debug.Log("Exiting Main Menu State");
        }
    }
}
