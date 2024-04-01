using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Manager;
using UnityEngine;

namespace GamePlay
{
    public class HintController : SingletonComponent<HintController>
    {
        private List<SwipeDirection> _swipeDirections = new List<SwipeDirection>
            { SwipeDirection.Right, SwipeDirection.Up, SwipeDirection.Left };

        public int carNumber = 0;

        private TweenerCore<Vector3, Vector3, VectorOptions> _moveAnim = null;
        private List<int> amountTomove = new List<int>();
        private GameObject _hand = null;

        [HideInInspector] public List<int> carOrder = new List<int> { 0, 2, 1 };

        private Dictionary<int, VehicleController> vehicleIndexData = new Dictionary<int, VehicleController>();

        private void Start()
        {
            Set_DataForHint();
            var rot = GameManager.Instance.tutorialHand.transform.eulerAngles;
            var startPos = vehicleIndexData[Get_CurrentCarNumber()].GetHandPos();
            _hand = Instantiate(GameManager.Instance.tutorialHand, startPos, Quaternion.Euler(rot),
                GameManager.Instance.transform);
            HandAnimation(startPos);
        }

        private void HandAnimation(Vector3 startPos)
        {
            _moveAnim.Kill();
            _hand.transform.position = startPos;
            var targetPos = GetTargetPos(GetTargetPos(startPos));
            _moveAnim = _hand.transform.DOMove(targetPos, 1f).SetDelay(0.5f).SetLoops(-1)
                .SetEase(Ease.Linear);
        }

        private Vector3 GetTargetPos(Vector3 currentPos)
        {
            var swipeDir = _swipeDirections[carNumber];
            const int amount = 10;
            return swipeDir switch
            {
                SwipeDirection.Left => currentPos - new Vector3(amount, 0, 0),
                SwipeDirection.Right => currentPos + new Vector3(amount, 0, 0),
                SwipeDirection.Up => currentPos + new Vector3(0, 0, amount),
                SwipeDirection.Down => currentPos - new Vector3(0, 0, amount),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public SwipeDirection GetCarSwipe()
        {
            return _swipeDirections[carNumber];
        }

        public void IncreaseCarNumber(int swipedCarNumber)
        {
            if (swipedCarNumber != carOrder[carNumber]) return;

            if (carNumber < _swipeDirections.Count - 1)
            {
                carNumber++;
                var startPos = vehicleIndexData[Get_CurrentCarNumber()].GetHandPos();
                HandAnimation(startPos);
            }
            else
            {
                Destroy(_hand);
                LevelGenerator.Instance.UnlockCarRemoveHint();
            }
        }

        public void Set_DataForHint()
        {
            _swipeDirections.Clear();
            carOrder.Clear();

            var vehicleControllers = FindObjectsOfType<VehicleController>().ToList();
            vehicleIndexData = vehicleControllers.ToDictionary(item => item.carNumber);
            var hintData = LevelGenerator.Instance._UnblockCarChallengeData.hintDataList;

            foreach (var item in hintData)
            {
                switch (LevelGenerator.Instance.rotateType)
                {
                    case RotateType.Default:
                        _swipeDirections.Add(GetDirection(vehicleIndexData[item.index].Get_CurrentOrientation(),
                            item.direction < 0 ? -1 : 1));
                        break;
                    case RotateType.Reverse:
                        _swipeDirections.Add(GetDirection(vehicleIndexData[item.index].Get_CurrentOrientation(),
                            item.direction < 0 ? 1 : -1));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                carOrder.Add(item.index);
                amountTomove.Add(item.direction);
            }
        }

        private SwipeDirection GetDirection(VehicleOrientation orientation, int dir)
        {
            return orientation switch
            {
                VehicleOrientation.Horizontal => dir == -1 ? SwipeDirection.Left : SwipeDirection.Right,
                VehicleOrientation.Verticle => dir == -1 ? SwipeDirection.Down : SwipeDirection.Up,
                _ => SwipeDirection.Left
            };
        }

        public int Get_CurrentCarNumber()
        {
            return carOrder[carNumber];
        }

        TweenerCore<Vector3, Vector3, VectorOptions> moveAnim = null;

        public void VehicleAnimation(VehicleController vehicleController)
        {
            if (moveAnim != null && moveAnim.IsPlaying()) return;
            if (carNumber == carOrder.Count - 1)
            {
                GamePlayManager.Instance.MoveVehicle(vehicleController, _swipeDirections[carNumber]);
                IncreaseCarNumber(vehicleController.carNumber);
                return;
            }

            switch (_swipeDirections[carNumber])
            {
                case SwipeDirection.Left:
                    moveAnim = vehicleController.transform
                        .DOMove(
                            vehicleController.transform.position -
                            new Vector3(Mathf.Abs(amountTomove[carNumber] * 20), 0, 0),
                            CalculateSpeed(amountTomove[carNumber]))
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            GamePlayManager.Instance.Do_Shake(vehicleController,_swipeDirections[carNumber]);
                            IncreaseCarNumber(vehicleController.carNumber);
                            moveAnim = null;
                        });
                    break;
                case SwipeDirection.Right:
                    moveAnim = vehicleController.transform
                        .DOMove(
                            vehicleController.transform.position +
                            new Vector3(Mathf.Abs(amountTomove[carNumber] * 20), 0, 0),
                            CalculateSpeed(amountTomove[carNumber]))
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            GamePlayManager.Instance.Do_Shake(vehicleController,_swipeDirections[carNumber]);
                            IncreaseCarNumber(vehicleController.carNumber);
                            moveAnim = null;
                        });
                    break;
                case SwipeDirection.Up:
                    moveAnim = vehicleController.transform
                        .DOMove(
                            vehicleController.transform.position +
                            new Vector3(0, 0, Mathf.Abs(amountTomove[carNumber] * 20)),
                            CalculateSpeed(amountTomove[carNumber]))
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            GamePlayManager.Instance.Do_Shake(vehicleController,_swipeDirections[carNumber]);
                            IncreaseCarNumber(vehicleController.carNumber);
                            moveAnim = null;
                        });
                    break;
                case SwipeDirection.Down:
                    moveAnim = vehicleController.transform
                        .DOMove(
                            vehicleController.transform.position -
                            new Vector3(0, 0, Mathf.Abs(amountTomove[carNumber] * 20)),
                            CalculateSpeed(amountTomove[carNumber]))
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            GamePlayManager.Instance.Do_Shake(vehicleController,_swipeDirections[carNumber]);
                            IncreaseCarNumber(vehicleController.carNumber);
                            moveAnim = null;
                        });
                    break;
                case SwipeDirection.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float CalculateSpeed(float amountToMove)
        {
            const float speed = 10f;
            return Mathf.Abs(amountToMove) / speed;
        }

        private void OnDestroy()
        {
            LevelGenerator.Instance.isHintTutorial = false;
            Destroy(_hand);
            GamePlayController.Instance?.Check_HintObj();
        }
    }
}