using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public class BarricadeColliderController : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Vehicle")) return;

            var vehicleController = other.GetComponent<VehicleController>();

            if (vehicleController._currentCarState == VehicleController.CarState.Exit) return;
            
            //GamePlayManager.Instance.Check_For_Stop_CarEngine_Sound(vehicleController);
            
            if (!GameManager.Instance.isGameStart)
            {
                vehicleController._currentCarState = VehicleController.CarState.Exit;
                return;
            }
            
            if(Random.Range(0,3) == 0)
                SoundManager.inst.Play("CarExit " + Random.Range(0,2));

            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                case CurrentChallengeType.BossChallenge:
                {
                    if (GamePlayManager.Instance.Is_GameComplete(vehicleController))
                        StartCoroutine(GamePlayManager.Instance.LevelComplete());
                    break;
                }
                case CurrentChallengeType.UnblockChallenge:
                    StartCoroutine(GamePlayManager.Instance.LevelComplete());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            vehicleController._currentCarState = VehicleController.CarState.Exit;
        }
    }
}