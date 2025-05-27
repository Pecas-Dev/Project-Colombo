using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ProjectColombo.UI
{
    public class DisplayDamageNumber : MonoBehaviour
    {
        public TMP_Text text;
        public float displayDuration = 1f;
        public Vector3 worldOffset;
        Vector3 currentOffset = new();
        public float speed;

        private Coroutine displayRoutine;
        private int accumulatedDamage = 0;
        HealthManager source;
        UnityEngine.Camera mainCamera;

        private void Start()
        {
            CustomEvents.OnDisplayDamageNumber += Display;
            text.text = "";
            currentOffset = worldOffset;
            source = GetComponentInParent<HealthManager>();
            mainCamera = UnityEngine.Camera.main;
        }

        private void OnDestroy()
        {
            CustomEvents.OnDisplayDamageNumber -= Display;
        }

        private void Update()
        {
            currentOffset.y += speed * Time.deltaTime;

            if (source != null)
            {
                transform.position = source.transform.position + currentOffset;
                transform.rotation = mainCamera.transform.rotation;
            }
        }

        private void Display(GameObject damagedObject, int amount)
        {
            if (source == null || damagedObject != source.gameObject) return;

            accumulatedDamage += amount;

            if (displayRoutine != null)
            {
                StopCoroutine(displayRoutine); // reset display timer
            }

            displayRoutine = StartCoroutine(ShowText());
        }

        private IEnumerator ShowText()
        {
            text.text = accumulatedDamage.ToString();
            currentOffset = worldOffset;
            yield return new WaitForSeconds(displayDuration);

            text.text = "";
            accumulatedDamage = 0;
            displayRoutine = null;
        }
    }
}
