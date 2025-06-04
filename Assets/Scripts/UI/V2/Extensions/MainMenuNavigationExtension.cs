using UnityEngine;
using UnityEngine.UI;
using System.Collections;


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

        }

        void OnEnable()
        {
            StartCoroutine(DelayedNavigationSetup());
        }

        IEnumerator DelayedNavigationSetup()
        {
            yield return new WaitForSecondsRealtime(0.3f);

            if (navigationManager != null)
            {
                if (navigationManager.GetCurrentState() != UINavigationState.MainMenu)
                {
                    navigationManager.SetNavigationState(UINavigationState.MainMenu);
                    LogDebug("Set navigation state to MainMenu after delay (without changing selection)");
                }
                else
                {
                    LogDebug("MainMenu state already set - no action needed");
                }
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