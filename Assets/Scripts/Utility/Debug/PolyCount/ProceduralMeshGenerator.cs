using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMeshGenerator : MonoBehaviour
{ 
    [SerializeField, ReadOnlyInspector][Range(1, 1000)] public int subdivisions = 10;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    void Start()
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        GenerateMesh();
    }

    public void UpdateMesh(int newSubdivisions)
    {
        if (subdivisions == newSubdivisions)
        {
            return;
        }

        subdivisions = newSubdivisions;
        GenerateMesh();
    }

    void GenerateMesh()
    {
        int gridSize = subdivisions + 1;

        vertices = new Vector3[gridSize * gridSize];
        triangles = new int[subdivisions * subdivisions * 6];

        for (int z = 0, i = 0; z < gridSize; z++)
        {
            for (int x = 0; x < gridSize; x++, i++)
            {
                vertices[i] = new Vector3(x, 0, z);
            }
        }

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < subdivisions; z++)
        {
            for (int x = 0; x < subdivisions; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + gridSize;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + gridSize;
                triangles[tris + 5] = vert + gridSize + 1;

                vert++;
                tris += 6;
            }

            vert++;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}