using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicSlime : MonoBehaviour, IEnemy
{
    #region Fields
    public int Health { get; set; } = 1; // Slime's health

    [Header("Sight Settings")]
    [SerializeField] private int sightRange = 20; // Range in which the slime can see the player
    [SerializeField] private int sightAngle = 45; // Field of view angle

    [Header("Movement Settings")]
    [SerializeField] private float wanderSpeed = 1f;
    [SerializeField] private float approachSpeed = 2f;

    [Header("Slime Effects")]
    [SerializeField] private ParticleSystem deathParticlesPrefab; // Reference to the particle system prefab

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] slimeAudioClips; // Array of possible audio clips
    private AudioSource audioSource; // AudioSource component

    private static List<BasicSlime> allEnemies = new List<BasicSlime>(); // List to store all enemies

    private Transform player;
    private NavMeshAgent agent;
    private bool isPlayerInSight;
    private float nextAudioPlayTime; // Timer for when to play next audio clip
    #endregion

    #region Unity Methods
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Ensure the player has the 'Player' tag.");
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing!");
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource if it's missing
        }

        // Add this enemy to the list of all enemies
        allEnemies.Add(this);

        // Notify GestorEnemigos about this slime's spawn
        if (GestorEnemigos.Instance != null)
        {
            GestorEnemigos.Instance.AddEnemy();
        }

        SetNewRandomWanderDestination();

        // Randomize the interval between 5 and 10 seconds
        float randomInterval = Random.Range(5f, 10f);
        nextAudioPlayTime = Time.time + randomInterval; // Initialize the next audio play time
    }

    private void OnDestroy()
    {
        // Remove this enemy from the list when destroyed
        allEnemies.Remove(this);

        // Notify GestorEnemigos about this slime's death
        if (GestorEnemigos.Instance != null)
        {
            GestorEnemigos.Instance.RemoveEnemy();
        }
    }

    private void Update()
    {
        ReactToPlayer();
        PlayRandomAudioClip();
    }
    #endregion

    #region Movement
    private void ReactToPlayer()
    {
        if (player == null) return;

        if (IsPlayerInSight(player.position))
        {
            isPlayerInSight = true;
            ApproachPlayer(player.position);
        }
        else
        {
            isPlayerInSight = false;
            Wander();
        }
    }

    private void ApproachPlayer(Vector3 targetPosition)
    {
        if (agent.isStopped) agent.isStopped = false;

        agent.speed = approachSpeed;
        agent.SetDestination(targetPosition);
    }

    public void Wander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetNewRandomWanderDestination();
        }

        agent.speed = wanderSpeed;
    }

    private void SetNewRandomWanderDestination()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
        Vector3 target = transform.position + randomDirection;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    #endregion

    #region Utilities
    private bool IsPlayerInSight(Vector3 playerPosition)
    {
        Vector3 directionToPlayer = playerPosition - transform.position;

        if (directionToPlayer.magnitude < sightRange)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            return angleToPlayer < sightAngle;
        }

        return false;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        Debug.Log($"Slime took damage! Current health: {Health}");

        if (Health <= 0)
        {
            Debug.Log("Slime defeated!");

            // Instantiate the particle system prefab and play it at the slime's position
            if (deathParticlesPrefab != null)
            {
                ParticleSystem particles = Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);
                particles.Play(); // Play the particle effect
            }

            Destroy(gameObject); // Enemy dies
        }
    }
    #endregion

    #region Audio
    private void PlayRandomAudioClip()
    {
        if (slimeAudioClips.Length == 0 || audioSource.isPlaying) return; // No clips or already playing

        // Play a random audio clip at the defined interval
        if (Time.time >= nextAudioPlayTime)
        {
            int randomIndex = Random.Range(0, slimeAudioClips.Length);
            audioSource.PlayOneShot(slimeAudioClips[randomIndex]);

            // Randomize the next audio play time between 5 and 10 seconds
            float randomInterval = Random.Range(5f, 10f);
            nextAudioPlayTime = Time.time + randomInterval;
        }
    }
    #endregion
}
