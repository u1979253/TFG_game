using System.Collections;
using System.Collections.Generic;
using System.Linq;
using deVoid.Utils;
using Unity.Collections;
using UnityEngine;

namespace ggj25
{
    using Direction = DoorController.Direction;
    public class RoomController : MonoBehaviour
    {
        private const float UPDATE_RATE = 0.5f;
        
        [field: SerializeField] public SpriteRenderer MainArt { get; private set; }
        [SerializeField] private SpriteRenderer _dust;
        [SerializeField] private SpriteRenderer _walldust;
        [SerializeField] private SpriteRenderer _roomLock;
        [SerializeField] private ParticleSystem _cleaningEffect;
        //[field: SerializeField] public Transform CheckPoint { get; private set; }
        
        [SerializeField, ReadOnly]
        private List<DoorController> _doors;
        private Dictionary<Direction, DoorController> _MapDoors;
        public float CleanFactor { get; private set; }

        public Vector2Int RoomIndex { get; set; }
        private HeroController _hero;

        private Vector3 _previousPosition;
        private Color32[] _currentColor;
        public DustCleaner DustCleaner { get; private set; }
        private int _pixelSize;

        [SerializeField] private Collider2D _roomAreaCollider;

        private float _timeStamp;
        [SerializeField] private List<Transform> _spawnPoints;

        public bool IsActive { get; private set; }
        public bool IsCompleted { get; private set; }


        private void Awake()
        {
            // recopilar todas las puertas hijo y mapearlas por dirección
            _MapDoors = GetComponentsInChildren<DoorController>()
                     .ToDictionary(d => d.Dir, d => d);
        }
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

        public void OpenDoor(Direction dir)
        {
            if (_MapDoors.TryGetValue(dir, out var door))
                door.Open();
        }
        public void SpawnEnemies(List<GameObject> enemyPrefabs, int minCount, int maxCount)
        {
            if (_spawnPoints == null || _spawnPoints.Count == 0) return;
            if (enemyPrefabs == null || enemyPrefabs.Count == 0) return;

            int count = Random.Range(minCount, maxCount + 1);
            var available = new List<Transform>(_spawnPoints);

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int pi = Random.Range(0, available.Count);
                Transform spawn = available[pi];
                available.RemoveAt(pi);

                int ei = Random.Range(0, enemyPrefabs.Count);
                var prefab = enemyPrefabs[ei];

                // Instancia el enemigo como hijo de la sala
                var go = Instantiate(prefab, spawn.position, Quaternion.identity, this.transform);

                if (ei == 0)
                {
                    var spawner = go.GetComponent<SpikeSpawner>();
                    if (spawner != null)
                    {
                        bool shootLeft = spawn.position.x > transform.position.x;
                        Vector2 dir = shootLeft ? Vector2.left : Vector2.right;
                        spawner.SetShootDirection(dir);

                        // Si dispara a la izquierda, invertimos el sprite en X:
                        if (shootLeft)
                        {
                            var ls = go.transform.localScale;
                            go.transform.localScale = new Vector3(-Mathf.Abs(ls.x), ls.y, ls.z);
                        }
                    }
                }
            }
        }
    }
}