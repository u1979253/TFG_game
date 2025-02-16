using UnityEngine;

namespace ggj25
{
    public class UIIMainMenuCanvas : MonoBehaviour
    {
        [SerializeField]
        private Transform _credits;

        private void Awake()
        {
            _credits.gameObject.SetActive(false);
        }

        public void OnClickInPlay()
        {
            SoundManager.Instance.PlaySFX(AudioType.SFX.Click);
            GameManager.Instance.ToGame();
        }

        public void OnClickInCredits()
        {
            SoundManager.Instance.PlaySFX(AudioType.SFX.Click);
            _credits.gameObject.SetActive(true);
        }
        
        public void OnClickInCloseCredits()
        {
            SoundManager.Instance.PlaySFX(AudioType.SFX.Click);
            _credits.gameObject.SetActive(false);
        }

        public void OnClickInExit()
        {
            SoundManager.Instance.PlaySFX(AudioType.SFX.Click);
            GameManager.Instance.ExitGame();
        }
    }
}
