using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace ProjectColombo.Camera
{
    public class CameraObstructionTransparency : MonoBehaviour
    {
        Transform playerTransform;
        Transform cameraTransform;
        List<GameObject> obstructingElements;
        List<GameObject> oldObstructingElements;

        float timer;
        public float checkInterval = 0.2f;
        public float targetOpacity = 0.0f;
        public float opacityChange = 0.1f;
        public LayerMask obstructingLayers;

        private void Start()
        {
            playerTransform = GameObject.Find("Player").transform;
            cameraTransform = GameObject.Find("Main Camera").transform;
            obstructingElements = new List<GameObject>();
            oldObstructingElements = new List<GameObject>();
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= checkInterval)
            {
                CheckForObstructingElements();
                timer = 0;
            }

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

        private void CheckForObstructingElements()
        {
            Vector3 origin = playerTransform.position;
            Vector3 direction = cameraTransform.position - origin;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction.normalized, direction.magnitude);

            List<GameObject> newHits = new();

            foreach (RaycastHit hit in hits)
            {
                if (((1 << hit.collider.gameObject.layer) & obstructingLayers) != 0)
                {
                    GameObject obj = hit.collider.gameObject;
                    newHits.Add(obj);
                }
            }

            foreach (GameObject obj in obstructingElements)
            {
                if (!newHits.Contains(obj))
                {
                    oldObstructingElements.Add(obj);
                }
            }

            obstructingElements = newHits;

            Debug.DrawRay(origin, direction, Color.red, 0.1f);
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
