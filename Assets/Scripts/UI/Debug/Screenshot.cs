using UnityEngine;

public class Screenshot : MonoBehaviour
{
    [SerializeField] string path;

    [SerializeField][Range(1, 5)] int size = 1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            path += "screenshot ";
            path += System.Guid.NewGuid().ToString() + ".png";

            ScreenCapture.CaptureScreenshot(path, size);
        }
    }
}
