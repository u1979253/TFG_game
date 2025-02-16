using UnityEngine;

namespace ggj25
{
    [CreateAssetMenu(fileName = "HeroConfig", menuName = "GGJ25/HeroConfig", order = 1)]
    public class HeroConfig : ScriptableObject
    {
        [field: Header("Movement")]
        [field: SerializeField]
        public float Speed { get; private set; }
        
        [field: SerializeField]
        public float FrictionRate { get; private set; }
        
        [field: SerializeField]
        public float ShieldDistance { get; private set; }
        
        [field: Header("Cleaning")]
        [field: SerializeField]
        public float CleanSize { get; private set; }
        
        [field: Header("Live")]
        [field: SerializeField]
        public int Lives { get; private set; }
        
        [field: Header("DEBUG")]
        [field: SerializeField]
        public bool Inmortal { get; private set; }
    }
}