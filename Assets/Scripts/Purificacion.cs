using System.Collections;
using UnityEngine;

public class Purificacion : MonoBehaviour
{
    public GameObject explosion; // Prefab de la explosión
    public GameObject planeta;   // Referencia al objeto planeta
    public Vector3 localizacion; // Posición de la explosión
    public AudioClip sonidoExplosion; // Clip de audio para la explosión
    public AudioClip sonidoPanfarria; // Clip de audio para la panfarria
    public GameObject canvasPanfarria; // Canvas que aparecerá con la transición

    private AudioSource audioSource; // Fuente de audio para reproducir los sonidos
    private RectTransform canvasTransform; // Transform del canvas para la animación

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>(); // Agrega un AudioSource al objeto actual
        if (canvasPanfarria != null)
        {
            canvasTransform = canvasPanfarria.GetComponent<RectTransform>();
            canvasPanfarria.SetActive(false); // Asegura que el canvas esté inicialmente invisible
        }
        Invoke("EjecutarExplosion", 1.5f); // Llama a la función EjecutarExplosion después de 1.5 segundos
    }

    private void EjecutarExplosion()
    {
        // Instancia la explosión
        Instantiate(explosion, localizacion, Quaternion.identity);

        // Reproduce el sonido de la explosión
        if (sonidoExplosion != null)
        {
            audioSource.PlayOneShot(sonidoExplosion);
        }

        // Destruye el planeta
        Destroy(planeta);

        // Llama a la función Panfarria después de 0.8 segundos
        Invoke("Panfarria", 7.7f);
    }

    private void Panfarria()
    {
        // Reproduce el sonido de la panfarria
        if (sonidoPanfarria != null)
        {
            audioSource.PlayOneShot(sonidoPanfarria);
        }

        // Activa el canvas y comienza la transición
        if (canvasPanfarria != null)
        {
            canvasPanfarria.SetActive(true);
        }
    }
}