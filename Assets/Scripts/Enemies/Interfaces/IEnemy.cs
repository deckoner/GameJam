using UnityEngine;

/// <summary>
/// Inteface to define enemy behaviour
/// </summary>
public interface IEnemy
{
    /// <summary>
    /// Attack the target.
    /// </summary>
    void Attack();

    /// <summary>
    /// Take damage with a specified amount.
    /// </summary>
    void TakeDamage(int damage);

    /// <summary>
    /// Health of the enemy.
    /// </summary>
    int Health { get; set; }

    /// <summary>
    /// Enemy's wandering behavior.
    /// </summary>
    void Wander();

    /// <summary>
    /// Behavior when the player is in sight.
    /// </summary>
    void ReactToPlayer();

    /// <summary>
    /// Checks if the player is within sight range.
    /// </summary>
    /// <param name="playerPosition">The player's position.</param>
    /// <returns>True if the player is in sight, otherwise false.</returns>
    bool IsPlayerInSight(Vector3 playerPosition);

    /// <summary>
    /// Enemy's type-specific action when reacting.
    /// </summary>
    void PerformAction();

    /// <summary>
    /// Current fear level of the enemy.
    /// </summary>
    //float FearMeter { get; }

    /// <summary>
    /// Increase the fear level of the enemy.
    /// </summary>
    //void IncreaseFear(float amount);

    /// <summary>
    /// Check if the enemy is afraid.
    /// </summary>
    //bool IsAfraid();

}
