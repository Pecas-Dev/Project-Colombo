using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;


namespace ProjectColombo.UI
{
    public class EventSystemManager : MonoBehaviour
    {
        [Header("Event System Settings")]
        [SerializeField] GameObject eventSystemPrefab;
        [SerializeField] bool makeChildOfManager = true;
        [SerializeField] bool debugLogging = false;

        EventSystem currentEventSystem;

        void Awake()
        {
            currentEventSystem = GetComponentInChildren<EventSystem>();

            if (currentEventSystem == null)
            {
                currentEventSystem = FindFirstObjectByType<EventSystem>();

                if (currentEventSystem != null && makeChildOfManager)
                {
                    currentEventSystem.transform.SetParent(transform);
                }
            }

            if (currentEventSystem == null)
            {
                CreateEventSystem();
            }
        }

        void CreateEventSystem()
        {
            GameObject eventSystemObject;

            if (eventSystemPrefab != null)
            {
                eventSystemObject = Instantiate(eventSystemPrefab);
                eventSystemObject.name = eventSystemPrefab.name;

                if (debugLogging)
                {
                    Debug.Log("[EventSystemManager] Created EventSystem from prefab");
                }
            }
            else
            {
                eventSystemObject = new GameObject("EventSystem");
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<InputSystemUIInputModule>();

                if (debugLogging)
                {
                    Debug.Log("[EventSystemManager] Created basic EventSystem");
                }
            }

            if (makeChildOfManager)
            {
                eventSystemObject.transform.SetParent(transform);
            }

            currentEventSystem = eventSystemObject.GetComponent<EventSystem>();
        }

        void OnDestroy()
        {
            if (currentEventSystem != null && currentEventSystem.transform.parent == transform)
            {
                currentEventSystem.transform.SetParent(null);
                DontDestroyOnLoad(currentEventSystem.gameObject);

                if (debugLogging)
                {
                    Debug.Log("[EventSystemManager] Preserved EventSystem on destroy");
                }
            }
        }
    }
}
