using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class RangedSlime : MonoBehaviour, IEnemy
{
    #region Fields
    public int Health { get; set; } = 1;
    public int sightRange = 20;
    public int sightAngle = 45;
    public float wanderSpeed = 1f;
    public float approachSpeed = 2f;
    public float playerTrackingRange = 5f;

    public float attackRange = 10f;
    public float shootCooldown = 2f;
    private float lastShootTime;

    private Transform player;
    private NavMeshAgent agent;
    private Vector3 lastKnownPlayerPosition;
    private bool isPlayerInSight = false;

    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    private bool isJumping = false; // To control jump animation
    private float groundY; // Store the ground level for resetting height

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float jumpDuration = 0.5f;
    #endregion

    #region Unity Methods
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent not found!");
            return;
        }

        groundY = transform.position.y; // Store the ground level
        SetNewRandomWanderDestination();
    }

    private void Update()
    {
        ReactToPlayer();

        if (isPlayerInSight && Time.time - lastShootTime > shootCooldown)
        {
            ShootAtPlayer();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            MakeAllEnemiesLoseHealth();
        }
    }
    #endregion

    #region Movement and Jumping
    private void ReactToPlayer()
    {
        if (player == null) return;

        if (IsPlayerInSight(player.position))
        {
            isPlayerInSight = true;
            lastKnownPlayerPosition = player.position;
            JumpTowardsTarget(lastKnownPlayerPosition, approachSpeed);
        }
        else
        {
            isPlayerInSight = false;
            Wander();
        }
    }

    public void Wander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetNewRandomWanderDestination();
        }
        JumpTowardsTarget(agent.destination, wanderSpeed);
    }

    private void SetNewRandomWanderDestination()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
        Vector3 newWanderTarget = transform.position + randomDirection;

        if (NavMesh.SamplePosition(newWanderTarget, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void JumpTowardsTarget(Vector3 targetPosition, float speed)
    {
        if (isJumping) return;

        isJumping = true;

        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 jumpTarget = new Vector3(
            transform.position.x + direction.x * speed * jumpDuration,
            groundY + jumpHeight,
            transform.position.z + direction.z * speed * jumpDuration
        );

        transform.DOMove(jumpTarget, jumpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                Vector3 landPosition = new Vector3(jumpTarget.x, groundY, jumpTarget.z);
                transform.DOMove(landPosition, jumpDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => isJumping = false);
            });
    }
    #endregion

    #region Combat
    private void ShootAtPlayer()
    {
        if (projectilePrefab == null) return;

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Physics.IgnoreCollision(projectile.GetComponent<Collider>(), GetComponent<Collider>());

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = directionToPlayer * projectileSpeed;
        }

        lastShootTime = Time.time;
    }

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
    #endregion

    #region Utilities
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void MakeAllEnemiesLoseHealth()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
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
