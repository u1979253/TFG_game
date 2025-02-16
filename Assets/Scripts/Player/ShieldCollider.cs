using UnityEngine;

namespace ggj25
{
    public class ShieldCollider : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _shieldHitEffect;

        // Números de las layers
        private const int LAYER_SPIKE = 8;

        void OnCollisionEnter2D(Collision2D collision)
        {
            // Detectar colisión con layers específicas
            int collisionLayer = collision.gameObject.layer;

            HitInShield();
            
            if (collisionLayer == LAYER_SPIKE)
            { 
                HitWithProjectile(collision.gameObject);
            }
        }

        private void HitInShield()
        {
            SoundManager.Instance.PlaySFX(AudioType.SFX.HitInShield);
            if (!_shieldHitEffect.gameObject.activeSelf)
            {
                _shieldHitEffect.gameObject.SetActive(true);
            }

            _shieldHitEffect.Play();
        }

        private static void HitWithProjectile(GameObject otherGameObject)
        {
            /*
            Debug.Log("Escudo bloqueó un proyectil (Trigger): " + otherGameObject.name);
            Destroy(otherGameObject.gameObject); // Destruir proyectil al impactar
            
            //De momento dejo esta cancion para empezar
            SoundManager.Instance.PlaySFX(AudioType.SFX.ProjectileDestroyed);
            */
        }
    }
}
