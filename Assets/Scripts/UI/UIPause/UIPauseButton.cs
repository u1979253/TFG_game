using UnityEngine;

namespace ggj25
{
    public class UIPauseButton : MonoBehaviour
    {
        public void OnClickInPause()
        {
            GameManager.Instance.PauseGame();
        }
    }
}
