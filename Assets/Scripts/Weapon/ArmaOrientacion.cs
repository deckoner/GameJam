using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmaOrientacion : MonoBehaviour
{
    [SerializeField] private Transform weapon;  // Referencia al modelo del arma
    [SerializeField] private Camera playerCamera;  // Referencia a la cámara del jugador

    void Start()
    {
        // Llamamos a la función para alinear el arma al centro de la pantalla una vez al inicio
        AlignWeaponToCenter();
    }

    public void AlignWeaponToCenter()
    {
        if (weapon != null && playerCamera != null)
        {
            // Obtener la dirección hacia donde apunta la cámara (sin modificar el eje Y)
            Vector3 targetDirection = playerCamera.transform.forward;
            targetDirection.y = 0; // Ignorar la inclinación hacia arriba o abajo de la cámara

            // Calcular la rotación necesaria para alinear el arma
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Rotar el arma 180 grados sobre el eje Y para que el cañón apunte hacia el centro de la pantalla
            targetRotation *= Quaternion.Euler(0, 180, 0);

            // Aplicar la rotación calculada al arma
            weapon.rotation = targetRotation;
        }
    }
}
