using System;
using System.Collections.Generic;
using System.Linq;
using GamePlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace AddressableManager
{
    public class CharecterSpawner : SingletonComponent<CharecterSpawner>
    {
       [SerializeField] private List<AssetReference> charecterReferences;

        private readonly Dictionary<AssetReference, List<GameObject>> _spawnedObjects = 
            new Dictionary<AssetReference, List<GameObject>>();
    
        /// The Queue holds requests to spawn an instanced that were made while we are already loading the asset
        /// They are spawned once the addressable is loaded, in the order requested
        private readonly Dictionary<AssetReference, Queue<CharecterData>> _queuedSpawnRequests = 
            new Dictionary<AssetReference, Queue<CharecterData>>();

        private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _asyncOperationHandles = 
            new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

        public void SpawnCharecter(int index,CharecterData carDataForSet)
        {
            var assetReference = charecterReferences[index-1];
            
            SpawnObject(assetReference,carDataForSet);
        }

        private void SpawnObject(AssetReference assetReference,CharecterData charecterData)
        {
            if (_asyncOperationHandles.ContainsKey(assetReference))
            {
                if (_asyncOperationHandles[assetReference].IsDone)
                    SpawnObjectFromLoadedReference(assetReference, charecterData);
                else
                    EnqueueSpawnForAfterInitialization(assetReference, charecterData);
            
                return;
            }
            LoadAndSpawn(assetReference,charecterData);
        }

        private void LoadAndSpawn(AssetReference assetReference,CharecterData charecterData)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
            _asyncOperationHandles[assetReference] = op;
            op.Completed += (operation) =>
            {
                SpawnObjectFromLoadedReference(assetReference, charecterData);
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

        private void EnqueueSpawnForAfterInitialization(AssetReference assetReference,CharecterData charecterData)
        {
            if (!_queuedSpawnRequests.ContainsKey(assetReference))
                _queuedSpawnRequests[assetReference] = new Queue<CharecterData>();
            _queuedSpawnRequests[assetReference].Enqueue(charecterData);
        }

        private void SpawnObjectFromLoadedReference(AssetReference assetReference,CharecterData charecterData)
        {
            assetReference.InstantiateAsync(charecterData.parent).Completed += (asyncOperationHandle) =>
            {
                if (_spawnedObjects.ContainsKey(assetReference) == false)
                {
                    _spawnedObjects[assetReference] = new List<GameObject>();
                }

                var obj = asyncOperationHandle.Result;
                _spawnedObjects[assetReference].Add(obj);
                obj.GetComponent<MovingObstacleController>().Set_Path(charecterData.pathData);
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
        
        public class CharecterData
        {
            public List<Float_Vector> pathData = new List<Float_Vector>();
            public Transform parent;
        }
    }
}
