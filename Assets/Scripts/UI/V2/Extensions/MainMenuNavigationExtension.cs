using UnityEngine;
using UnityEngine.UI;


namespace ProjectColombo.UI
{
    public class MainMenuNavigationExtension : MonoBehaviour
    {
        [Header("First Selectable Object")]
        [SerializeField] GameObject firstSelectedObject;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        MainMenuController mainMenuController;
        UINavigationManager navigationManager;

        void Awake()
        {
            mainMenuController = GetComponent<MainMenuController>();

            navigationManager = FindFirstObjectByType<UINavigationManager>();

            if (navigationManager == null)
            {
                GameObject navigationManagerObj = new GameObject("UINavigationManager");
                navigationManager = navigationManagerObj.AddComponent<UINavigationManager>();
                LogDebug("Created UINavigationManager");
            }
        }

        void Start()
        {
            FindFirstSelectableIfNeeded();

            if (navigationManager != null && firstSelectedObject != null)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.MainMenu, firstSelectedObject);
            }
        }

        void OnEnable()
        {
            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.MainMenu);
                LogDebug("Set navigation state to MainMenu");
            }
        }

        void FindFirstSelectableIfNeeded()
        {
            if (firstSelectedObject == null)
            {
                if (mainMenuController != null)
                {
                    System.Reflection.FieldInfo buttonsField = typeof(MainMenuController).GetField("buttons", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    if (buttonsField != null)
                    {
                        Button[] buttons = buttonsField.GetValue(mainMenuController) as Button[];

                        if (buttons != null && buttons.Length > 0)
                        {
                            firstSelectedObject = buttons[0].gameObject;
                            LogDebug($"Found first selectable from MainMenuController: {firstSelectedObject.name}");
                            return;
                        }
                    }
                }

                Button[] childButtons = GetComponentsInChildren<Button>(true);
                if (childButtons.Length > 0)
                {
                    firstSelectedObject = childButtons[0].gameObject;
                    LogDebug($"Found first selectable in children: {firstSelectedObject.name}");
                }
            }
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00AAFF>[MainMenuNavigationExtension] {message}</color>");
            }
        }
    }
}