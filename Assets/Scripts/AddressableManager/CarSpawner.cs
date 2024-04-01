using System;
using System.Collections.Generic;
using System.Linq;
using GamePlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableManager
{
    public class CarSpawner : SingletonComponent<CarSpawner>
    {
        [SerializeField] private List<AssetReference> carReferences;

        private readonly Dictionary<AssetReference, List<GameObject>> _spawnedObjects = 
            new Dictionary<AssetReference, List<GameObject>>();
    
        /// The Queue holds requests to spawn an instanced that were made while we are already loading the asset
        /// They are spawned once the addressable is loaded, in the order requested
        private readonly Dictionary<AssetReference, Queue<CarDataForSet>> _queuedSpawnRequests = 
            new Dictionary<AssetReference, Queue<CarDataForSet>>();

        private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _asyncOperationHandles = 
            new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

        public void SpawnCar(CarDataForSet carDataForSet)
        {
            var carType = GetCar.Instance.Get_CarType(LevelGenerator.Instance.Get_Vector(carDataForSet.CarDataNew.Scale));
            var index = carType switch
            {
                CarType.X3 => 0,
                CarType.X4 => 1,
                CarType.X5 => 2,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (index >= carReferences.Count)
                return;

            var assetReference = carReferences[index];

            if (assetReference.RuntimeKeyIsValid() == false)
            {
                Debug.Log("Invalid Key " + assetReference.RuntimeKey.ToString());
                return;
            }
            
            SpawnObject(assetReference,carDataForSet);
        }

        private void SpawnObject(AssetReference assetReference,CarDataForSet carDataForSet)
        {
            if (_asyncOperationHandles.ContainsKey(assetReference))
            {
                if (_asyncOperationHandles[assetReference].IsDone)
                    SpawnObjectFromLoadedReference(assetReference, carDataForSet);
                else
                    EnqueueSpawnForAfterInitialization(assetReference, carDataForSet);
            
                return;
            }
            LoadAndSpawn(assetReference,carDataForSet);
        }

        private void LoadAndSpawn(AssetReference assetReference,CarDataForSet carDataForSet)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
            _asyncOperationHandles[assetReference] = op;
            op.Completed += (operation) =>
            {
                SpawnObjectFromLoadedReference(assetReference, carDataForSet);
                if (_queuedSpawnRequests.ContainsKey(assetReference))
                {
                    while (_queuedSpawnRequests[assetReference]?.Any() == true)
                    {
                        var carData = _queuedSpawnRequests[assetReference].Dequeue();
                        SpawnObjectFromLoadedReference(assetReference, carData);
                    }
                }
            };
        }

        private void EnqueueSpawnForAfterInitialization(AssetReference assetReference,CarDataForSet carDataForSet)
        {
            if (!_queuedSpawnRequests.ContainsKey(assetReference))
                _queuedSpawnRequests[assetReference] = new Queue<CarDataForSet>();
            _queuedSpawnRequests[assetReference].Enqueue(carDataForSet);
        }

        private void SpawnObjectFromLoadedReference(AssetReference assetReference,CarDataForSet carDataForSet)
        {
            assetReference.InstantiateAsync(LevelGenerator.Instance.Get_CarParent()).Completed += (asyncOperationHandle) =>
            {
                if (_spawnedObjects.ContainsKey(assetReference) == false)
                {
                    _spawnedObjects[assetReference] = new List<GameObject>();
                }
            
                _spawnedObjects[assetReference].Add(asyncOperationHandle.Result);
                var notify = asyncOperationHandle.Result.AddComponent<NotifyOnDestroy>();
                notify.Destroyed += Remove;
                LevelGenerator.Instance.SetCarData(notify.gameObject,carDataForSet);
                notify.AssetReference = assetReference;
            };
        }
        
        private void Remove(AssetReference assetReference, NotifyOnDestroy obj)
        {
            Addressables.ReleaseInstance(obj.gameObject);
            if(!_spawnedObjects.ContainsKey(assetReference))return;
            _spawnedObjects[assetReference].Remove(obj.gameObject);
            if (_spawnedObjects[assetReference].Count == 0)
            {
                //Debug.Log($"Removed all {assetReference.RuntimeKey.ToString()}");
            
                if (_asyncOperationHandles[assetReference].IsValid())
                    Addressables.Release(_asyncOperationHandles[assetReference]);

                _asyncOperationHandles.Remove(assetReference);
            }
        }

        public bool CheckAllLoaded()
        {
            return _asyncOperationHandles.All(x => x.Value.IsDone);
        }
        
        public class CarDataForSet
        {
            public int index;
            public CarDataNew CarDataNew;
        }
    }
}