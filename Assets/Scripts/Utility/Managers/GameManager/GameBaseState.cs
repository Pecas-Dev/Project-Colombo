namespace ProjectColombo.StateMachine.GameManager
{
    public abstract class GameState : State
    {
        protected GameManagerStateMachine m_gameManager;

        public GameState(GameManagerStateMachine gameManager)
        {
            this.m_gameManager = gameManager;
        }
    }
}