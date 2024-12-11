using UnityEngine;

public class GestorEnemigos : MonoBehaviour
{
    public static GestorEnemigos Instance { get; private set; }

    private int enemyCount = 0;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persists between scenes
    }

    /// <summary>
    /// Adds an enemy to the counter.
    /// </summary>
    public void AddEnemy()
    {
        enemyCount++;
        Debug.Log($"Enemy added. Current count: {enemyCount}");
    }

    /// <summary>
    /// Removes an enemy from the counter.
    /// </summary>
    public void RemoveEnemy()
    {
        enemyCount = Mathf.Max(0, enemyCount - 1); // Prevent negative counts
        Debug.Log($"Enemy removed. Current count: {enemyCount}");
    }

    /// <summary>
    /// Gets the current number of active enemies.
    /// </summary>
    public int GetEnemyCount()
    {
        return enemyCount;
    }
}
