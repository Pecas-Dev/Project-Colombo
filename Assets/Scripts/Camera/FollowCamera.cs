using UnityEngine;

namespace ProjectColombo.Core
{
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] Transform target;


        void LateUpdate()
        {
            transform.position = target.position;
        }
    }
}
