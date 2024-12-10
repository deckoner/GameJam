using UnityEngine;
using UnityEngine.AI;

public class BasicSlime : MonoBehaviour, IEnemy
{
    #region Fields
    // Public fields for health, sight, and movement speeds
    public int Health { get; set; } = 1; // The slime's health
    public int sightRange = 20; // Default sight range (in units)
    public int sightAngle = 45; // Default sight angle (in degrees)
    public float wanderSpeed = 1f; // Speed when wandering
    public float approachSpeed = 2f; // Speed when approaching player
    public float playerTrackingRange = 5f; // Range at which the slime will stop tracking the player

    // Private fields for references to other game objects and movement logic
    private Transform player; // Reference to the player's Transform
    private NavMeshAgent agent; // NavMeshAgent for movement
    private Vector3 lastKnownPlayerPosition; // Store last known player position
    private bool isPlayerInSight = false; // Track if the player is in sight
    #endregion

    #region Unity Methods
    /// <summary>
    /// Unity's Start method is called when the script is first initialized.
    /// Initializes references and sets up the NavMeshAgent for the slime's movement.
    /// </summary>
    void Start()
    {
        // Find the player object in the scene (assuming it has the "Player" tag)
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
        }

        // Get the NavMeshAgent component for movement and pathfinding
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on BasicSlime.");
        }

        // Set default NavMeshAgent properties for movement
        agent.speed = wanderSpeed;
        agent.angularSpeed = 300f; // Rotating speed towards the target
        agent.stoppingDistance = 0.1f; // To avoid stopping too early

        // Start wandering immediately
        SetNewRandomWanderDestination();
    }

    /// <summary>
    /// Unity's Update method is called once per frame.
    /// It checks if the player is in sight and handles movement accordingly.
    /// It also checks for key input (Q) to trigger the health reduction for all enemies.
    /// </summary>
    void Update()
    {
        // React to the player's position
        ReactToPlayer();

        // Check if the "Q" key is pressed to trigger health reduction for all enemies
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MakeAllEnemiesLoseHealth();
        }
    }

    /// <summary>
    /// Handles the behavior of the slime based on the player's position.
    /// If the player is in sight, the slime approaches the player. 
    /// If not, it will wander randomly.
    /// </summary>
    public void ReactToPlayer()
    {
        // Ensure 'player' is properly assigned
        if (player != null)
        {
            if (IsPlayerInSight(player.position))
            {
                // Player is in sight, start approaching the player
                if (!isPlayerInSight)
                {
                    isPlayerInSight = true;
                    lastKnownPlayerPosition = player.position; // Save the last known position
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
                // Player went out of sight, wander randomly
                if (isPlayerInSight)
                {
                    isPlayerInSight = false;
                    Wander();
                }
                else
                {
                    // If player is out of sight and no longer in range, continue wandering
                    Wander();
                }
            }
        }
    }

    /// <summary>
    /// Makes the slime approach the given target position (last known player position).
    /// </summary>
    /// <param name="targetPosition">The target position for the slime to move towards.</param>
    private void ApproachPlayer(Vector3 targetPosition)
    {
        // Set the destination of the NavMeshAgent to the target position
        agent.SetDestination(targetPosition);

        // Rotate the slime to face the target position (ignoring vertical axis)
        Vector3 directionToTarget = new Vector3(targetPosition.x - transform.position.x, 0, targetPosition.z - transform.position.z);
        if (directionToTarget.magnitude > 0.1f) // Avoid unnecessary rotation
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, agent.angularSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Makes the slime wander randomly around the map.
    /// If the slime is not moving towards a destination, it will find a new wander target.
    /// </summary>
    public void Wander()
    {
        // If the agent is not already heading to a destination, set a new wander destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetNewRandomWanderDestination();
        }
    }

    /// <summary>
    /// Sets a new random wander destination within a specified range.
    /// The target position is chosen randomly around the slime's current position.
    /// </summary>
    private void SetNewRandomWanderDestination()
    {
        // Generate a random position in a larger radius around the slime
        Vector3 randomDirection = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)); // Increased range
        Vector3 newWanderTarget = transform.position + randomDirection;

        // Ensure the new target is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newWanderTarget, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // If we can't find a valid point, just try again
            SetNewRandomWanderDestination();
        }
    }

    /// <summary>
    /// Checks if the player is within sight range and field of view of the slime.
    /// </summary>
    /// <param name="playerPosition">The position of the player.</param>
    /// <returns>True if the player is in sight, false otherwise.</returns>
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

    /// <summary>
    /// Reduces the slime's health by the specified damage amount.
    /// If health reaches 0, the slime is defeated and destroyed.
    /// </summary>
    /// <param name="damage">The amount of damage to reduce from health.</param>
    public void TakeDamage(int damage)
    {
        Health -= damage;
        Debug.Log("Slime took damage! Current health: " + Health);

        if (Health <= 0)
        {
            Debug.Log("DEFEATED");
            Destroy(gameObject); // Destroy the slime when health reaches 0
        }
    }

    /// <summary>
    /// Placeholder for performing some action (e.g., attack or idle animation).
    /// </summary>
    public void PerformAction()
    {
        Debug.Log("Starting action!");
        Attack();
    }

    /// <summary>
    /// Placeholder for the slime's attack action.
    /// </summary>
    public void Attack()
    {
        Debug.Log("The slime attack!");
    }
    #endregion

    #region Debugging Methods
    /// <summary>
    /// Reduces the health of all enemies tagged as "Enemy" by 1 when the "Q" key is pressed.
    /// This method is only available in the Unity Editor.
    /// </summary>
    private void MakeAllEnemiesLoseHealth()
    {
        // Find all game objects with the "Enemy" tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Loop through all enemies and reduce their health by 1
        foreach (GameObject enemy in enemies)
        {
            IEnemy enemyScript = enemy.GetComponent<IEnemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(1);
            }
        }
    }
    #endregion
}
