using TMPro;
using UnityEngine;

public class GestorEnemigos : MonoBehaviour
{
    public static GestorEnemigos Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI enemyCountText; // Reference to the TextMeshPro UI element
    private int enemyCount = 0;

    public int EnemyCount // Public property to access enemy count
    {
        get { return enemyCount; }
    }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persists between scenes
    }

    private void Start()
    {
        UpdateEnemyCountText();
    }

    /// <summary>
    /// Adds an enemy to the counter and updates the UI.
    /// </summary>
    public void AddEnemy()
    {
        enemyCount++;
        UpdateEnemyCountText();
    }

    /// <summary>
    /// Removes an enemy from the counter and updates the UI.
    /// </summary>
    public void RemoveEnemy()
    {
        enemyCount = Mathf.Max(0, enemyCount - 1); // Prevent negative counts
        UpdateEnemyCountText();
    }

    /// <summary>
    /// Updates the TextMeshPro UI element with the current enemy count.
    /// </summary>
    private void UpdateEnemyCountText()
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = $"{enemyCount}";
        }
        else
        {
            Debug.LogWarning("Enemy count TextMeshPro reference is missing!");
        }
    }
}
