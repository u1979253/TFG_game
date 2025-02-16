using UnityEngine;

namespace ggj25
{
    public class UIPauseView : MonoBehaviour
    {
        public void Open()
        {
            gameObject.SetActive(true);
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
        }
        
        public void ClickOnContinue()
        {
            SoundManager.Instance.PlaySFX(AudioType.SFX.Click);
            GameManager.Instance.ContinueGame();
        }
        
        public void ClickOnRetry()
        {
            SoundManager.Instance.PlaySFX(AudioType.SFX.Click);
            GameManager.Instance.ToGame();
        }
        
        public void ClickOnMainMenu()
        {
            SoundManager.Instance.PlaySFX(AudioType.SFX.Click);
            GameManager.Instance.ToMainMenu();
        }
    }
}
