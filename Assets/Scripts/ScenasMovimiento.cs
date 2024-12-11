using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenasMovimiento : MonoBehaviour
{
    // Check for no more slimes and load "Purificacion" scene
    private void Update()
    {
        if (GestorEnemigos.Instance != null && GestorEnemigos.Instance.EnemyCount == 0)
        {
            LoadPurificacion();
        }
    }

    // Function to load the "Purificacion" scene
    private void LoadPurificacion()
    {
        SceneManager.LoadScene("Purificacion");
    }
}
