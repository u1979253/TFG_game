using MobileConsole;
using MyBox;
using UnityEngine;

namespace ggj25
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "GGJ25/GameConfig", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [field: SerializeField]
        public float CleanSuccessRate { get; private set; }
        
        [field: Header("Game Over Delay")]
        [field: SerializeField]
        public float GameOverDelay { get; private set; }

        [ButtonMethod]
        public void CleanCurrent()
        {
            GameManager.Instance.LevelManager.CleanCurrentRoom();
        }
    }
}
