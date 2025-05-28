using UnityEngine;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;


namespace ProjectColombo.InputSystem.Controllers
{
    public class LightbarTestScript : MonoBehaviour
    {
        [Header("Test Colors")]
        [SerializeField] Color testColor1 = Color.red;
        [SerializeField] Color testColor2 = Color.green;
        [SerializeField] Color testColor3 = Color.blue;

        PlayStationControllerLightbarManager lightbarManager;

        void Start()
        {
            lightbarManager = GameManager.Instance?.LightbarManager;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestMinorAttack();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestMajorAttack();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestCustomColor(testColor1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestCustomColor(testColor2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                TestCustomColor(testColor3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                TestDefaultColor();
            }
        }

        [ContextMenu("Test Minor Attack")]
        public void TestMinorAttack()
        {
            CustomEvents.MinorAttackPerformed(GameGlobals.MusicScale.MINOR);
            Debug.Log("Testing Minor Attack - Press 1 to test");
        }

        [ContextMenu("Test Major Attack")]
        public void TestMajorAttack()
        {
            CustomEvents.MajorAttackPerformed(GameGlobals.MusicScale.MAJOR);
            Debug.Log("Testing Major Attack - Press 2 to test");
        }

        [ContextMenu("Test Custom Color 1")]
        public void TestCustomColor1()
        {
            TestCustomColor(testColor1);
        }

        [ContextMenu("Test Custom Color 2")]
        public void TestCustomColor2()
        {
            TestCustomColor(testColor2);
        }

        [ContextMenu("Test Custom Color 3")]
        public void TestCustomColor3()
        {
            TestCustomColor(testColor3);
        }

        public void TestCustomColor(Color color)
        {
            if (lightbarManager != null)
            {
                lightbarManager.ForceColor(color);
                Debug.Log($"Testing custom color: {color}");
            }
        }

        [ContextMenu("Test Default Color")]
        public void TestDefaultColor()
        {
            CustomEvents.RequestLightbarColorChange(Color.blue);
            Debug.Log("Testing Default Color - Press 0 to test");
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 300, 20), "Lightbar Test Controls:");
            GUI.Label(new Rect(10, 30, 300, 20), "1 - Minor Attack (Purple)");
            GUI.Label(new Rect(10, 50, 300, 20), "2 - Major Attack (Yellow)");
            GUI.Label(new Rect(10, 70, 300, 20), "3 - Test Color 1");
            GUI.Label(new Rect(10, 90, 300, 20), "4 - Test Color 2");
            GUI.Label(new Rect(10, 110, 300, 20), "5 - Test Color 3");
            GUI.Label(new Rect(10, 130, 300, 20), "0 - Default Color");
        }
    }
}