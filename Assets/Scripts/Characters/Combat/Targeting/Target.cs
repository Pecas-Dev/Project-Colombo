using UnityEngine;


namespace ProjectColombo.Combat
{
    public class Target : MonoBehaviour
    {
        void Start()
        {
            Renderer renderer = GetComponent<Renderer>();

            if (renderer == null)
            {
                return;
            }

            if (renderer != null)
            {
                renderer.material.SetColor("_BaseColor", Color.white);
            }
        }
    }
}
