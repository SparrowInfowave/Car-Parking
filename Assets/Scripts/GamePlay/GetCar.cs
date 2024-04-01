using System;
using AddressableManager;
using Manager;
using ThemeSelection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public enum CarType
    {
        X3,
        X4,
        X5
    }

    public class GetCar : SingletonComponent<GetCar>
    {
        private readonly int MainTex = Shader.PropertyToID("_BaseMap");

        private CarThemeData _carThemeData;
        
        public void Set_Car_Data()
        {
            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                case CurrentChallengeType.BossChallenge:
                {
                    _carThemeData =
                        Resources.Load<CarThemeData>("Car/CarThemeData/Theme" +
                                                     ThemeSavedDataManager.CarThemeNumber);
                    break;
                }
                case CurrentChallengeType.UnblockChallenge:
                {
                    _carThemeData =
                        Resources.Load<CarThemeData>("Car/CarThemeData/Theme" + 1);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public CarType Get_CarType(Vector3 sizeDelta)
        {
            var scaleTotal = Mathf.RoundToInt(sizeDelta.x + sizeDelta.z);

            return scaleTotal switch
            {
                5 => CarType.X3,
                6 => CarType.X4,
                7 => CarType.X5,
                _ => CarType.X3
            };
        }

        public Material GetMaterial(int carId)
        {
            var material = new Material(_carThemeData.material);
            Texture texture;

            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                case CurrentChallengeType.BossChallenge:
                    texture = _carThemeData.textures[carId % _carThemeData.textures.Count];
                    break;
                case CurrentChallengeType.UnblockChallenge:
                    texture = _carThemeData.textures[0];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            material.SetTexture(MainTex, texture);
            return material;
        }
        
        public Material GetUnblockCarUniqueMaterial()
        {
            var material = new Material(_carThemeData.material);

            var texture = _carThemeData.textures[1];
            
            material.SetTexture(MainTex, texture);
            return material;
        }

        public Mesh Get_Mesh(CarType carType)
        {
            var number = carType switch
            {
                CarType.X3 => 0,
                CarType.X4 => 1,
                CarType.X5 => 2,
                _ => 0
            };

            return _carThemeData.mesh[number];
        }
        
    }
}