using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Manager;
using ThemeSelection;
using UnityEngine;

namespace GamePlay
{
    public enum EmojiCarType
    {
        InsideCar,
        WaitCar,
        OutSideCar
    }

    public class GamePlayManager : SingletonComponent<GamePlayManager>
    {
        public Vector3 Get_OffSet(float halfLength)
        {
            return TouchDetectController.Inst.currentSwipeDirection switch
            {
                SwipeDirection.Left => new Vector3(halfLength, 0, 0),
                SwipeDirection.Right => new Vector3(-halfLength, 0, 0),
                SwipeDirection.Up => new Vector3(0, 0, -halfLength),
                SwipeDirection.Down => new Vector3(0, 0, halfLength),
                SwipeDirection.None => Vector3.zero,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public Vector3 Set_TargetPos_X_Or_Z(Vector3 targetPos, Vector3 vehiclePos)
        {
            switch (TouchDetectController.Inst.currentSwipeDirection)
            {
                case SwipeDirection.Left:
                case SwipeDirection.Right:
                    return new Vector3(targetPos.x, vehiclePos.y, vehiclePos.z);
                case SwipeDirection.Up:
                case SwipeDirection.Down:
                    return new Vector3(vehiclePos.x, vehiclePos.y, targetPos.z);
                case SwipeDirection.None:
                default:
                    return targetPos;
            }
        }

        public void MoveVehicle(VehicleController vehicleController, SwipeDirection swipeDirection)
        {
            vehicleController.Set_Last_SwipeDirection(swipeDirection);

            var currentOrientation = vehicleController.Get_CurrentOrientation();

            switch (vehicleController.Get_Last_SwipeDirection())
            {
                case SwipeDirection.None:
                    return;
                case SwipeDirection.Left:
                    if (currentOrientation != VehicleOrientation.Horizontal) return;
                    break;
                case SwipeDirection.Right:
                    if (currentOrientation != VehicleOrientation.Horizontal) return;
                    break;
                case SwipeDirection.Up:
                    if (currentOrientation != VehicleOrientation.Verticle) return;
                    break;
                case SwipeDirection.Down:
                    if (currentOrientation != VehicleOrientation.Verticle) return;
                    break;
                default:
                    return;
            }

            vehicleController.Set_Target();
            GamePlayController.Instance.Check_Move_And_Time();
        }

        public void Start_Exit_Car(VehicleController vehicleController, bool isReverse)
        {
            LevelGenerator.Instance.StartCoroutine(Exit_Car(vehicleController, isReverse));
        }

        public void ResetLevelInGamePlay()
        {
            var vehicleController = FindObjectsOfType<VehicleController>();

            foreach (var item in vehicleController)
            {
                item.SetVehicleAtDefaultPos();
            }
        }

        private IEnumerator Exit_Car(VehicleController vehicleController, bool isReverse)
        {
            if (LevelGenerator.Instance.isTutorial)
                FirstLevelTutorial.Instance.IncreaseCarNumber();
            const float toTheRoadDuration = 0.2f;

            var vehicleTransform = vehicleController.transform;
            vehicleController.shakeTween?.Kill();
            vehicleTransform.DOMove(Road_Center_Position_For_Car(vehicleController), toTheRoadDuration);

            yield return new WaitForSeconds(toTheRoadDuration);

            const float turnSpeed = 0.2f;
            if (isReverse)
            {
                vehicleController.transform.DORotate(new Vector3(0, Rotate_To(vehicleController), 0),
                    turnSpeed);
                vehicleController.transform.DOMove(Reverse_Move_Turn_Pos(vehicleController), turnSpeed);

                yield return new WaitForSeconds(0.2f);
            }

            float distance = 500;
            var closestPoint = Vector3.zero;
            var vehicleFrontOrBack = vehicleController.transform.position;


            foreach (var point in LevelGenerator.Instance.Get_PathPoints().Where(point =>
                         !Check_Behind_Point(point, vehicleController) &&
                         Vector3.Distance(point, vehicleFrontOrBack) < distance))
            {
                distance = Vector3.Distance(point, vehicleFrontOrBack);
                closestPoint = point;
            }

            //Whole Path.
            var startPointIndex = LevelGenerator.Instance.Get_PathPoints().IndexOf(closestPoint);
            var path = LevelGenerator.Instance.Get_PathPoints().Where((t, i) => i >= startPointIndex).ToList();

            vehicleController.Enable_Trail();
            vehicleController.transform.DOPath(path.ToArray(), Calculate_Speed_ExitPath(path.Count)).SetEase(Ease.Linear).SetLookAt(0.02f)
                .SetUpdate(UpdateType.Fixed).OnUpdate(() => Check_Barricade_Arm(vehicleController.transform.position))
                .OnComplete(vehicleController.Destroy_Car);
        }

        private void Check_Barricade_Arm(Vector3 vehiclePos)
        {
            var posOfBarricade = LevelGenerator.Instance.Get_Barricade_Position();

            if (Mathf.Abs(vehiclePos.x - posOfBarricade.x) > (RoadController.Instance.Get_RoadWidth() / 2f + 3f))
                return;

            if (vehiclePos.z > posOfBarricade.z && vehiclePos.z - posOfBarricade.z < 100)
                BarricadeArmMovement.Instance.OpenBarricade();

            if (vehiclePos.z < posOfBarricade.z)
                BarricadeArmMovement.Instance.Close_Barricade();
        }

        private float Calculate_Speed_ExitPath(int pathLength)
        {
            return pathLength / GameManager.Instance.mainPathSpeed;
        }

        public static float Calculate_InParking_Speed(float distance)
        {
            return distance / GameManager.Instance.inParkingSpeed;
        }

        private Vector3 Road_Center_Position_For_Car(VehicleController vehicleController)
        {
            var vehiclePos = vehicleController.transform.position;
            var fullLength = 20;
            return vehicleController.Get_Last_SwipeDirection() switch
            {
                SwipeDirection.Left => new Vector3(vehiclePos.x - fullLength, vehiclePos.y, vehiclePos.z),
                SwipeDirection.Right => new Vector3(vehiclePos.x + fullLength, vehiclePos.y, vehiclePos.z),
                SwipeDirection.Up => new Vector3(vehiclePos.x, vehiclePos.y, vehiclePos.z + fullLength),
                SwipeDirection.Down => new Vector3(vehiclePos.x, vehiclePos.y, vehiclePos.z - fullLength),
                _ => vehiclePos
            };
        }

        private Vector3 Reverse_Move_Turn_Pos(VehicleController vehicleController)
        {
            var vehiclePos = vehicleController.transform.position;
            var posFactor = vehicleController.Get_Half_Length() - 5;
            return vehicleController.Get_Last_SwipeDirection() switch
            {
                SwipeDirection.Left => new Vector3(vehiclePos.x - posFactor, vehiclePos.y, vehiclePos.z + posFactor),
                SwipeDirection.Right => new Vector3(vehiclePos.x + posFactor, vehiclePos.y, vehiclePos.z - posFactor),
                SwipeDirection.Up => new Vector3(vehiclePos.x + posFactor, vehiclePos.y, vehiclePos.z + posFactor),
                SwipeDirection.Down => new Vector3(vehiclePos.x - posFactor, vehiclePos.y, vehiclePos.z - posFactor),
                _ => vehiclePos
            };
        }

        private float Rotate_To(VehicleController vehicleController)
        {
            return vehicleController.Get_Last_SwipeDirection() switch
            {
                SwipeDirection.Left => 180,
                SwipeDirection.Right => 0,
                SwipeDirection.Up => -90,
                SwipeDirection.Down => 90,
                _ => 0
            };
        }

        private bool Check_Behind_Point(Vector3 point, VehicleController vehicleController)
        {
            var position = vehicleController.transform.position;
            var lastSwipeDirection = vehicleController.Get_Last_SwipeDirection();
            return lastSwipeDirection switch
            {
                SwipeDirection.Left => position.z < point.z,
                SwipeDirection.Right => position.z > point.z,
                SwipeDirection.Up => position.x < point.x,
                SwipeDirection.Down => position.x > point.x,
                _ => true
            };
        }

        public void Do_Shake(VehicleController vehicleController, SwipeDirection swipeDirection, bool isCollidedObjMove = true)
        {
            var transform1 = vehicleController.transform;
            var path = new List<Vector3>();
            var vehiclePos = transform1.position;
            path.Add(vehiclePos);
            switch (swipeDirection)
            {
                case SwipeDirection.None:
                    break;
                case SwipeDirection.Left:
                    path.Add(vehiclePos + new Vector3(-3, 0, 0));
                    path.Add(vehiclePos + new Vector3(+5, 0, 0));
                    path.Add(vehiclePos + new Vector3(+3, 0, 0));
                    break;
                case SwipeDirection.Right:
                    path.Add(vehiclePos + new Vector3(+3, 0, 0));
                    path.Add(vehiclePos + new Vector3(-5, 0, 0));
                    path.Add(vehiclePos + new Vector3(-3, 0, 0));
                    break;
                case SwipeDirection.Up:
                    path.Add(vehiclePos + new Vector3(0, 0, +3));
                    path.Add(vehiclePos + new Vector3(0, 0, -5));
                    path.Add(vehiclePos + new Vector3(0, 0, -3));
                    break;
                case SwipeDirection.Down:
                    path.Add(vehiclePos + new Vector3(0, 0, -3));
                    path.Add(vehiclePos + new Vector3(0, 0, +5));
                    path.Add(vehiclePos + new Vector3(0, 0, +3));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (GameManager.Instance.challengeType != CurrentChallengeType.UnblockChallenge &&
                vehicleController._currentCarState != VehicleController.CarState.OnField)
            {
                path.Add(vehiclePos);
            }

            if (LevelGenerator.Instance.isHintTutorial)
            {
                path.Add(vehiclePos);
            }

            transform1.DOPath(path.ToArray(), 0.3f, PathType.Linear, PathMode.Ignore).SetEase(Ease.OutSine).OnComplete(
                () =>
                {
                    if (TouchDetectController.Inst._clickedObject == vehicleController.gameObject)
                        TouchDetectController.Inst.Set_ClickedObjectNull();
                    vehicleController.Set_IsMoving_InField(false);
                });
            if (isCollidedObjMove)
                CollidedObjectMove(vehicleController, TouchDetectController.Inst.currentSwipeDirection);
        }


        private void CollidedObjectMove(Component vehicleController, SwipeDirection swipeDirection)
        {
            var collidedObjects = swipeDirection != SwipeDirection.None
                ? Check_Other_CollidedObject(Get_Direction_Vector(swipeDirection), vehicleController.gameObject)
                : new List<GameObject>();

            foreach (var item in collidedObjects)
            {
                Do_Rotate(item, swipeDirection);
            }
        }

        public void Do_Rotate(GameObject obj, SwipeDirection swipeDirection, bool isEmojiShow = true)
        {
            var rotationData = Get_Rotation(obj, swipeDirection);
            if (rotationData.Item1 == 0 && rotationData.Item2 == 0) return;

            var defaultRotation = obj.transform.eulerAngles;
            var rotateOffset = new Vector3(rotationData.Item1, 0, rotationData.Item2);

            var targetRot = defaultRotation + rotateOffset;
            var secondTargetRot = defaultRotation - (rotateOffset / 2f);
            const float halfAnimTime = 0.12f;
            var sequence = DOTween.Sequence();

            sequence.Append(obj.transform.DORotate(targetRot, halfAnimTime).SetEase(Ease.Linear));
            sequence.AppendCallback(() =>
            {
                if (obj.CompareTag("Vehicle") && isEmojiShow)
                    Set_Emoji_For_Car(obj.transform, EmojiCarType.InsideCar);
            });
            sequence.Append(obj.transform.DORotate(secondTargetRot, halfAnimTime).SetEase(Ease.Linear));
            sequence.Append(obj.transform.DORotate(defaultRotation, halfAnimTime).SetEase(Ease.OutSine));
        }

        private (float, float) Get_Rotation(GameObject obj, SwipeDirection swipeDirection)
        {
            var yAngle = obj.transform.eulerAngles.y;
            const float rotationAmount = 8f;
            float xRot = 0;
            float zRot = 0;
            if (Math.Abs(yAngle - 90) < 2)
            {
                switch (swipeDirection)
                {
                    case SwipeDirection.Left:
                        xRot = -rotationAmount;
                        break;
                    case SwipeDirection.Right:
                        xRot = rotationAmount;
                        break;
                    case SwipeDirection.Up:
                        zRot = rotationAmount;
                        break;
                    case SwipeDirection.Down:
                        zRot = -rotationAmount;
                        break;
                }
            }
            else
            {
                switch (swipeDirection)
                {
                    case SwipeDirection.Left:
                        zRot = rotationAmount;
                        break;
                    case SwipeDirection.Right:
                        zRot = -rotationAmount;
                        break;
                    case SwipeDirection.Up:
                        xRot = rotationAmount;
                        break;
                    case SwipeDirection.Down:
                        xRot = -rotationAmount;
                        break;
                }
            }

            return (xRot, zRot);
        }

        private List<GameObject> Check_Other_CollidedObject(Vector3 direction, GameObject vehicle)
        {
            var collidedObjects = new List<GameObject>();
            const int distance = 5;
            foreach (var item in Get_Object_CornerPos(vehicle))
            {
                var raycastHits = Physics.RaycastAll(item, direction, distance);
                foreach (var hit in raycastHits)
                {
                    if (hit.collider == null) continue;
                    if (hit.collider.gameObject == null) continue;
                    if (hit.collider.gameObject == vehicle) continue;

                    var obj = hit.collider.gameObject;
                    if (obj.CompareTag("Road") || obj.CompareTag("MovingObstacle"))
                        continue;
                    if (!collidedObjects.Contains(obj))
                        collidedObjects.Add(obj);
                }
            }

            return collidedObjects;
        }


        private List<Vector3> Get_Object_CornerPos(GameObject obj)
        {
            var bounds = obj.GetComponent<BoxCollider>().bounds;
            var min = bounds.min;
            var max = bounds.max;
            var center = bounds.center;
            var topLeft = new Vector3(min.x, center.y, max.z);
            var topRight = new Vector3(max.x, center.y, max.z);
            var bottomLeft = new Vector3(min.x, center.y, min.z);
            var bottomRight = new Vector3(max.x, center.y, min.z);

            return new List<Vector3> { topLeft, topRight, bottomLeft, bottomRight };
        }

        public static Vector3 Get_Direction_Vector(SwipeDirection swipeDirection)
        {
            return swipeDirection switch
            {
                SwipeDirection.None => Vector3.zero,
                SwipeDirection.Left => new Vector3(-1, 0, 0),
                SwipeDirection.Right => new Vector3(1, 0, 0),
                SwipeDirection.Up => new Vector3(0, 0, 1),
                SwipeDirection.Down => new Vector3(0, 0, -1),
                _ => Vector3.zero
            };
        }

        public bool Is_GameComplete(VehicleController vehicleController)
        {
            foreach (var item in LevelGenerator.Instance.vehicleControllers.Where(item => vehicleController != item))
            {
                switch (item._currentCarState)
                {
                    case VehicleController.CarState.OnField:
                        return false;
                    case VehicleController.CarState.OnPath:
                        return false;
                }
            }

            return true;
        }

        public IEnumerator LevelComplete()
        {
            if (GeneralDataManager.Instance.currentOpenedPopupList.Contains(GeneralDataManager.Popup
                    .LevelCompleteCoinReward)
                || GeneralDataManager.Instance.currentOpenedPopupList.Contains(GeneralDataManager.Popup
                    .LevelCompleteThemeReward)
                || GeneralDataManager.Instance.currentOpenedPopupList.Contains(GeneralDataManager.Popup
                    .BossChallengeComplete)
                || GeneralDataManager.Instance.currentOpenedPopupList.Contains(GeneralDataManager.Popup
                    .LevelCompleteThemeNotAvailable)
                || GeneralDataManager.Instance.currentOpenedPopupList.Contains(GeneralDataManager.Popup
                    .LevelCompleteCarSceneRentPopUp))
            {
                yield break;
            }

            GameManager.Instance.isGameStart = false;
            GameManager.Instance.Stop_Car_Sounds();
            GamePlayController.Instance.EnableParticleSystem();
            yield return new WaitForSeconds(2f);
            SoundManager.inst.Play("LevelComplete");

            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                case CurrentChallengeType.UnblockChallenge:
                    {
                        CommonGameData.IncreaseCurrentCompletedLevel();

                        if (CommonGameData.CurrentCompletedLevel >= CommonGameData.RoadAndTrailThemeRewardInterval &&
                            CommonGameData.CurrentCompletedLevel % CommonGameData.RoadAndTrailThemeRewardInterval == 0)
                        {
                            CommonGameData.IncreaseThemeRewardItemNumber();
                            GameManager.Instance.Show_Popup(GameManager.Instance.CanThemeRewardShow()
                                ? GeneralDataManager.Popup.LevelCompleteThemeReward
                                : GeneralDataManager.Popup.LevelCompleteThemeNotAvailable);
                        }
                        else
                        {
                            GameManager.Instance.Show_Popup(GeneralDataManager.Popup.LevelCompleteCoinReward);
                        }
                    }
                    break;
                case CurrentChallengeType.BossChallenge:
                    GameManager.Instance.Show_Popup(GeneralDataManager.Popup.BossChallengeComplete);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            GameManager.Instance.Hide_Screen();
        }

        public Vector3 Get_Boy_ThrowForceVector()
        {
            const int throwForce = 1500;
            const int upForce = 5000;

            return TouchDetectController.Inst.currentSwipeDirection switch
            {
                SwipeDirection.None => Vector3.zero,
                SwipeDirection.Left => new Vector3(-throwForce, upForce, 0),
                SwipeDirection.Right => new Vector3(throwForce, upForce, 0),
                SwipeDirection.Up => new Vector3(0, upForce, throwForce),
                SwipeDirection.Down => new Vector3(0, upForce, -throwForce),
                _ => Vector3.zero
            };
        }

        public void Reset_Level()
        {
            var theme = (Scene)ThemeSavedDataManager.EnvironmentThemeNumber;
            SceneController.Instance.LoadScene(theme, null,
                () =>
                {
                    GameManager.Instance.HideAllPopUp();
                    GameManager.Instance.Show_Screen(GeneralDataManager.Screen.GamePlay);
                });
        }

        public void Next_Level()
        {
            CommonGameData.IncreaseLevelNumber();
            var theme = (Scene)ThemeSavedDataManager.EnvironmentThemeNumber;
            SceneController.Instance.LoadScene(theme, null, () =>
            {
                GameManager.Instance.HideAllPopUp();
                GameManager.Instance.Show_Screen(GeneralDataManager.Screen.GamePlay);
                GamePlayController.Instance?.Reset_Level();
            });
        }

        public bool CanPressAnyButtonIn_GamePlay()
        {
            return (LevelGenerator.Instance.vehicleControllers.Any(x =>
                        x._currentCarState == VehicleController.CarState.OnField) && Is_Move_Available() &&
                    !LevelGenerator.Instance.isTutorial);
        }

        public void Check_For_Stop_CarEngine_Sound(VehicleController vehicleController)
        {
            var vehicles = new List<VehicleController>(LevelGenerator.Instance.vehicleControllers);

            if (!vehicles.Any(x => x._currentCarState == VehicleController.CarState.OnPath && x != vehicleController))
            {
                SoundManager.inst.Stop("CarEngine");
            }
        }

        public Vector3 Get_Reverse_Position(Vector3 position)
        {
            var planeBound = LevelGenerator.Instance.Get_PlaneBound();
            var offset = planeBound.max - position;
            var reversePos = planeBound.min + offset;
            reversePos.y = position.y;
            return reversePos;
        }

        public bool Is_Move_Available()
        {
            if (GameManager.Instance.challengeType == CurrentChallengeType.UnblockChallenge)
                return true;

            if (GameManager.Instance.challengeType == CurrentChallengeType.Level
                && CommonGameData.LevelNumber <= GameManager.Instance.startMoveInLevel)
                return true;

            if (GameManager.Instance.targetMoves <= 0)
                return false;

            return true;
        }

        public void Set_Emoji_For_Car(Transform vehicleTransform, EmojiCarType emojiCarType)
        {
            if (UnityEngine.Random.Range(1, 11) is 8 or 9 or 10) return;
            var emojiController = Instantiate(GamePlayController.Instance.upEmoji, vehicleTransform)
                .GetComponent<EmojiAnimationController>();
            var emojiPack = GamePlayController.Instance.GetEmojiPack(emojiCarType);
            emojiController.Up_Animation(emojiPack[UnityEngine.Random.Range(0, emojiPack.Length)],
                vehicleTransform.position);
        }
    }
}