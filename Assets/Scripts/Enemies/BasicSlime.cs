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
    [SerializeField] private float fleeSpeed = 3f; // Speed at which the slime flees

    [Header("Slime Effects")]
    [SerializeField] private ParticleSystem deathParticlesPrefab; // Reference to the particle system prefab

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] slimeAudioClips; // Array of possible audio clips
    [SerializeField] private AudioClip[] fleeAudioClips; // Array of possible audio clips when fleeing
    private AudioSource audioSource; // AudioSource component
    [SerializeField] private float audioPlayRange = 10f; // Range at which the slime will play sounds

    private static List<BasicSlime> allEnemies = new List<BasicSlime>(); // List to store all enemies

    private Transform player;
    private NavMeshAgent agent;
    private bool isPlayerInSight;
    private float nextAudioPlayTime; // Timer for when to play next audio clip
    private bool isDead = false; // To track if the slime is dead
    private bool isFleeing = false; // To track if the slime is currently fleeing
    private float fleeAudioDelay; // Delay for fleeing audio (random between 0.1 and 0.5 seconds)
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

        // Trigger fleeing behavior for nearby slimes when this one dies
        if (isDead)
        {
            MakeNearbySlimesFlee();
        }
    }

    private void Update()
    {
        if (!isDead)
        {
            ReactToPlayer();
            PlayRandomAudioClip();
        }
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
        if (isDead) return; // Don't process damage if already dead

        Health -= damage;
        Debug.Log($"Slime took damage! Current health: {Health}");

        if (Health <= 0)
        {
            isDead = true;
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

        // Play audio only if the player is close enough
        if (Vector3.Distance(transform.position, player.position) <= audioPlayRange)
        {
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

        // Play a fleeing sound when the slime is running away
        if (isFleeing && fleeAudioClips.Length > 0)
        {
            // Introduce a random delay between 0.1 and 0.5 seconds before playing fleeing sound
            if (fleeAudioDelay <= 0)
            {
                int randomIndex = Random.Range(0, fleeAudioClips.Length);
                audioSource.PlayOneShot(fleeAudioClips[randomIndex]);

                // Reset the flee audio delay timer
                fleeAudioDelay = Random.Range(0.1f, 0.5f);
            }
            else
            {
                fleeAudioDelay -= Time.deltaTime; // Countdown until we play the fleeing audio
            }

            isFleeing = false; // Reset fleeing flag after playing sound
        }
    }
    #endregion

    #region Fleeing Behavior
    private void MakeNearbySlimesFlee()
    {
        foreach (var slime in allEnemies)
        {
            if (slime == this) continue; // Skip self

            // Flee if within range (e.g., 15 units)
            if (Vector3.Distance(slime.transform.position, transform.position) <= 15f)
            {
                Vector3 fleeDirection = slime.transform.position - transform.position; // Direction away from the dead slime
                Vector3 fleeTarget = slime.transform.position + fleeDirection.normalized * 5f; // Flee by 5 units

                slime.agent.SetDestination(fleeTarget); // Set destination to flee position
                slime.agent.speed = fleeSpeed; // Increase speed while fleeing
                slime.isFleeing = true; // Set the fleeing flag to true
            }
        }
    }
    #endregion
}
