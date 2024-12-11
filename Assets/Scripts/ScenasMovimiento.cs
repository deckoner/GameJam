using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenasMovimiento : MonoBehaviour
{
    // Funci�n para salir del juego
    public void ExitGame()
    {
        // Si estamos en el editor de Unity, detener la ejecuci�n
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Funci�n para cargar la escena "Bonito que flipas"
    public void LoadBonitoQueFlipas()
    {
        SceneManager.LoadScene("Bonito que flipas");
    }

    // Funci�n para cargar la escena "Purificacion"
    public void LoadPurificacion()
    {
        SceneManager.LoadScene("Purificacion");
    }
}