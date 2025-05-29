using UnityEngine;
using ProjectColombo.Combat.ComboMeter;


namespace ProjectColombo
{
    public class ComboMeterDebug : MonoBehaviour
    {
        [Header("Debug Settings")]
        public int pointsToAdd = 50;
        public int pointsToSubtract = 50;

        [Header("Key Bindings")]
        public KeyCode addPointsKey = KeyCode.Alpha1;
        public KeyCode subtractPointsKey = KeyCode.Alpha2;
        public KeyCode resetMeterKey = KeyCode.Alpha3;

        [Header("References")]
        public ComboMeter comboMeter;


        void Start()
        {
            if (comboMeter == null)
            {
                comboMeter = FindFirstObjectByType<ComboMeter>();

                if (comboMeter == null)
                {
                    Debug.LogWarning("ComboMeterDebug: Could not find ComboMeter component!");
                }
            }
        }

        void Update()
        {
            if (comboMeter == null)
            {
                return;
            }

            if (Input.GetKeyDown(addPointsKey))
            {
                AddDebugPoints(pointsToAdd);
                Debug.Log($"[ComboMeter Debug] Added {pointsToAdd} points. Current: {comboMeter.currentPoints} points, Level: {comboMeter.currentLevel}");
            }

            if (Input.GetKeyDown(subtractPointsKey))
            {
                AddDebugPoints(-pointsToSubtract);
                Debug.Log($"[ComboMeter Debug] Subtracted {pointsToSubtract} points. Current: {comboMeter.currentPoints} points, Level: {comboMeter.currentLevel}");
            }

            if (Input.GetKeyDown(resetMeterKey))
            {
                ResetComboMeter();
                Debug.Log("[ComboMeter Debug] Reset combo meter to 0 points, Level 0");
            }
        }

        void AddDebugPoints(int amount)
        {
            var addPointsMethod = typeof(ComboMeter).GetMethod("AddPoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (addPointsMethod != null)
            {
                addPointsMethod.Invoke(comboMeter, new object[] { amount });
            }
            else
            {
                Debug.LogError("Could not find AddPoints method in ComboMeter!");
            }
        }

        void ResetComboMeter()
        {
            var pointsField = typeof(ComboMeter).GetField("currentPoints");
            var levelField = typeof(ComboMeter).GetField("currentLevel");

            if (pointsField != null && levelField != null)
            {
                pointsField.SetValue(comboMeter, 0);
                levelField.SetValue(comboMeter, 0);

                var updateUIMethod = typeof(ComboMeter).GetMethod("UpdateUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                updateUIMethod?.Invoke(comboMeter, null);

                var deactivateMethod = typeof(ComboMeter).GetMethod("DeactivateAllAttribStorages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                deactivateMethod?.Invoke(comboMeter, null);
            }
        }

        void OnGUI()
        {
            if (comboMeter == null)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== COMBO METER DEBUG ===", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label($"Current Points: {comboMeter.currentPoints}");
            GUILayout.Label($"Current Level: {comboMeter.currentLevel}");
            GUILayout.Label($"Max Level: {comboMeter.maxLevel}");

            GUILayout.Space(10);

            GUILayout.Label("Controls:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label($"[{addPointsKey}] Add {pointsToAdd} points");
            GUILayout.Label($"[{subtractPointsKey}] Subtract {pointsToSubtract} points");
            GUILayout.Label($"[{resetMeterKey}] Reset meter");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}