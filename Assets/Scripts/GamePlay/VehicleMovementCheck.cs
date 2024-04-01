using System;
using UnityEngine;

namespace GamePlay
{
    public class VehicleMovementCheck : MonoBehaviour
    {
        private VehicleController _vehicleController;
        private void Start()
        {
            _vehicleController = GetComponent<VehicleController>();
        }

        public void Check_For_UpDown_Movement(Vector3 direction)
        {
            var bounds = _vehicleController._boxCollider.bounds;
            var center = bounds.center;

            float distance = 1000;
            var roadCollidedRay = 0;
            if (Physics.Raycast(center, direction, out var raycastHit, 500))
            {
                if (Check_Colliding_Object_Road(raycastHit.collider.gameObject)) roadCollidedRay++;

                if (Vector3.Distance(center, raycastHit.point) < distance)
                {
                    distance = Vector3.Distance(center, raycastHit.point);
                    _vehicleController.targetPos = raycastHit.point;
                }
            }

            var leftPos = bounds.center;
            leftPos.x = bounds.min.x;

            if (Physics.Raycast(leftPos, direction, out raycastHit, 500))
            {
                if (Check_Colliding_Object_Road(raycastHit.collider.gameObject)) roadCollidedRay++;

                if (Vector3.Distance(leftPos, raycastHit.point) < distance)
                {
                    distance = Vector3.Distance(leftPos, raycastHit.point);
                    _vehicleController.targetPos = raycastHit.point;
                }
            }

            var rightPos = bounds.center;
            rightPos.x = bounds.max.x;

            if (Physics.Raycast(rightPos, direction, out raycastHit, 500))
            {
                if (Check_Colliding_Object_Road(raycastHit.collider.gameObject)) roadCollidedRay++;

                if (roadCollidedRay >= 3)
                {
                    _vehicleController._currentCarState = VehicleController.CarState.OnPath;
                    return;
                }

                if (Vector3.Distance(rightPos, raycastHit.point) < distance)
                    _vehicleController.targetPos = raycastHit.point;
            }

            _vehicleController.collisionPos = _vehicleController.targetPos;
        }

        public void Check_For_LeftRight_Movement(Vector3 direction)
        {
            var bounds = _vehicleController._boxCollider.bounds;
            var center = bounds.center;

            float distance = 1000;
            var roadCollidedRay = 0;
            if (Physics.Raycast(center, direction, out var raycastHit, 500))
            {
                if (Check_Colliding_Object_Road(raycastHit.collider.gameObject)) roadCollidedRay++;

                if (Vector3.Distance(center, raycastHit.point) < distance)
                {
                    distance = Vector3.Distance(center, raycastHit.point);
                    _vehicleController.targetPos = raycastHit.point;
                }
            }

            var downPos = bounds.center;
            downPos.z = bounds.min.z;

            if (Physics.Raycast(downPos, direction, out raycastHit, 500))
            {
                if (Check_Colliding_Object_Road(raycastHit.collider.gameObject)) roadCollidedRay++;

                if (Vector3.Distance(downPos, raycastHit.point) < distance)
                {
                    distance = Vector3.Distance(downPos, raycastHit.point);
                    _vehicleController.targetPos = raycastHit.point;
                }
            }

            var upPos = bounds.center;
            upPos.z = bounds.max.z;

            if (Physics.Raycast(upPos, direction, out raycastHit, 500))
            {
                if (Check_Colliding_Object_Road(raycastHit.collider.gameObject)) roadCollidedRay++;

                if (roadCollidedRay >= 3)
                {
                    _vehicleController._currentCarState = VehicleController.CarState.OnPath;
                    return;
                }

                if (Vector3.Distance(upPos, raycastHit.point) < distance)
                    _vehicleController.targetPos = raycastHit.point;
            }
            _vehicleController.collisionPos = _vehicleController.targetPos;
        }
        private bool Check_Colliding_Object_Road(GameObject collidedObject)
        {
            return collidedObject.CompareTag("Road");
        }
    }
}
