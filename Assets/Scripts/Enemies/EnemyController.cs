using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ggj25
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField]
        private EnemyConfig _enemyConfig;

        private float _timeStamp;

        private bool _jumping;

        private Rigidbody2D _rigidbody2D;

        private RoomController _room;

        private void Start()
        {
            WaitForJump();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _room = GetComponentInParent<RoomController>();
        }

        private void Update()
        {
            if (_room == null)
            {
                return;
            }
            if (!_room.IsActive)
            {
                return;
            }
            if (_jumping)
            {
                CheckJumpStop();
                return;
            }
            
            _timeStamp -= Time.deltaTime;
            if (_timeStamp < 0)
            {
                Jump();
            }
            _rigidbody2D.velocity *= _enemyConfig.Friction;
        }

        private void CheckJumpStop()
        {
            if (_rigidbody2D.velocity.magnitude < _enemyConfig.SpeedToConsiderStopped)
            {
                Paint();
                WaitForJump();
                _rigidbody2D.velocity = Vector2.zero;
            }
        }

        private void WaitForJump()
        {
            _timeStamp = _enemyConfig.JumpInterval;
            _jumping = false;
        }

        private void Paint()
        {
            GameManager.Instance.LevelManager.DustCleaner.PaintOther(transform.position, 
                                                                     _enemyConfig.AreaToPaint, 
                                                                     _enemyConfig.ColorToPaint);
        }

        private void Jump()
        {
            _jumping = true;
            var jump = Random.insideUnitCircle.normalized * _enemyConfig.JumpDistance;
            _rigidbody2D.AddForce(jump);
        }
    }
}
