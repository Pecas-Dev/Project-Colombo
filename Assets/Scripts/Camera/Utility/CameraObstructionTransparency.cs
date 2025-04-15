using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace ProjectColombo.Camera
{
    public class CameraObstructionTransparency : MonoBehaviour
    {
        public List<GameObject> obstructingElements;
        public List<GameObject> oldObstructingElements;

        public float targetOpacity = 0.0f;
        public float opacityChange = 0.1f;
        public LayerMask obstructingLayers;

        private void Start()
        {
            obstructingElements = new List<GameObject>();
            oldObstructingElements = new List<GameObject>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Start();
            }

            for (int i = obstructingElements.Count - 1; i >= 0; i--)
            {
                GameObject g = obstructingElements[i];
                if (g == null)
                {
                    obstructingElements.RemoveAt(i);
                    continue;
                }

                MakeTransparent(g);
            }

            for (int i = 0; i < oldObstructingElements.Count;)
            {
                if (!MakeVisible(oldObstructingElements[i]))
                {
                    i++;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & obstructingLayers) != 0)
            {
                if (!obstructingElements.Contains(other.gameObject))
                {
                    obstructingElements.Add(other.gameObject);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (((1 << other.gameObject.layer) & obstructingLayers) != 0)
            {
                if (obstructingElements.Contains(other.gameObject))
                {
                    obstructingElements.Remove(other.gameObject);
                    oldObstructingElements.Add(other.gameObject);
                }
            }
        }

        private void MakeTransparent(GameObject g)
        {
            Renderer renderer = g.GetComponent<Renderer>();
            if (renderer == null) return;

            SetMaterialTransparent(renderer);

            Color color = renderer.material.color;
            if (color.a > targetOpacity)
            {
                color.a = Mathf.Max(targetOpacity, color.a - opacityChange * Time.deltaTime);
                renderer.material.color = color;
            }
        }

        private bool MakeVisible(GameObject g)
        {
            Renderer renderer = g.GetComponent<Renderer>();
            if (renderer == null) return false;

            Color color = renderer.material.color;
            if (color.a < 1)
            {
                color.a = Mathf.Min(1, color.a + opacityChange * Time.deltaTime);
                renderer.material.color = color;
                return false;
            }
            else
            {
                oldObstructingElements.Remove(g);
                return true;
            }
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
