using UnityEngine;
using DG.Tweening;

public class Jumping : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float jumpHeight = 1f; // Height of each jump
    [SerializeField] private float jumpDuration = 0.5f; // Time for each jump
    [SerializeField] private float moveSpeed = 2f; // Speed of horizontal movement

    private Vector3 targetPosition; // Position the slime is moving towards
    private bool isJumping = false; // Prevent overlapping jumps

    private void Start()
    {
        // Set an initial random target position for demonstration
        SetRandomTargetPosition();
        StartJumpingMovement();
    }

    private void SetRandomTargetPosition()
    {
        // Set a random target position for wandering
        targetPosition = new Vector3(
            transform.position.x + Random.Range(-5f, 5f),
            transform.position.y,
            transform.position.z + Random.Range(-5f, 5f)
        );
    }

    private void StartJumpingMovement()
    {
        // Start continuous jumping
        JumpTowardsTarget();
    }

    private void JumpTowardsTarget()
    {
        if (isJumping) return;

        isJumping = true;

        // Calculate the horizontal movement direction
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Calculate the new jump target position
        Vector3 jumpTarget = new Vector3(
            transform.position.x + direction.x * moveSpeed * jumpDuration,
            transform.position.y + jumpHeight,
            transform.position.z + direction.z * moveSpeed * jumpDuration
        );
        transform.DOScaleY(0.5f, jumpDuration / 2).OnComplete(() =>
        {
            transform.DOScaleY(1f, jumpDuration / 2);
        });

        // Perform the jump using DoTween
        transform.DOMove(jumpTarget, jumpDuration)
            .SetEase(Ease.OutQuad) // Smooth upward arc
            .OnComplete(() =>
            {
                // Return to ground level
                Vector3 landPosition = new Vector3(jumpTarget.x, transform.position.y, jumpTarget.z);
                transform.DOMove(landPosition, jumpDuration)
                    .SetEase(Ease.InQuad) // Smooth downward arc
                    .OnComplete(() =>
                    {
                        // Check if target is reached or continue jumping
                        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                        {
                            isJumping = false;
                            JumpTowardsTarget(); // Continue jumping
                        }
                        else
                        {
                            SetRandomTargetPosition(); // Change target if reached
                            isJumping = false;
                            JumpTowardsTarget(); // Restart jumping
                        }
                    });
            });
    }
}
