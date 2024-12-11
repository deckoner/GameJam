using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RangedSlime : MonoBehaviour
{
    #region Fields
    public int Health { get; set; } = 1;

    [Header("Sight Settings")]
    [SerializeField] private int sightRange = 20;
    [SerializeField] private int sightAngle = 45;

    [Header("Movement Settings")]
    [SerializeField] private float wanderSpeed = 1f;
    [SerializeField] private float approachSpeed = 2f;
    [SerializeField] private float playerTrackingRange = 5f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float shootCooldown = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float jumpDuration = 0.5f;

    private Transform player;
    private NavMeshAgent agent;
    private float lastShootTime;
    private Vector3 lastKnownPlayerPosition;
    private bool isPlayerInSight;
    private bool isJumping;
    private float groundY;
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

        groundY = transform.position.y;
        SetNewRandomWanderDestination();
    }

    private void Update()
    {
        if (player == null) return;

        ReactToPlayer();

        if (isPlayerInSight && Time.time - lastShootTime > shootCooldown)
        {
            ShootAtPlayer();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Sight range and angle visualization
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Vector3 forward = transform.forward * sightRange;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, sightAngle / 2, 0) * forward);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, -sightAngle / 2, 0) * forward);
    }
    #endregion

    #region Movement and Jumping
    private void ReactToPlayer()
    {
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
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        randomDirection.y = groundY;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void JumpTowardsTarget(Vector3 targetPosition, float speed)
    {
        if (isJumping) return;

        isJumping = true;

        // Calculate direction and horizontal distance
        Vector3 direction = (targetPosition - transform.position).normalized;
        float horizontalDistance = Vector3.Distance(
            new Vector3(targetPosition.x, 0, targetPosition.z),
            new Vector3(transform.position.x, 0, transform.position.z)
        );

        // Calculate mid-point for the arc
        Vector3 midPoint = (transform.position + targetPosition) / 2;
        midPoint.y += jumpHeight;

        // Use DOTween to create a smooth parabolic path
        Vector3[] path = new Vector3[]
        {
        transform.position,
        midPoint,
        new Vector3(targetPosition.x, groundY, targetPosition.z)
        };

        transform.DOPath(path, jumpDuration, PathType.CatmullRom, PathMode.Full3D)
            .SetEase(Ease.Linear)
            .OnComplete(() => isJumping = false);
    }

    #endregion

    #region Combat
    private void ShootAtPlayer()
    {
        if (projectilePrefab == null) return;

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Physics.IgnoreCollision(projectile.GetComponent<Collider>(), GetComponent<Collider>());

        if (projectile.TryGetComponent(out Rigidbody rb))
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            rb.velocity = directionToPlayer * projectileSpeed;
        }

        lastShootTime = Time.time;
    }

    private bool IsPlayerInSight(Vector3 playerPosition)
    {
        Vector3 directionToPlayer = playerPosition - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance < sightRange)
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
    #endregion
}
