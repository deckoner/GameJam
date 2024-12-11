using UnityEngine;
using UnityEngine.AI;

public class BasicSlime : MonoBehaviour, IEnemy
{
    #region Fields
    // Campos públicos para salud, rango de visión y velocidades de movimiento
    public int Health { get; set; } = 1; // La salud del slime
    public int sightRange = 20; // Rango de visión predeterminado (en unidades)
    public int sightAngle = 45; // Ángulo de visión predeterminado (en grados)
    public float wanderSpeed = 1f; // Velocidad al deambular
    public float approachSpeed = 2f; // Velocidad al acercarse al jugador
    public float playerTrackingRange = 5f; // Rango en el que el slime dejará de seguir al jugador

    // Campos privados para referencias a otros objetos y lógica de movimiento
    private Transform player; // Referencia al Transform del jugador
    private NavMeshAgent agent; // NavMeshAgent para el movimiento
    private Vector3 lastKnownPlayerPosition; // Guardar la última posición conocida del jugador
    private bool isPlayerInSight = false; // Controla si el jugador está a la vista
    #endregion

    #region Unity Methods
    /// <summary>
    /// Inicializa las referencias y configura el NavMeshAgent para el movimiento del slime.
    /// </summary>
    void Start()
    {
        // Encuentra el objeto jugador en la escena (suponiendo que tiene la etiqueta "Player")
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("¡Jugador no encontrado! Asegúrate de que el jugador tenga la etiqueta 'Player'.");
        }

        // Obtiene el componente NavMeshAgent para el movimiento y la búsqueda de caminos
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("Componente NavMeshAgent no encontrado en BasicSlime.");
        }

        // Configura las propiedades predeterminadas del NavMeshAgent para el movimiento
        agent.speed = wanderSpeed;
        agent.angularSpeed = 300f; // Velocidad de rotación hacia el objetivo
        agent.stoppingDistance = 0.1f; // Para evitar detenerse demasiado pronto

        // Comienza a deambular inmediatamente
        SetNewRandomWanderDestination();
    }

    /// <summary>
    /// Verifica si el jugador está a la vista y maneja el movimiento en consecuencia.
    /// También verifica la entrada de teclas (Q) para activar la reducción de salud para todos los enemigos.
    /// </summary>
    void Update()
    {
        // Reacciona a la posición del jugador
        ReactToPlayer();

        // Verifica si la tecla "Q" es presionada para activar la reducción de salud de todos los enemigos
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MakeAllEnemiesLoseHealth();
        }
    }

    /// <summary>
    /// Maneja el comportamiento del slime en función de la posición del jugador.
    /// Si el jugador está a la vista, el slime se acerca al jugador. 
    /// Si no, deambulará aleatoriamente.
    /// </summary>
    public void ReactToPlayer()
    {
        // Asegura que 'player' esté correctamente asignado
        if (player != null)
        {
            if (IsPlayerInSight(player.position))
            {
                // El jugador está a la vista, el slime comienza a acercarse
                if (!isPlayerInSight)
                {
                    isPlayerInSight = true;
                    lastKnownPlayerPosition = player.position; // Guardar la última posición conocida
                    ApproachPlayer(lastKnownPlayerPosition);
                }
                else
                {
                    // Continúa actualizando la última posición conocida mientras el jugador esté a la vista
                    lastKnownPlayerPosition = player.position;
                    ApproachPlayer(lastKnownPlayerPosition);
                }
            }
            else
            {
                // El jugador salió de la vista, deambulará aleatoriamente
                if (isPlayerInSight)
                {
                    isPlayerInSight = false;
                    Wander();
                }
                else
                {
                    // Si el jugador ya no está a la vista, sigue deambulando
                    Wander();
                }
            }
        }
    }

    /// <summary>
    /// Hace que el slime se acerque a la posición del objetivo (última posición conocida del jugador).
    /// </summary>
    /// <param name="targetPosition">La posición del objetivo hacia donde se moverá el slime.</param>
    private void ApproachPlayer(Vector3 targetPosition)
    {
        // Establece la destinación del NavMeshAgent hacia la posición del objetivo
        agent.SetDestination(targetPosition);

        // Rota el slime para mirar hacia la posición objetivo (ignorando el eje vertical)
        Vector3 directionToTarget = new Vector3(targetPosition.x - transform.position.x, 0, targetPosition.z - transform.position.z);
        if (directionToTarget.magnitude > 0.1f) // Evita rotaciones innecesarias
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, agent.angularSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Hace que el slime deambule aleatoriamente por el mapa.
    /// Si el slime no se está moviendo hacia un destino, encontrará un nuevo destino aleatorio.
    /// </summary>
    public void Wander()
    {
        // Si el agente no está ya yendo hacia un destino, establece un nuevo destino de deambulación
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetNewRandomWanderDestination();
        }
    }

    /// <summary>
    /// Establece un nuevo destino aleatorio dentro de un rango especificado.
    /// La posición del objetivo se elige aleatoriamente alrededor de la posición actual del slime.
    /// </summary>
    private void SetNewRandomWanderDestination()
    {
        // Genera una posición aleatoria en un radio más grande alrededor del slime
        Vector3 randomDirection = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)); // Rango aumentado
        Vector3 newWanderTarget = transform.position + randomDirection;

        // Asegura que el nuevo objetivo esté sobre el NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newWanderTarget, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Si no se puede encontrar un punto válido, intenta de nuevo
            SetNewRandomWanderDestination();
        }
    }

    /// <summary>
    /// Verifica si el jugador está dentro del rango de visión y el campo de visión del slime.
    /// </summary>
    /// <param name="playerPosition">La posición del jugador.</param>
    /// <returns>True si el jugador está a la vista, false si no.</returns>
    public bool IsPlayerInSight(Vector3 playerPosition)
    {
        // Calcula la dirección desde el enemigo hacia el jugador
        Vector3 directionToPlayer = playerPosition - transform.position;

        // Verifica si el jugador está dentro del rango de visión
        if (directionToPlayer.magnitude < sightRange)
        {
            // Calcula el ángulo entre la dirección hacia el jugador y la dirección hacia el frente del slime
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            // Si el ángulo está dentro del campo de visión (por ejemplo, 45 grados), el jugador está a la vista
            if (angleToPlayer < sightAngle)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Reduce la salud del slime por la cantidad de daño especificada.
    /// Si la salud llega a 0, el slime es derrotado y destruido.
    /// </summary>
    /// <param name="damage">La cantidad de daño a reducir de la salud.</param>
    public void TakeDamage(int damage)
    {
        Health -= damage;
        Debug.Log("¡El slime ha recibido daño! Salud actual: " + Health);

        if (Health <= 0)
        {
            Debug.Log("DEFEATED");
            Destroy(gameObject); // Destruye el slime cuando su salud llega a 0
        }
    }

    /// <summary>
    /// Función de acción para realizar alguna acción (por ejemplo, atacar o animación de reposo).
    /// </summary>
    public void PerformAction()
    {
        Debug.Log("¡Iniciando acción!");
        Attack();
    }

    /// <summary>
    /// Función de ataque del slime.
    /// </summary>
    public void Attack()
    {
        Debug.Log("¡El slime ataca!");
    }
    #endregion

    #region Debugging Methods
    /// <summary>
    /// Reduce la salud de todos los enemigos con la etiqueta "Enemy" en 1 cuando se presiona la tecla "Q".
    /// Este método solo está disponible en el Editor de Unity.
    /// </summary>
    private void MakeAllEnemiesLoseHealth()
    {
        // Encuentra todos los objetos con la etiqueta "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Itera a través de todos los enemigos y reduce su salud en 1
        foreach (GameObject enemy in enemies)
        {
            IEnemy enemyScript = enemy.GetComponent<IEnemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(1);
            }
        }
    }
    #endregion
}
