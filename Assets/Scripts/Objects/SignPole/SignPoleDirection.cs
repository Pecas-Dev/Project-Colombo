using ProjectColombo.LevelManagement;
using UnityEngine;

namespace ProjectColombo.Objects
{
    public class SignPoleDirection : MonoBehaviour
    {
        public float rotation = 15f;

        private void Start()
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            transform.rotation = Quaternion.FromToRotation(transform.right, Vector3.right) * transform.rotation;
            transform.rotation *= Quaternion.Euler(0, rotation, 0);
        }
    }
}