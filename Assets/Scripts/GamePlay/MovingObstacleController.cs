using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public class MovingObstacleController : MonoBehaviour
    {
        public enum PathEndAnimation
        {
            Normal,
            Jump
        }

        private List<Vector3> _listPathData = new List<Vector3>();

        private TweenerCore<Vector3, Path, PathOptions> _pathAnim = null;
        private TweenerCore<Vector3, Path, PathOptions> _jumpPath;
        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _JumpRot = null;

        private Vector3 _nextPosition = Vector3.zero;
        private const float AnimSpeed = 1f;

        private readonly float _offSetInY = -4f;

        [SerializeField] private PathEndAnimation _pathEndAnimation = PathEndAnimation.Normal;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);

            if (LevelGenerator.Instance.rotateType == RotateType.Reverse)
                Reverse_Path();

            StartAnimation();
        }

        private void StartAnimation()
        {
            _listPathData.Reverse();
            transform.position = _listPathData[0];
            _pathAnim = transform
                .DOPath(_listPathData.ToArray(), Get_AnimationSpeed(_listPathData.Count), PathType.CatmullRom)
                .SetLookAt(0.4f).SetEase(Ease.Linear).SetLookAt(-1f).OnUpdate(OnAnimationUpdate)
                .OnComplete(GiveRotation);
        }

        private void GiveRotation()
        {
            var time = 0.6f;
            var euler = transform.eulerAngles;
            switch (_pathEndAnimation)
            {
                case PathEndAnimation.Normal:
                    _JumpRot = transform.DORotate(euler + new Vector3(0, 180, 0), time).SetEase(Ease.Linear)
                        .OnComplete(StartAnimation);
                    break;
                case PathEndAnimation.Jump:
                    _JumpRot = transform.DORotate(euler + new Vector3(0, 180, 0), time).SetEase(Ease.Linear);
                    var positions = new List<Vector3>();
                    var position = transform.position;
                    positions.Add(position);
                    positions.Add(position + new Vector3(0, 7, 0));
                    positions.Add(position);
                    _jumpPath = transform.DOPath(positions.ToArray(), time, PathType.CatmullRom).SetEase(Ease.Linear)
                        .OnComplete(StartAnimation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Pause_Animation()
        {
            _pathAnim.Pause();
        }

        private void Resume_Animation()
        {
            _pathAnim.Play();
        }

        private void OnAnimationUpdate()
        {
            if (Is_Object_In_Front())
            {
                Pause_Animation();
                InvokeRepeating(nameof(ContinuousCheckAfterAnimationPause), 0.5f, 0.5f);
            }
        }

        private bool Is_Object_In_Front()
        {
            if (Physics.Raycast(transform.position + new Vector3(0, 3, 0), transform.forward, out var hit, 6))
            {
                if (hit.collider == null)
                    return true;

                if (hit.collider.CompareTag("Vehicle"))
                    return true;
            }

            return false;
        }

        private void ContinuousCheckAfterAnimationPause()
        {
            if (!Is_Object_In_Front())
            {
                Resume_Animation();
                CancelInvoke(nameof(ContinuousCheckAfterAnimationPause));
            }
        }

        public void Set_Path(IEnumerable<Float_Vector> pathData)
        {
            _listPathData = pathData.Select(item =>
            {
                var pos = LevelGenerator.Instance.Get_Vector(item);
                pos.y = _offSetInY;
                return pos;
            }).ToList();
            transform.position = _listPathData[0];
        }

        private bool _isCollided = false;

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Vehicle") && !_isCollided &&
                other.GetComponent<VehicleController>().Is_Moving_In_Field())
            {
                _pathAnim?.Kill();
                _jumpPath?.Kill();
                _JumpRot?.Kill();

                StartCoroutine(Throw_Boy());
                var onCollideWithMovingObject = other.GetComponent<VehicleController>().On_Collide_With_Moving_Object();
                StartCoroutine(onCollideWithMovingObject);
                _isCollided = true;
            }
        }

        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _boyRot = null;

        private IEnumerator Throw_Boy()
        {
            SoundManager.inst.Play("BoyHit");
            GameManager.Instance.StartCoroutine(GameManager.Instance.Set_Temporary_Gravity());
            GetComponent<BoxCollider>().isTrigger = false;
            var rigidbody1 = GetComponent<Rigidbody>();

            var animator = GetComponentInChildren<Animator>();
            if(animator != null)
                animator.enabled = false;
            
            rigidbody1.useGravity = true;
            rigidbody1.AddForce(GamePlayManager.Instance.Get_Boy_ThrowForceVector());
            yield return new WaitForSeconds(0.3f);
            _boyRot = transform
                .DORotate(new Vector3(Random.Range(180, 360), Random.Range(180, 360), Random.Range(180, 360)), 3f)
                .OnUpdate(
                    () =>
                    {
                        if (transform.position.y < 10)
                            _boyRot.Kill();
                    });
        }

        private void Reverse_Path()
        {
            var planeBound = LevelGenerator.Instance.Get_PlaneBound();

            var offsetList = _listPathData.Select(item =>
            {
                var pos = planeBound.max - item;
                pos.y = _offSetInY;
                return pos;
            }).ToList();
            _listPathData = offsetList.Select(item =>
            {
                var pos = planeBound.min + item;
                pos.y = _offSetInY;
                return pos;
            }).ToList();
            transform.position = _listPathData[0];
        }

        private float Get_AnimationSpeed(int count)
        {
            return count / AnimSpeed;
        }
    }
}