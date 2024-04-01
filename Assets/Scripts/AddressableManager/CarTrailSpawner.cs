using System;
using System.Collections.Generic;
using System.Linq;
using GamePlay;
using ThemeSelection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableManager
{
    public class CarTrailSpawner : SingletonComponent<CarTrailSpawner>
    {
         [SerializeField] private List<AssetReference> carTrailReference;

        private readonly Dictionary<AssetReference, List<GameObject>> _spawnedObjects = 
            new Dictionary<AssetReference, List<GameObject>>();
    
        /// The Queue holds requests to spawn an instanced that were made while we are already loading the asset
        /// They are spawned once the addressable is loaded, in the order requested
        private readonly Dictionary<AssetReference, Queue<CarTrailDataForSet>> _queuedSpawnRequests = 
            new Dictionary<AssetReference, Queue<CarTrailDataForSet>>();

        private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _asyncOperationHandles = 
            new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

        public void SpawnCarTrail(CarTrailDataForSet carTrailDataForSet)
        {
            var assetReference = carTrailReference[ThemeSavedDataManager.TrailThemeNumber - 1];

            if (assetReference.RuntimeKeyIsValid() == false)
            {
                Debug.Log("Invalid Key " + assetReference.RuntimeKey.ToString());
                return;
            }
            
            SpawnObject(assetReference,carTrailDataForSet);
        }

        private void SpawnObject(AssetReference assetReference,CarTrailDataForSet carTrailDataForSet)
        {
            if (_asyncOperationHandles.ContainsKey(assetReference))
            {
                if (_asyncOperationHandles[assetReference].IsDone)
                    SpawnObjectFromLoadedReference(assetReference, carTrailDataForSet);
                else
                    EnqueueSpawnForAfterInitialization(assetReference, carTrailDataForSet);
            
                return;
            }
            LoadAndSpawn(assetReference,carTrailDataForSet);
        }

        private void LoadAndSpawn(AssetReference assetReference,CarTrailDataForSet carTrailDataForSet)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
            _asyncOperationHandles[assetReference] = op;
            op.Completed += (operation) =>
            {
                SpawnObjectFromLoadedReference(assetReference, carTrailDataForSet);
                if (_queuedSpawnRequests.ContainsKey(assetReference))
                {
                    while (_queuedSpawnRequests[assetReference]?.Any() == true)
                    {
                        var carTrailData = _queuedSpawnRequests[assetReference].Dequeue();
                        SpawnObjectFromLoadedReference(assetReference, carTrailData);
                    }
                }
            };
        }

        private void EnqueueSpawnForAfterInitialization(AssetReference assetReference,CarTrailDataForSet carTrailDataForSet)
        {
            if (!_queuedSpawnRequests.ContainsKey(assetReference))
                _queuedSpawnRequests[assetReference] = new Queue<CarTrailDataForSet>();
            _queuedSpawnRequests[assetReference].Enqueue(carTrailDataForSet);
        }

        private void SpawnObjectFromLoadedReference(AssetReference assetReference,CarTrailDataForSet carTrailDataForSet)
        {
            assetReference.InstantiateAsync(carTrailDataForSet.Parent).Completed += (asyncOperationHandle) =>
            {
                if (_spawnedObjects.ContainsKey(assetReference) == false)
                {
                    _spawnedObjects[assetReference] = new List<GameObject>();
                }
            
                _spawnedObjects[assetReference].Add(asyncOperationHandle.Result);

                var obj = asyncOperationHandle.Result;
                obj.transform.position = carTrailDataForSet.TrailTransform.position;
                var notify = asyncOperationHandle.Result.AddComponent<NotifyOnDestroy>();
                notify.Destroyed += Remove;
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
        
        public class CarTrailDataForSet
        {
            public Transform TrailTransform;
            public Transform Parent;
        }
    }
}
