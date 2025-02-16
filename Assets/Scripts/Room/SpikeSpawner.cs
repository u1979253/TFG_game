using System;
using System.Collections;
using System.Collections.Generic;
using ggj25;
using UnityEngine;

public class SpikeSpawner : MonoBehaviour
{
    [Header("Spawner configurator")]
    [SerializeField] protected GameObject spikePrefab; // Prefab del pincho
    [SerializeField] protected Transform shootPoint;   // Punto de disparo
    [SerializeField] protected float shootFrequency = 2f; // Frecuencia de disparo (en segundos)
    [SerializeField] protected Vector2 shootDirection = Vector2.right; // Direcci�n de disparo
    [SerializeField] protected float spikeSpeed = 5f; // Velocidad del pincho

    private float timer;
    private RoomController _roomController;

    private void Awake()
    {
        _roomController = GetComponentInParent<RoomController>();
    }

    private void Update()
    {
        if (_roomController?.IsActive == false)
        {
            return;
        }
        
        // Actualiza el temporizador
        timer += Time.deltaTime;

        // Dispara un pincho si se cumple el tiempo de frecuencia
        if (timer >= shootFrequency)
        {
            ShootSpike();
            timer = 0f; // Reinicia el temporizador
        }
    }

    protected virtual void ShootSpike()
    {
        SoundManager.Instance.PlaySFX(AudioType.SFX.ShootSpike);
        // Instancia el pincho en el punto de disparo
        GameObject spike = Instantiate(spikePrefab, shootPoint.position, Quaternion.identity);

        spike.gameObject.SetActive(true);
        // Ajusta la direcci�n y velocidad del pincho
        Rigidbody2D rb = spike.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = shootDirection.normalized * spikeSpeed;
        }

        // Opcional: destruye el pincho despu�s de un tiempo para optimizar
        Destroy(spike, 5f); // Elimina el pincho despu�s de 5 segundos
    }

    // Funci�n para cambiar la direcci�n del disparo desde otros scripts
    public void SetShootDirection(Vector2 newDirection)
    {
        shootDirection = newDirection;
    }
}
