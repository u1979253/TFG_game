using UnityEngine;

namespace ggj25
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "GGJ25/EnemyConfig", order = 1)]
    public class EnemyConfig : ScriptableObject
    {
        [field: Header("Movement")]
        [field: SerializeField]
        public float JumpDistance { get; private set; }
        
        [field: SerializeField]
        public float JumpInterval { get; private set; }
        [field: SerializeField]
        public float Friction { get; private set; }
        
        [field: SerializeField]
        public int AreaToPaint { get; private set; }
        
        [field: SerializeField]
        public Color ColorToPaint { get; private set; }
        [field: SerializeField]
        public float SpeedToConsiderStopped { get; private set; }
    }
    
}
