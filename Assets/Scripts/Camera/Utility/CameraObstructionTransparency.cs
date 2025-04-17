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

            foreach (Material m in renderer.materials)
            {
                float currentTransparency = m.GetFloat("_Transparency");

                if (currentTransparency > targetOpacity)
                {
                    currentTransparency = Mathf.Max(targetOpacity, currentTransparency - opacityChange * Time.deltaTime);
                    m.SetFloat("_Transparency", currentTransparency);
                }
            }
        }

        private bool MakeVisible(GameObject g)
        {
            Renderer renderer = g.GetComponent<Renderer>();
            if (renderer == null) return false;

            foreach (Material m in renderer.materials)
            {
                float currentTransparency = m.GetFloat("_Transparency");

                if (currentTransparency < 1)
                {
                    currentTransparency = Mathf.Min(1, currentTransparency + opacityChange * Time.deltaTime);
                    m.SetFloat("_Transparency", currentTransparency);
                    return false;
                }
                else
                {
                    oldObstructingElements.Remove(g);
                    return true;
                }
            }

            return false;
        }

        private void SetMaterialTransparent(Renderer renderer)
        {
            Material material = renderer.material;

            if (material == null) return;
        }
    }
}
