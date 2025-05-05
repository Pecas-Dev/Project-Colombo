using UnityEngine;


namespace ProjectColombo.UI
{
    public class UIManagerInitializer : MonoBehaviour
    {
        [Header("UI Manager Setup")]
        [SerializeField] GameObject uiManagerPrefab;

        void Awake()
        {
            InitializeUIManager();
        }

        void InitializeUIManager()
        {
            if (UIManager.Instance == null)
            {
                if (uiManagerPrefab != null)
                {
                    GameObject uiManagerObject = Instantiate(uiManagerPrefab);
                    uiManagerObject.name = "UIManager";
                }
                else
                {
                    GameObject uiManagerObject = new GameObject("UIManager");
                    uiManagerObject.AddComponent<UIManager>();
                }
            }
        }
    }
}
