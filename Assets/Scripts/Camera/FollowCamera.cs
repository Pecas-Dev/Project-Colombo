using UnityEngine;


public class FollowCamera : MonoBehaviour
{
    [SerializeField] Transform target;


    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position;
        }
    }
}

