using System;
using System.Linq;
using Manager;
using UnityEngine;

namespace GamePlay
{
    public enum SwipeDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    public class TouchDetectController : MonoBehaviour
    {
        public static TouchDetectController Inst;

        private Vector3 _startPos;
        private Vector3 _endPos;
        [HideInInspector] public GameObject _clickedObject;
        public SwipeDirection currentSwipeDirection = SwipeDirection.None;
        [SerializeField] private Camera mainCam;

        private void Awake()
        {
            Inst = this;
        }

        private void Update()
        {
            if (!GameManager.Instance.isGameStart || GamePlayController.Instance == null ||
                !GamePlayManager.Instance.Is_Move_Available())
                return;

            if (Input.GetMouseButtonDown(0))
            {
                var ray = mainCam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    var hitCollider = hit.collider;
                    if (hitCollider && hitCollider.gameObject.CompareTag("Vehicle"))
                    {
                        var vehicleController = hitCollider.gameObject.GetComponent<VehicleController>();
                        if (vehicleController != null && LevelGenerator.Instance.isHintTutorial &&
                            !IsCurrentTutorialCar(vehicleController)) return;
                        
                        _startPos = Input.mousePosition;
                        _clickedObject = hitCollider.gameObject;
                    }
                }
            }

            if (Input.GetMouseButtonUp(0) && _clickedObject != null)
            {
                var vehicleController = _clickedObject.GetComponent<VehicleController>();
                
                if (LevelGenerator.Instance.isHintTutorial && !IsCurrentTutorialCar(vehicleController)) return;

                if (vehicleController.Is_Moving_In_Field() ||
                    vehicleController._currentCarState is VehicleController.CarState.OnPath
                        or VehicleController.CarState.Exit ||
                    Is_Any_Other_Vehicle_Move_OtherDirection(vehicleController))
                    return;

                _endPos = Input.mousePosition;
                Decide_Swipe_Direction(_startPos, _endPos);
                
                if (LevelGenerator.Instance.isHintTutorial)
                {
                    if(currentSwipeDirection != HintController.Instance.GetCarSwipe())
                        return;
                    
                    HintController.Instance.VehicleAnimation(vehicleController);
                    return;
                }

                if (LevelGenerator.Instance.isTutorial &&
                    currentSwipeDirection != FirstLevelTutorial.Instance.GetCarSwipe())
                    return;

                if (currentSwipeDirection != SwipeDirection.None)
                    GamePlayManager.Instance.MoveVehicle(vehicleController, currentSwipeDirection);
            }
        }
        
        private bool IsCurrentTutorialCar(VehicleController vehicleController)
        {
            return vehicleController.carNumber == HintController.Instance.Get_CurrentCarNumber();
        }

        private void Decide_Swipe_Direction(Vector3 start, Vector3 end)
        {
            if (start == end || Vector3.Distance(start, end) < 3)
            {
                currentSwipeDirection = SwipeDirection.None;
                _clickedObject = null;
                return;
            }

            var isHorizontal = Mathf.Abs(end.x - start.x) > Mathf.Abs(end.y - start.y);

            if (isHorizontal)
                currentSwipeDirection = end.x > start.x ? SwipeDirection.Right : SwipeDirection.Left;
            else
                currentSwipeDirection = end.y > start.y ? SwipeDirection.Up : SwipeDirection.Down;
        }

        private bool Is_Any_Other_Vehicle_Move_OtherDirection(VehicleController vehicleController)
        {
            foreach (var item in LevelGenerator.Instance.vehicleControllers.Where(item => item.Is_Moving_In_Field()))
            {
                if (item.Get_CurrentOrientation() != vehicleController.Get_CurrentOrientation())
                    return true;

                //same line check.
                switch (vehicleController.Get_CurrentOrientation())
                {
                    case VehicleOrientation.Horizontal:
                        if (Math.Abs(item.transform.position.z - vehicleController.transform.position.z) < 2)
                            return true;
                        break;
                    case VehicleOrientation.Verticle:
                        if (Math.Abs(item.transform.position.x - vehicleController.transform.position.x) < 2)
                            return true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return false;
        }

        public void Set_ClickedObjectNull()
        {
            _clickedObject = null;
        }
    }
}