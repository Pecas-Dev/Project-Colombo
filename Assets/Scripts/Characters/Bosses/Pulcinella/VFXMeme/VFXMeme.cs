using UnityEngine;

namespace ProjectColombo.Enemies
{
    public class VFXMeme : MonoBehaviour
    {
        public Vector3 offset;
        
        void Start()
        {
            GetComponent<RectTransform>().position = GetComponentInParent<Collider>().gameObject.transform.position + offset;
        }

        void Update()
        {
            GetComponent<RectTransform>().position = GetComponentInParent<Collider>().gameObject.transform.position + offset;
            transform.rotation = UnityEngine.Camera.main.transform.rotation;
        }
    }
}