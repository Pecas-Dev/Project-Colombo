using UnityEngine;
using UnityEngine.UI;


public class MeshControlUI : MonoBehaviour
{
    public ProceduralMeshGenerator meshGenerator;

    public Slider subdivisionSlider;
    public Text subdivisionText;

    void Start()
    {
        subdivisionSlider.onValueChanged.AddListener(UpdateSubdivisions);
        UpdateSubdivisions(subdivisionSlider.value);
    }

    void UpdateSubdivisions(float value)
    {
        int subdivisions = Mathf.RoundToInt(value);

        if (subdivisions != meshGenerator.subdivisions)
        {
            meshGenerator.UpdateMesh(subdivisions);
        }

        subdivisionText.text = "Subdivisions: " + subdivisions;
    }
}