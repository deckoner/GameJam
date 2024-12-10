using UnityEngine;
using UnityEngine.AI;

public class BasicSlime : MonoBehaviour, IEnemy
{
    public int Health { get; set; } = 1;
    public int sightRange = 20; // Default sight range
    public int sightAngle = 45; // Default sight angle
    public float wanderSpeed = 1f; // Speed when wandering
    public float approachSpeed = 2f; // Speed when approaching player
    public float playerTrackingRange = 5f; // Range at which the slime will stop tracking the player

    private Transform player; // Reference to the player's Transform
    private NavMeshAgent agent; // NavMeshAgent for movement
    private Vector3 lastKnownPlayerPosition; // Store last known player position
    private bool isPlayerInSight = false; // Track if the player is in sight

    // Start is called before the first frame update
    void Start()
    {
        // Find the player object in the scene (assuming it has the "Player" tag)
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
        }

        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on BasicSlime.");
        }

        // Set default NavMeshAgent properties
        agent.speed = wanderSpeed;
        agent.angularSpeed = 300f; // Rotating speed towards the target
        agent.stoppingDistance = 0.1f; // To avoid stopping too early

        // Start wandering immediately
        SetNewRandomWanderDestination();
    }

    void Update()
    {
        ReactToPlayer();
    }

    public void ReactToPlayer()
    {
        // Ensure 'player' is properly assigned
        if (player != null)
        {
            if (IsPlayerInSight(player.position))
            {
                if (!isPlayerInSight)
                {
                    // Player just entered sight
                    isPlayerInSight = true;
                    lastKnownPlayerPosition = player.position; // Save the last known position
                    Debug.Log("Player detected, approaching!");
                    // Start approaching the player position (not the player itself)
                    ApproachPlayer(lastKnownPlayerPosition);
                }
                else
                {
                    // Continue updating the last known position while the player is in sight
                    lastKnownPlayerPosition = player.position;
                    ApproachPlayer(lastKnownPlayerPosition);
                }
            }
            else
            {
                if (isPlayerInSight)
                {
                    // Player just went out of sight
                    isPlayerInSight = false;
                    Debug.Log("Player out of sight, wandering!");
                    // Start wandering randomly
                    Wander();
                }
                else
                {
                    // If player is out of sight and no longer in range, keep the slime idle or wander
                    Wander();
                }
            }
        }
    }

    // Move the slime toward the last known player position
    private void ApproachPlayer(Vector3 targetPosition)
    {
        Debug.Log("Approaching!");

        // Set the destination of the NavMeshAgent to the last known player position
        agent.SetDestination(targetPosition);

        // Rotate the slime to face the player position (on the X and Z axis)
        Vector3 directionToTarget = new Vector3(targetPosition.x - transform.position.x, 0, targetPosition.z - transform.position.z);
        if (directionToTarget.magnitude > 0.1f) // Avoid unnecessary rotation
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, agent.angularSpeed * Time.deltaTime);
        }
    }

    // Wander randomly around the map
    public void Wander()
    {
        // If the agent is not already heading to a destination, set a new wander destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetNewRandomWanderDestination();
        }
    }

    // Set a new random wander destination within a specified range (increased range)
    private void SetNewRandomWanderDestination()
    {
        // Random position in a larger radius around the slime
        Vector3 randomDirection = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)); // Increased range
        Vector3 newWanderTarget = transform.position + randomDirection;

        // Ensure the new target is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newWanderTarget, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log("New wander target set: " + hit.position);
        }
        else
        {
            // If we can't find a valid point, just try again
            SetNewRandomWanderDestination();
        }
    }

    public bool IsPlayerInSight(Vector3 playerPosition)
    {
        // Calculate the direction from the enemy to the player
        Vector3 directionToPlayer = playerPosition - transform.position;

        // Check if the player is within the sight range
        if (directionToPlayer.magnitude < sightRange)
        {
            // Calculate the angle between the enemy's forward direction and the direction to the player
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            // If the angle is within the sight field (e.g., 45 degrees), the player is in sight
            if (angleToPlayer < sightAngle)
            {
                return true;
            }
        }

        return false;
    }

    public void PerformAction()
    {
        Debug.Log("Starting action!");
    }

    public void Attack()
    {
        Debug.Log("The slime attack!");
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("The slime has been damaged");
    }
}
