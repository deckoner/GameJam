using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    [Header("Projectile Lifetime")]
    [SerializeField] private float lifetime = 5f; // Time in seconds before self-destruction

    private void Start()
    {
        Destroy(gameObject, lifetime); // Self-destruct after a set time
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Projectile collided with: {collision.gameObject.name}");

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Projectile hit the Player!");
            Destroy(gameObject); // Destroy projectile
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Projectile triggered by: {other.gameObject.name}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("Projectile hit the Player!");
            Destroy(gameObject); // Destroy projectile
        }
    }
}
