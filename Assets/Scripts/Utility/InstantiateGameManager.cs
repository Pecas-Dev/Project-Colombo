using UnityEngine;

public class InstantiateGameManager : MonoBehaviour
{
    [SerializeField] GameObject gameManager;

    void Awake()
    {
        Instantiate(gameManager);
    }
}
