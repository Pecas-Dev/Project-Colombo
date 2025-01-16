using UnityEngine;

public class TestHealth : MonoBehaviour
{
    [SerializeField] int health = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! Current health: {health}");

        if (health <= 0)
        {
            Debug.Log($"{gameObject.name} has been defeated!");
            Destroy(gameObject);
        }
    }
}
