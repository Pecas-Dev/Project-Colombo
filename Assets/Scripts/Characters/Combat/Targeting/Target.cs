using UnityEngine;


namespace ProjectColombo.Combat
{
    [RequireComponent(typeof(Renderer))]
    public class Target : MonoBehaviour
    {
        void Start()
        {
            Renderer renderer = GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material.SetColor("_BaseColor", Color.white);
            }
        }
    }
}
