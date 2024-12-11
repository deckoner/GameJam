using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class Escopeta : MonoBehaviour
{
    public int municion = 8; // Munición disponible
    public int municionMaxima = 8; // Munición máxima
    public float tiempoRecarga = 2f; // Tiempo de recarga
    public AudioClip sonidoDisparo; // Audio del disparo
    public AudioClip sonidoRecarga; // Sonido de recarga
    public GameObject efectosDisparo; // Efectos visuales de disparo (partículas)
    public GameObject efectosDisparoDos; // Efectos visuales de disparo (partículas)
    public float cadenciaDeDisparo = 1f; // Tiempo entre disparos
    private float proximoDisparo = 0f; // Tiempo para el próximo disparo

    public AudioSource audioSourceDisparo; // AudioSource exclusivo para efectos de disparo
    public AudioSource audioSourceRecarga; // AudioSource para el sonido de recarga

    private bool recargando = false; // Bandera de recarga

    // Referencia a TextMeshPro para mostrar la munición en el HUD
    public TextMeshProUGUI textoMunicion; 

    // Referencia al transform del arma para animaciones
    public Transform armaTransform; // Referencia al transform del arma (debe asignarse en el Inspector)

    private Quaternion rotacionOriginal;

    // Nuevo parámetro para el alcance máximo de la escopeta
    public float alcanceMaximo = 15f; // Alcance máximo de los disparos de la escopeta

    void Start()
    {
        // Verificar que el AudioSource para disparos y recarga estén configurados
        if (audioSourceDisparo == null || audioSourceRecarga == null)
        {
            Debug.LogError("AudioSource para disparos o recarga no asignado.");
            return;
        }

        // Asegurarse de que los efectos estén desactivados al inicio
        efectosDisparo.SetActive(false);
        efectosDisparoDos.SetActive(false);

        // Verificar que el TextMeshPro para la munición esté asignado
        if (textoMunicion == null)
        {
            Debug.LogError("Texto de munición no asignado.");
            return;
        }

        // Verificar que el arma esté asignada
        if (armaTransform == null)
        {
            Debug.LogError("Transform del arma no asignado.");
            return;
        }

        // Almacenar la rotación original del arma
        rotacionOriginal = armaTransform.rotation;

        // Actualizar el texto del HUD con la munición inicial
        ActualizarHUD();
    }

    void Update()
    {   
        if (Input.GetMouseButton(0)) // Botón de disparo
        {
            if (municion > 0 && !recargando && Time.time >= proximoDisparo)
            {
                Disparar();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !recargando && municion < municionMaxima)
        {
            StartCoroutine(Recargar());
        }
    }

    void Disparar()
    {
        if (municion > 0)
        {
            municion--;

            // Reproducir sonido de disparo
            if (sonidoDisparo != null && audioSourceDisparo != null)
            {
                audioSourceDisparo.PlayOneShot(sonidoDisparo);
            }

            // Activar efectos visuales
            efectosDisparo.SetActive(true);
            efectosDisparoDos.SetActive(true);
            Invoke("DesactivarEfectos", 0.1f);

            // Disparar rayos (dispersión de escopeta)
            for (int i = 0; i < 5; i++)
            {
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                ray.direction = Quaternion.Euler(Random.insideUnitSphere * 0.1f) * ray.direction;

                // Realizar el raycast con un alcance limitado
                if (Physics.Raycast(ray, out RaycastHit hit, alcanceMaximo))
                {
                    Debug.Log("Impacto en: " + hit.collider.name);
                }
            }

            // Actualizar el tiempo de disparo y el HUD
            proximoDisparo = Time.time + cadenciaDeDisparo;

            // Actualizar la interfaz de munición
            ActualizarHUD();
        }
    }

    void DesactivarEfectos()
    {
        efectosDisparo.SetActive(false);
        efectosDisparoDos.SetActive(false);
    }

    IEnumerator Recargar()
    {
        recargando = true;

        // Guardar la rotación local original del arma
        rotacionOriginal = armaTransform.localRotation;

        // Reproducir sonido de recarga
        if (sonidoRecarga != null && audioSourceRecarga != null)
        {
            audioSourceRecarga.PlayOneShot(sonidoRecarga);
        }

        // Animación de recarga (arma hacia abajo) con cuaternión
        Quaternion rotacionRecarga = Quaternion.Euler(new Vector3(-30f, 180f, 0f));
        armaTransform.DORotateQuaternion(rotacionRecarga, 0.5f);  // Aquí animamos la rotación
        yield return new WaitForSeconds(0.5f); // Esperar la mitad del tiempo de recarga antes de volver a poner el arma

        // Espera durante el tiempo de recarga
        yield return new WaitForSeconds(tiempoRecarga - 0.5f);

        // Recarga finalizada
        municion = municionMaxima;
        recargando = false;

        // Volver a la rotación original usando localRotation (para evitar que se vea afectado por el movimiento global)
        armaTransform.DOLocalRotateQuaternion(rotacionOriginal, 0.5f);  // Animamos la rotación local

        // Actualizar la interfaz de munición
        ActualizarHUD();
    }
    
    // Método para actualizar el texto del HUD
    void ActualizarHUD()
    {
        textoMunicion.text = municion + "/" + municionMaxima;
    }
}
