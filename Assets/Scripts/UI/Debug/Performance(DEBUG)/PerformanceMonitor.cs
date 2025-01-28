using UnityEngine;
using UnityEngine.UI;


public class PerformanceMonitor : MonoBehaviour
{
    public Text statsText;

    float deltaTime;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        long memory = System.GC.GetTotalMemory(false) / 1024 / 1024;

        int totalTris, totalPolys;
        GetTotalMeshInfo(out totalTris, out totalPolys);

        statsText.text = $"FPS: {fps:0.0}\nMemory: {memory} MB\nTris: {totalTris}\nPolys: {totalPolys}";
    }

    void GetTotalMeshInfo(out int totalTris, out int totalPolys)
    {
        MeshFilter[] meshes = FindObjectsByType<MeshFilter>(FindObjectsSortMode.InstanceID);

        totalTris = 0;
        totalPolys = 0;

        foreach (MeshFilter mf in meshes)
        {
            if (mf.mesh != null)
            {
                totalTris += mf.mesh.triangles.Length / 3;
                totalPolys += mf.mesh.vertexCount / 4;
            }
        }
    }
}