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
            foreach (GameObject g in obstructingElements)
            {
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
                SetMaterialOpaque(renderer);

                oldObstructingElements.Remove(g);
                return true;
            }
        }

        private void SetMaterialTransparent(Renderer renderer)
        {
            Material material = renderer.material;
            if (material.renderQueue == 3000) return;

            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        private void SetMaterialOpaque(Renderer renderer)
        {
            Material material = renderer.material;
            if (material.renderQueue == 2000) return;

            material.SetInt("_SrcBlend", (int)BlendMode.One);
            material.SetInt("_DstBlend", (int)BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 2000;
        }
    }
}
