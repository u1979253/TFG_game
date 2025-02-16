using UnityEngine;

namespace ggj25
{
    public class EnemyBase : SpikeSpawner
    {
        private RoomController _room;
        private GameObject Target;

        private void Start()
        {
            Target = GameObject.Find("Hero");
            _room = GetComponentInParent<RoomController>();
        }

        protected override void ShootSpike()
        {
            if (_room == null)
            {
                return;
            }
            if (!_room.IsActive)
            {
                return;
            }
            if (Target != null)
            {
                // Calcula la dirección hacia el héroe
                Vector3 directionToHero = (Target.transform.position - shootPoint.position).normalized;

                // Instancia el pincho en el punto de disparo
                GameObject spike = Instantiate(spikePrefab, shootPoint.position, Quaternion.identity);

                spike.gameObject.SetActive(true);

                // Ajusta la dirección y velocidad del pincho
                Rigidbody2D rb = spike.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = directionToHero * spikeSpeed;
                }
            }
            else
            {
                Debug.LogWarning("No se encontró el objeto 'Hero' en la escena.");
            }
        }
    }
}
