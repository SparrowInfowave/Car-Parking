using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AddressableManager;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay
{
    [RequireComponent(typeof(VehicleMovementCheck))]
    public class VehicleController : MonoBehaviour
    {
        public enum CarState
        {
            OnField,
            OnPath,
            Exit
        }

        private VehicleOrientation _currentVehicleOrientation = VehicleOrientation.Verticle;
        private SwipeDirection _lastSwipeDirection = SwipeDirection.None;
        private float _halfLength = 0f;
        private bool _canMove = false;
        private VehicleMovementCheck _vehicleMovementCheck;
        public BoxCollider _boxCollider;
        [HideInInspector] public CarState _currentCarState = CarState.OnField;
        [HideInInspector] public Vector3 targetPos, collisionPos;
        private Vector3 _pathCheckTargetPos = Vector3.zero;

        private bool _isMovingInField = false;

        public int carNumber;

        [SerializeField] private Transform trailTransform;
        
        private Vector3 _startPos = Vector3.zero;
        
        private IEnumerator Start()
        {
            LevelGenerator.Instance.vehicleControllers.Add(this);
            _boxCollider = GetComponent<BoxCollider>();
            yield return new WaitForSeconds(0.1f);
            _vehicleMovementCheck = GetComponent<VehicleMovementCheck>();
            Set_HalfLength();
            StartCoroutine(Refresh_Collider());
        }

        public Vector3 GetHandPos()
        {
            return Vector3.Lerp(transform.position,Camera.main.transform.position, 0.2f);
        }
        
        

        public void SetStartPos()
        {
            _startPos = transform.localPosition;
        }

        private void Exit()
        {
            if (_currentCarState is CarState.OnField or CarState.Exit) return;
            GamePlayManager.Instance.Start_Exit_Car(this, Get_Car_Reverse_Or_Not(targetPos));
        }

        private void After_Collision_Set_Position()
        {
            Vector3 currentPos;
            var distanceToMaintain = 1f;
            switch (_lastSwipeDirection)
            {
                case SwipeDirection.None:
                    return;
                case SwipeDirection.Left:
                    currentPos = targetPos + new Vector3(distanceToMaintain, 0, 0);
                    currentPos.z = transform.position.z;
                    transform.position = currentPos;
                    break;
                case SwipeDirection.Right:
                    currentPos = targetPos + new Vector3(-distanceToMaintain, 0, 0);
                    currentPos.z = transform.position.z;
                    transform.position = currentPos;
                    break;
                case SwipeDirection.Up:
                    currentPos = targetPos + new Vector3(0, 0, -distanceToMaintain);
                    currentPos.x = transform.position.x;
                    transform.position = currentPos;
                    break;
                case SwipeDirection.Down:
                    currentPos = targetPos + new Vector3(0, 0, distanceToMaintain);
                    currentPos.x = transform.position.x;
                    transform.position = currentPos;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void Set_Target()
        {
            _swipeDirectionContinuous = TouchDetectController.Inst.currentSwipeDirection;
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            switch (_lastSwipeDirection)
            {
                case SwipeDirection.None:
                    break;
                case SwipeDirection.Left:
                    _vehicleMovementCheck.Check_For_LeftRight_Movement(
                        GamePlayManager.Get_Direction_Vector(SwipeDirection.Left));
                    break;
                case SwipeDirection.Right:
                    _vehicleMovementCheck.Check_For_LeftRight_Movement(
                        GamePlayManager.Get_Direction_Vector(SwipeDirection.Right));
                    break;
                case SwipeDirection.Up:
                    _vehicleMovementCheck.Check_For_UpDown_Movement(
                        GamePlayManager.Get_Direction_Vector(SwipeDirection.Up));
                    break;
                case SwipeDirection.Down:
                    _vehicleMovementCheck.Check_For_UpDown_Movement(
                        GamePlayManager.Get_Direction_Vector(SwipeDirection.Down));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            gameObject.layer = LayerMask.NameToLayer("Obstacle");

            var vehiclePosition = transform.position;
            targetPos = new Vector3(targetPos.x, vehiclePosition.y, targetPos.z);
            _pathCheckTargetPos = targetPos;
            targetPos += GamePlayManager.Instance.Get_OffSet(_halfLength);
            targetPos = GamePlayManager.Instance.Set_TargetPos_X_Or_Z(targetPos, vehiclePosition);
            
            var distance = Vector3.Distance(vehiclePosition, targetPos);

            _infieldMove = transform.DOMove(targetPos, GamePlayManager.Calculate_InParking_Speed(distance))
                .SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    if (!_isMovingInField)
                        _isMovingInField = true;
                })
                .OnComplete(OnReachBoundary);

            if (_currentCarState == CarState.OnPath)
            {
                SoundManager.inst.Play("CarDrift");
                if(!SoundManager.inst.Get_Is_Playing_Sound("CarEngine"))
                    SoundManager.inst.Play("CarEngine",true,0);
            }
        }

        private TweenerCore<Vector3, Vector3, VectorOptions> _infieldMove = null;

        public bool Is_Moving_In_Field()
        {
            return _isMovingInField;
        }

        public void Set_IsMoving_InField(bool isOn)
        {
            _isMovingInField = isOn;
        }


        private void OnReachBoundary()
        {
            if (_currentCarState == CarState.Exit)
                return;
            
            if (_currentCarState == CarState.OnPath)
            {
                InvokeRepeating(nameof(Check_All_Car_Passed), 0f, 0.3f);
                GamePlayController.Instance.Check_Stop_Time_CountDown();
                Set_IsMoving_InField(false);
            }
            else
            {
                HapticSoundController.Instance.HapticSoundMedium();
                SoundManager.inst.Play("CarHitDefault");
                if(Random.Range(0,3) != 0)
                    SoundManager.inst.Play("CarHit " + Random.Range(0,5));
                GamePlayManager.Instance.Do_Shake(this,TouchDetectController.Inst.currentSwipeDirection);
                LevelGenerator.Instance.Make_Collision_Effect(collisionPos);
            }

            After_Collision_Set_Position();
            
            if (LevelGenerator.Instance.isHintTutorial)
                HintController.Instance.IncreaseCarNumber(carNumber);
        }

        private void Set_HalfLength()
        {
            var bounds = _boxCollider.bounds;
            _halfLength = _currentVehicleOrientation switch
            {
                VehicleOrientation.Horizontal => (bounds.max.x - bounds.min.x) / 2f,
                VehicleOrientation.Verticle => (bounds.max.z - bounds.min.z) / 2f,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public float Get_Half_Length()
        {
            return _halfLength;
        }

        private bool Get_Car_Reverse_Or_Not(Vector3 tempTargetPos)
        {
            var dir = transform.position - tempTargetPos;
            return Vector3.Angle(transform.forward, dir) < 150f;
        }

        public SwipeDirection Get_Last_SwipeDirection()
        {
            return _lastSwipeDirection;
        }

        public void Set_Last_SwipeDirection(SwipeDirection swipeDirection)
        {
            _lastSwipeDirection = swipeDirection;
        }

        public void Destroy_Car()
        {
            Destroy(gameObject);
        }

        private CarState Get_Current_CarState()
        {
            return _currentCarState;
        }

        public VehicleOrientation Get_CurrentOrientation()
        {
            return _currentVehicleOrientation;
        }

        public void Set_Orientation(VehicleOrientation vehicleOrientation)
        {
            _currentVehicleOrientation = vehicleOrientation;
        }

        public IEnumerator On_Collide_With_Moving_Object()
        {
            _currentCarState = CarState.OnField;
            StopInFieldMovement();
            yield return new WaitForEndOfFrame();
            GameManager.Instance.StartCoroutine(GameManager.Instance.GameOver(2f,GameOverReason.HitPerson));
        }

        public void StopInFieldMovement()
        {
            _infieldMove.Kill();
            Set_IsMoving_InField(false);
            _currentCarState = CarState.OnField;
            gameObject.layer = LayerMask.NameToLayer("Obstacle");
            CancelInvoke(nameof(Check_All_Car_Passed));
        }

        private bool _isOneTimeShakeOnBoundary = true;
        
        private void Check_All_Car_Passed()
        {
            if (_canMove)
            {
                CancelInvoke(nameof(Check_All_Car_Passed));
                return;
            }

            var thisVehicleNearPathPoint = LevelGenerator.Instance.Get_Nearest_PathPoint_Index(_pathCheckTargetPos);

            var nearPassingVehicle = LevelGenerator.Instance.vehicleControllers
                .Where(vehicleController =>
                    (vehicleController.Get_Current_CarState() is CarState.OnPath or CarState.Exit) &&
                    vehicleController != this && vehicleController &&
                    vehicleController._canMove).Where(vehicleController =>
                {
                    var otherVehiclePosition = vehicleController.transform.position;
                    return LevelGenerator.Instance.Get_Nearest_PathPoint_Index(otherVehiclePosition) -
                           thisVehicleNearPathPoint < 6 &&
                           LevelGenerator.Instance.Get_Nearest_PathPoint_Index(otherVehiclePosition) -
                           thisVehicleNearPathPoint > -14;
                });
            
            if (nearPassingVehicle.Any())
            {
                _canMove = false;
                if (_isOneTimeShakeOnBoundary)
                {
                    SwipeAnimationContinuous();
                    GamePlayManager.Instance.Do_Rotate(this.gameObject, TouchDetectController.Inst.currentSwipeDirection, false);
                    SoundManager.inst.Play("CarHorn");
                    
                    if(Random.Range(0,2) == 0)
                        GamePlayManager.Instance.Set_Emoji_For_Car(nearPassingVehicle.First().transform, EmojiCarType.OutSideCar);
                    
                    _isOneTimeShakeOnBoundary = false;
                }

                return;
            }
            CancelInvoke(nameof(InvokeShake));
            _canMove = true;
            Exit();
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            CancelInvoke(nameof(Check_All_Car_Passed));
        }

        private SwipeDirection _swipeDirectionContinuous = SwipeDirection.Left;
        private void SwipeAnimationContinuous()
        {
            InvokeRepeating(nameof(InvokeShake),0f,1f);
        }

        internal TweenerCore<Vector3,DG.Tweening.Plugins.Core.PathCore.Path,PathOptions> shakeTween = null;
        private void InvokeShake()
        {
            if(Random.Range(0,2) == 0)
                GamePlayManager.Instance.Set_Emoji_For_Car(transform, EmojiCarType.WaitCar);
            
            GamePlayManager.Instance.Do_Shake(this,_swipeDirectionContinuous, false);
        }

        private IEnumerator Refresh_Collider()
        {
            _boxCollider.enabled = false;
            yield return new WaitForEndOfFrame();
            _boxCollider.enabled = true;
        }

        public void Enable_Trail()
        {
            CarTrailSpawner.Instance.SpawnCarTrail(new CarTrailSpawner.CarTrailDataForSet
            {
                Parent = this.transform,
                TrailTransform = trailTransform
            });
        }
        
        public void SetVehicleAtDefaultPos()
        {
            transform.localPosition = _startPos;
        }


        private void OnDestroy()
        {
            LevelGenerator.Instance?.vehicleControllers.Remove(this);
            GamePlayManager.Instance.Check_For_Stop_CarEngine_Sound(this);
        }
    }
}