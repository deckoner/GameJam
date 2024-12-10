using UnityEngine;
using System.Collections; // Necesario para usar IEnumerator

public class Escopeta : MonoBehaviour
{
    // Configuración del arma
    public int municion = 8; // Munición disponible
    public int municionMaxima = 8; // Munición máxima
    public float tiempoRecarga = 2f; // Tiempo de recarga
    public AudioClip sonidoDisparo; // Audio del disparo
    public GameObject efectosDisparo; // Efectos visuales de disparo (partículas)
    public GameObject efectosDisparoDos; // Efectos visuales de disparo (partículas)
    public float cadenciaDeDisparo = 1f; // Tiempo entre disparos
    private float proximoDisparo = 0f; // Tiempo para el próximo disparo

    // Configuración de disparo
    public int cantidadRaycasts = 5; // Cantidad de rayos que la escopeta disparará (simulando esparcimiento)
    public float dispersion = 0.1f; // Cuánto se dispersan los rayos en grados
    private AudioSource audioSource; // Fuente de audio para reproducir el sonido

    private bool recargando = false; // Bandera de recarga

    void Start()
    {
        // Obtener el componente AudioSource
        audioSource = GetComponent<AudioSource>();

        // Asegurarse de que los efectos estén desactivados al inicio
        efectosDisparo.SetActive(false);
        efectosDisparoDos.SetActive(false);
    }

    void Update()
    {   
        // si se ha presionado el boton de disprar
        if (Input.GetMouseButton(0)) {
            // si hay munición
            if (municion > 0 && !recargando && Time.time >= proximoDisparo)
            {
                Disparar();
            }
        }

        // Recarga manual al presionar la tecla "R"
        if (Input.GetKeyDown(KeyCode.R) && !recargando && municion < municionMaxima)
        {
            StartCoroutine(Recargar());
        }
    }

    void Disparar()
    {
        if (municion > 0)
        {
            // Descontar munición
            municion--;

            // Reproducir sonido de disparo
            audioSource.PlayOneShot(sonidoDisparo);

            // Activar efectos visuales
            efectosDisparo.SetActive(true);
            efectosDisparoDos.SetActive(true);
            Invoke("DesactivarEfectos", 0.1f);

            // Disparar múltiples rayos para simular el disparo de la escopeta
            for (int i = 0; i < cantidadRaycasts; i++)
            {
                // Dispersar los rayos en un ángulo aleatorio
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                ray.direction = Quaternion.Euler(Random.insideUnitSphere * dispersion) * ray.direction;

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    // Aquí puedes agregar lógica para lo que sucede cuando el raycast impacta un objeto
                    // Como aplicar daño a un enemigo, destruir objetos, etc.
                    Debug.Log("Impacto en: " + hit.collider.name);
                }
            }

            // Calcular el próximo disparo
            proximoDisparo = Time.time + cadenciaDeDisparo;
        }
    }

    void DesactivarEfectos()
    {
        // Desactivar efectos visuales después de cierto tiempo
        efectosDisparo.SetActive(false);
        efectosDisparoDos.SetActive(false);
    }

    IEnumerator Recargar()
    {
        recargando = true;
        // Aquí puedes agregar animaciones o efectos de recarga si lo deseas
        Debug.Log("Recargando...");
        yield return new WaitForSeconds(tiempoRecarga); // Esperar el tiempo de recarga
        municion = municionMaxima; // Recargar la munición
        recargando = false; // Terminar recarga
    }
}
