using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agua : MonoBehaviour
{
    // El punto donde se teletransportará al jugador
    [SerializeField] private Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // Comprobar si el objeto que entra es el jugador
        if (other.CompareTag("Player"))
        {
            // Teletransportar al jugador al SpawnPoint
            other.transform.position = spawnPoint.position;
        }
    }
}