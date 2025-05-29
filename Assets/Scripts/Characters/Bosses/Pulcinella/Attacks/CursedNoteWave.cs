using ProjectColombo;
using ProjectColombo.StateMachine.Player;
using UnityEngine;
using UnityEngine.VFX;

public class CursedNoteWave : MonoBehaviour
{
    public int damage;
    public float timeTillDelete;
    float timer;
    public Collider myCollider;
    bool hit = false;

    bool isMajor;
    public VisualEffect majorVFX;
    public VisualEffect minorVFX;

    float growthDuration = 6f;
    float minSize = 1f;
    float maxSize = 50f;

    private void Start()
    {
        myCollider.enabled = true;
    }

    void Update()
    {
        if (!hit) myCollider.enabled = true;

        timer += Time.deltaTime;
        if (timer >= timeTillDelete)
        {
            Destroy(this.gameObject);
        }

        // Calculate size growth using smoothstep for deceleration
        float t = Mathf.Clamp01(timer / growthDuration);
        float easedT = 1 - Mathf.Pow(1 - t, 2); // quadratic ease-out
        float currentSize = Mathf.Lerp(minSize, maxSize, easedT);

        // Apply size to VFX
        majorVFX.SetFloat("Size", currentSize);
        minorVFX.SetFloat("Size", currentSize);
        ((SphereCollider)myCollider).radius = currentSize * 2;
    }

    public void SetVFX(GameGlobals.MusicScale scale)
    {
        if (scale == GameGlobals.MusicScale.MAJOR)
        {
            isMajor = true;
            majorVFX.Play();
        }
        else if (scale == GameGlobals.MusicScale.MINOR)
        {
            isMajor = false;
            minorVFX.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStateMachine sm = other.GetComponent<PlayerStateMachine>();

            hit = true;
            myCollider.enabled = false;

            if (sm.currentStateEnum == PlayerStateMachine.PlayerState.Roll ||
                sm.isParrying || sm.isBlocking)
            {
                return;
            }

            sm.myHealthManager.TakeDamage(damage);
        }
    }
}
