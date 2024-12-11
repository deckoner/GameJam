using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class ArmaV2 : MonoBehaviour
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
    public float alcanceMaximo = 15f;
    public int dano;
    [SerializeField] private Transform canon;
    [SerializeField] private Color colorRaycast = Color.red; // Color de la línea

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

            // Realizar el raycast desde el cañón hacia el centro de la pantalla
            Ray ray = new Ray(canon.position, DireccionHaciaCentroPantalla());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, alcanceMaximo))
            {
                // Mostrar información del impacto en el log
                Debug.Log($"Impacto en: {hit.collider.name} a una distancia de: {hit.distance}");

                // Comprobar si el objeto impactado tiene la interfaz IEnemy
                IEnemy enemy = hit.collider.GetComponent<IEnemy>();
                if (enemy != null)
                {
                    Debug.Log($"Impacto a un enemigo: {hit.collider.name}. Aplicando {dano} de daño.");
                    enemy.TakeDamage(dano);
                }
                
                // Mostrar el punto exacto del impacto
                Debug.DrawLine(ray.origin, hit.point, Color.green, 1.0f);
            }

            // Activar efectos visuales
            efectosDisparo.SetActive(true);
            efectosDisparoDos.SetActive(true);
            Invoke("DesactivarEfectos", 0.1f);

            // Visualizar el raycast completo
            Debug.DrawRay(ray.origin, ray.direction * alcanceMaximo, colorRaycast, 1.0f);

            // Actualizar el tiempo de disparo y el HUD
            proximoDisparo = Time.time + cadenciaDeDisparo;

            // Actualizar la interfaz de munición
            ActualizarHUD();
        }
    }

    // Método para calcular la dirección desde el cañón hacia el centro de la pantalla
    private Vector3 DireccionHaciaCentroPantalla()
    {
        // Obtener un rayo desde la cámara al centro de la pantalla
        Ray camRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Vector3 puntoDestino;

        // Usar un raycast para encontrar el punto en la dirección de la cámara
        if (Physics.Raycast(camRay, out RaycastHit hit, alcanceMaximo))
        {
            puntoDestino = hit.point; // Si impacta, el punto destino es el impacto
        }
        else
        {
            puntoDestino = camRay.origin + camRay.direction * alcanceMaximo; // Si no impacta, usar el máximo alcance
        }

        // Devolver la dirección normalizada desde el cañón hacia el punto destino
        return (puntoDestino - canon.position).normalized;
    }

    // Dibujar el raycast en el editor incluso cuando no se esté ejecutando
    private void OnDrawGizmos()
    {
        if (canon == null || Camera.main == null) return;

        Gizmos.color = colorRaycast;

        // Obtener la dirección hacia el centro de la pantalla
        Vector3 direccion = DireccionHaciaCentroPantalla();
        Vector3 puntoFinal = canon.position + direccion * alcanceMaximo;

        Gizmos.DrawLine(canon.position, puntoFinal);
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