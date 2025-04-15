using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace ProjectColombo.LevelManagement
{
    public class BossRoomCulling : MonoBehaviour
    {
        public float targetOpacity = 0.0f;
        public List<GameObject> obstructingElements;

        private void Start()
        {
            GetAllChildren(transform);

            foreach (GameObject g in obstructingElements)
            {
                MakeTransparent(g);
            }
        }



        void GetAllChildren(Transform parent)
        {
            foreach (Transform child in parent)
            {
                obstructingElements.Add(child.gameObject);
                GetAllChildren(child); // Recursively add this child's children
            }
        }


        private void MakeTransparent(GameObject g)
        {
            Renderer renderer = g.GetComponent<Renderer>();
            if (renderer == null) return;

            SetMaterialTransparent(renderer);

            Color color = renderer.material.color;

            color.a = targetOpacity;
            renderer.material.color = color;
        }


        private void SetMaterialTransparent(Renderer renderer)
        {
            Material material = renderer.material;
            if (material == null) return;

            // Mark the surface as transparent (URP)
            material.SetFloat("_Surface", 1); // 1 = Transparent
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;

            // Make sure blending is enabled properly
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);

            // URP transparency keywords
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            material.DisableKeyword("_ALPHATEST_ON"); // optional unless using cutout
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON"); // optional, depending on how you want the blend

            // This is key: force the GPU to recompile material with updated properties
            material.EnableKeyword("_ALPHABLEND_ON");
        }

    }
}