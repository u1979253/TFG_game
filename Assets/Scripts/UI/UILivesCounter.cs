using System.Collections.Generic;
using System.Linq;
using deVoid.Utils;
using UnityEngine;

namespace ggj25
{
    public class UILivesCounter : MonoBehaviour
    {
        [SerializeField] private List<Transform> _lives;
        
        void Start()
        {
            Signals.Get<OnCurrentLivesChangedEvent>().AddListener(OnCurrentLivesChanged);

            var maxLives = GameObject.FindObjectOfType<HeroController>().Config.Lives;
            OnCurrentLivesChanged(maxLives);

        }

        private void OnCurrentLivesChanged(int currentLives)
        {
            for (int i = 0; i < _lives.Count; i++)
            {
                _lives[i].gameObject.SetActive((i+1) <= currentLives);
            }
        }
        private void OnDestroy()
        {
            Signals.Get<OnCurrentLivesChangedEvent>().RemoveListener(OnCurrentLivesChanged);
        }
    }
}
