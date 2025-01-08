using ProjectColombo.Control;
using ProjectColombo.Input;

using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerStateMachine : StateMachine
    {
        [field: SerializeField] public GameInput GameInput { get; private set; }
        [field: SerializeField] public Rigidbody PlayerRigidbody { get; private set; }
        [field: SerializeField] public PlayerAnimator PlayerAnimator { get; private set; }
        [field: SerializeField] public EntityAttributes EntityAttributes { get; private set; }


        void Awake()
        {
            PlayerRigidbody = GetComponent<Rigidbody>();
            PlayerAnimator = GetComponent<PlayerAnimator>();
            EntityAttributes = GetComponent<EntityAttributes>();
        }

        void Start()
        {
            LogMissingReferenceErrors();
              
            SwitchState(new PlayerMovementState(this));
        }

        void LogMissingReferenceErrors()
        {
            if (GameInput == null)
            {
                Debug.LogError("GameInput reference is missing!");
            }

            if (PlayerRigidbody == null)
            {
                Debug.LogError("PlayerRigidbody reference is missing!");
            }

            if (PlayerAnimator == null)
            {
                Debug.LogError("PlayerAnimator reference is missing!");
            }

            if (EntityAttributes == null)
            {
                Debug.LogError("EntityAttributes reference is missing!");
            }
        }
    }
}

