using System.Collections;
using System.Collections.Generic;
using System.Linq;
using deVoid.Utils;
using Unity.Collections;
using UnityEngine;

namespace ggj25
{
    public class RoomController : MonoBehaviour
    {
        private const float UPDATE_RATE = 0.5f;
        
        [field: SerializeField] public SpriteRenderer MainArt { get; private set; }
        [SerializeField] private SpriteRenderer _dust;
        [SerializeField] private SpriteRenderer _walldust;
        [SerializeField] private SpriteRenderer _roomLock;
        [SerializeField] private ParticleSystem _cleaningEffect;
        [field: SerializeField] public Transform CheckPoint { get; private set; }
        
        [SerializeField, ReadOnly]
        private List<DoorController> _doors;
        
        public float CleanFactor { get; private set; }
        
        private HeroController _hero;

        private Vector3 _previousPosition;
        private Color32[] _currentColor;
        public DustCleaner DustCleaner { get; private set; }
        private int _pixelSize;

        private float _timeStamp;


        public bool IsActive { get; private set; }
        public bool IsCompleted { get; private set; }
        
        public void Init()
        {
            _hero = GameObject.FindObjectOfType<HeroController>();
            _previousPosition = _hero.transform.position;

            DustCleaner = new DustCleaner(_dust);

            _pixelSize = _dust.sprite.texture.GetPixels().Length;

            _doors = GetComponentsInChildren<DoorController>().ToList();
            _cleaningEffect.Stop();
        }

        private void Update()
        {
            if (!IsActive || IsCompleted)
            {
                return;
            }
            
            DustCleaner.CleanPlayer(_hero.transform.position);

            _timeStamp -= Time.deltaTime;
            if (_timeStamp <= 0)
            {
                StartCoroutine(CalculateCleaningRateCo());
                _timeStamp = UPDATE_RATE;
            }
        }

        private IEnumerator CalculateCleaningRateCo()
        {
            int iterations = 4;
            int x = 0;
            int y = 0;
            int width = Mathf.FloorToInt(_dust.sprite.texture.width/(float)iterations*2);
            int height = Mathf.FloorToInt(_dust.sprite.texture.height/(float)iterations*2);
            int pixelColored = 0;
            for (int i = 0; i < iterations * 0.5f; i++)
            {
                y = 0;
                for (int j = 0; j < iterations * 0.5f; j++)
                {
                    pixelColored += CalculateCleaningRate(x, y, width, height);
                    y += height;
                    yield return 0;
                }

                x += width;
                yield return 0;
            }

            CleanFactor = pixelColored / (float)_pixelSize;
            Signals.Get<OnRoomCleaningRateChanged>().Dispatch(CleanFactor);
        }

        private int CalculateCleaningRate(int x, int y, int width, int height)
        {
            var pixels = _dust.sprite.texture.GetPixels(x, y, width, height);
            var pixelColored = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a == 0)
                {
                    pixelColored++;
                }
            }

            return pixelColored;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            if (IsActive)
            {
                Init();
            }
            
            if (_roomLock.gameObject.activeSelf && IsActive)
            { 
                _roomLock.gameObject.SetActive(false);
            }
        }

        public void Complete()
        {
            _dust.gameObject.SetActive(false);
            _doors.ForEach(door => door.OpenDoor());
            _walldust.gameObject.SetActive(false);
            IsCompleted = true;
            _cleaningEffect.gameObject.SetActive(true);
            _cleaningEffect.Stop();
            _cleaningEffect.Play();
        }
    }
}