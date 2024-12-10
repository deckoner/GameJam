using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f; // Bullet speed
    public int damage = 1; // Bullet damage
    public float lifeTime = 5f; // Time before the bullet disappears

    private Vector3 direction;

    void Start()
    {
        // Destroy the bullet after its lifetime
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector3 direction)
    {
        this.direction = direction;
    }

    void Update()
    {
        // Move the bullet in the set direction
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Assuming the player has a method to take damage
            // other.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(this); // Destroy the bullet on impact with the player
        }
        else
        {
            Destroy(gameObject); // Destroy the bullet if it hits something else
        }
    }
}
