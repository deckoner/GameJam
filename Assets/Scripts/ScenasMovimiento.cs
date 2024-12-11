using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenasMovimiento : MonoBehaviour
{
    // Función para salir del juego
    public void ExitGame()
    {
        // Si estamos en el editor de Unity, detener la ejecución
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();  // Cerrar la aplicación en una compilación
        #endif
    }

    // Función para cargar la escena "Bonito que flipas"
    public void LoadBonitoQueFlipas()
    {
        SceneManager.LoadScene("Bonito que flipas");
    }

    // Función para cargar la escena "Purificacion"
    public void LoadPurificacion()
    {
        SceneManager.LoadScene("Purificacion");
    }
}