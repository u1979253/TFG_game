using deVoid.Utils;
using TMPro;
using UnityEngine;

namespace ggj25
{
    public class CleanCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _counter;
        
        void Start()
        {
            Signals.Get<OnRoomCleaningRateChanged>().AddListener(OnRoomCleaningRate);
            OnRoomCleaningRate(0);
        }

        private void OnRoomCleaningRate(float rate)
        {
            _counter.text = $"Cleaned: {rate * 100:#00}%";
        }
        private void OnDestroy()
        {
            Signals.Get<OnRoomCleaningRateChanged>().RemoveListener(OnRoomCleaningRate);
        }
    }
}
