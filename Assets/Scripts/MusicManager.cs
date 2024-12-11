using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource; // El componente AudioSource para reproducir los audios
    public AudioClip initialAudio; // El audio inicial
    public List<AudioClip> musicPlaylist; // Lista de canciones
    private int currentTrackIndex = 0; // Índice de la canción actual

    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource no asignado.");
            return;
        }

        if (initialAudio != null)
        {
            PlayInitialAudio();
        }
        else
        {
            Debug.LogWarning("No hay audio inicial asignado. Comenzando directamente con la lista de reproducción.");
            PlayMusicPlaylist();
        }
    }

    // Método para reproducir el audio inicial
    private void PlayInitialAudio()
    {
        audioSource.clip = initialAudio;
        audioSource.Play();

        // Usamos Invoke para esperar la duración del audio inicial antes de empezar la siguiente reproducción
        Invoke("PlayMusicPlaylist", initialAudio.length);
    }

    // Método para reproducir la lista de música
    private void PlayMusicPlaylist()
    {
        if (musicPlaylist.Count == 0)
        {
            Debug.LogWarning("La lista de canciones está vacía.");
            return;
        }

        PlayNextTrack();
    }

    // Método que maneja la reproducción de la siguiente canción en la lista
    private void PlayNextTrack()
    {
        if (currentTrackIndex >= musicPlaylist.Count)
        {
            Debug.LogWarning("Se ha llegado al final de la lista de reproducción.");
            return;
        }

        // Cargar el siguiente clip de música
        audioSource.clip = musicPlaylist[currentTrackIndex];
        audioSource.Play();

        // Calcular el tiempo de la siguiente canción, iniciando justo cuando la actual termina
        float nextTrackTime = audioSource.clip.length;

        // Avanzamos al siguiente track
        currentTrackIndex = (currentTrackIndex + 1) % musicPlaylist.Count;

        // Programar la siguiente canción con un retraso igual al tiempo de la duración de la canción actual
        Invoke("PlayNextTrackDelayed", nextTrackTime);
    }

    // Este método se llama después de que termine la canción actual, para reproducir la siguiente
    private void PlayNextTrackDelayed()
    {
        // Llamamos a PlayNextTrack para reproducir la siguiente canción
        PlayNextTrack();
    }
}
