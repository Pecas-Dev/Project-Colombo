using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


namespace ProjectColombo.UI
{
    public class PickUpScreenNavigationExtension : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        UINavigationManager navigationManager;
        PickUpScreenController pickUpController;

        void Awake()
        {
            pickUpController = GetComponent<PickUpScreenController>();

            if (pickUpController == null)
            {
                LogError("PickUpScreenController not found on same GameObject!");
            }
        }

        void Start()
        {
            navigationManager = FindFirstObjectByType<UINavigationManager>();

            if (navigationManager == null)
            {
                LogError("UINavigationManager not found!");
            }
        }

        void OnEnable()
        {
            StartCoroutine(DelayedRegisterState());
        }

        IEnumerator DelayedRegisterState()
        {
            yield return new WaitForEndOfFrame();
            yield return null;

            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.PickUpScreen);
                LogDebug("Navigation state set to PickUpScreen");
            }
        }

        public void OnPickUpScreenActivated()
        {
            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.PickUpScreen);
                LogDebug("Pick up screen navigation activated");
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OnPickUpScreenDeactivated()
        {
            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.None);
                LogDebug("Pick up screen navigation deactivated");
            }
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00FF88>[PickUpScreenNavigationExtension] {message}</color>");
            }
        }

        void LogError(string message)
        {
            Debug.LogError($"[PickUpScreenNavigationExtension] {message}");
        }
    }
}