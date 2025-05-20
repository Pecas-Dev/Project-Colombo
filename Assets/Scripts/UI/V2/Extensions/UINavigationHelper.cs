using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


namespace ProjectColombo.UI
{
    public class UINavigationHelper : MonoBehaviour
    {
        [Header("Navigation Configuration")]
        [SerializeField] UINavigationState menuState = UINavigationState.None;
        [SerializeField] GameObject firstSelectableObject;
        [SerializeField] bool autoRegisterOnEnable = true;
        [SerializeField] float registrationDelay = 0.1f;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        void Awake()
        {
            FindFirstSelectableIfNeeded();
        }

        void OnEnable()
        {
            if (autoRegisterOnEnable)
            {
                StartCoroutine(DelayedRegistration());
            }
        }
        IEnumerator DelayedRegistration()
        {
            yield return new WaitForSecondsRealtime(registrationDelay);

            if (gameObject.activeInHierarchy)
            {
                RegisterWithNavigationManager();
            }
        }
        public void RegisterWithNavigationManager()
        {
            if (menuState == UINavigationState.None)
            {
                LogWarning("MenuState is not set!");
                return;
            }

            FindFirstSelectableIfNeeded();

            if (firstSelectableObject == null)
            {
                LogWarning("No selectable object found to register!");
                return;
            }

            UINavigationManager navigationManager = UINavigationManager.Instance;

            if (navigationManager != null)
            {
                navigationManager.RegisterFirstSelectable(menuState, firstSelectableObject);
                navigationManager.SetNavigationState(menuState);
                LogDebug($"Registered with UINavigationManager: State={menuState}, Selectable={firstSelectableObject.name}");
            }
            else
            {
                LogWarning("UINavigationManager instance not found!");

                EventSystem eventSystem = EventSystem.current;

                if (eventSystem != null)
                {
                    eventSystem.SetSelectedGameObject(firstSelectableObject);
                }
            }
        }

        public void SetMenuState(UINavigationState state)
        {
            menuState = state;
        }

        public void SetFirstSelectable(GameObject selectable)
        {
            firstSelectableObject = selectable;

            UINavigationManager navigationManager = UINavigationManager.Instance;
            if (navigationManager != null && menuState != UINavigationState.None)
            {
                navigationManager.RegisterFirstSelectable(menuState, firstSelectableObject);
            }
        }

        void FindFirstSelectableIfNeeded()
        {
            if (firstSelectableObject == null)
            {
                Button[] buttons = GetComponentsInChildren<Button>(true);

                if (buttons.Length > 0)
                {
                    foreach (Button button in buttons)
                    {
                        if (button.gameObject.name.Contains("First"))
                        {
                            firstSelectableObject = button.gameObject;
                            LogDebug($"Found first selectable by name: {firstSelectableObject.name}");
                            return;
                        }
                    }

                    foreach (Button button in buttons)
                    {
                        if (button.gameObject.activeInHierarchy && button.interactable)
                        {
                            firstSelectableObject = button.gameObject;
                            LogDebug($"Found first active selectable: {firstSelectableObject.name}");
                            return;
                        }
                    }

                    firstSelectableObject = buttons[0].gameObject;
                    LogDebug($"Using first button as selectable: {firstSelectableObject.name}");
                }
                else
                {
                    Selectable selectable = GetComponentInChildren<Selectable>(true);

                    if (selectable != null)
                    {
                        firstSelectableObject = selectable.gameObject;
                        LogDebug($"Using fallback selectable: {firstSelectableObject.name}");
                    }
                }

                if (firstSelectableObject == null)
                {
                    LogWarning("No selectable object found in hierarchy!");
                }
            }
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00CCFF>[UINavigationHelper] {message}</color>");
            }
        }

        void LogWarning(string message)
        {
            Debug.LogWarning($"[UINavigationHelper] {message}");
        }
    }
}