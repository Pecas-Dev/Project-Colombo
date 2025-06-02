using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class EmitterVelocitySender : MonoBehaviour
{
    private Vector3 lastPosition;
    private VisualEffect vfx;

    void Start()
    {
        lastPosition = transform.position;
        vfx = GetComponent<VisualEffect>();
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 velocity = (currentPosition - lastPosition) / Time.deltaTime;
        lastPosition = currentPosition;

        
        vfx.SetVector3("EmitterVelocity", velocity);
    }
}