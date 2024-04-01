using System.Collections.Generic;
using System.Linq;
using GamePlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableManager
{
    public class EnvironmentObjectSpawner : SingletonComponent<EnvironmentObjectSpawner>
    {
        [SerializeField] private List<AssetReference> environmentObjects;
        [SerializeField] private AssetReference wallAssetReference;
        [SerializeField] private AssetReference wallCornerAssetReference;

        private readonly Dictionary<AssetReference, List<GameObject>> _spawnedObjects = 
            new Dictionary<AssetReference, List<GameObject>>();
    
        /// The Queue holds requests to spawn an instanced that were made while we are already loading the asset
        /// They are spawned once the addressable is loaded, in the order requested
        private readonly Dictionary<AssetReference, Queue<TransformData>> _queuedSpawnRequests = 
            new Dictionary<AssetReference, Queue<TransformData>>();

        private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _asyncOperationHandles = 
            new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

        public void SpawnedEnvironmentObject(TransformData transformData)
        {
            var index = Random.Range(0,environmentObjects.Count);

            if (index >= environmentObjects.Count)
                return;

            var assetReference = environmentObjects[index];

            if (assetReference.RuntimeKeyIsValid() == false)
            {
                Debug.Log("Invalid Key " + assetReference.RuntimeKey.ToString());
                return;
            }
            
            SpawnObject(assetReference,transformData);
        }

        public void SpawnWall(TransformData transformData)
        {
            SpawnObject(wallAssetReference,transformData);
        }
        
        public void SpawnWallCorner(TransformData transformData)
        {
            SpawnObject(wallCornerAssetReference,transformData);
        }

        private void SpawnObject(AssetReference assetReference,TransformData transformData)
        {
            if (_asyncOperationHandles.ContainsKey(assetReference))
            {
                if (_asyncOperationHandles[assetReference].IsDone)
                    SpawnObjectFromLoadedReference(assetReference, transformData);
                else
                    EnqueueSpawnForAfterInitialization(assetReference, transformData);
            
                return;
            }
            LoadAndSpawn(assetReference,transformData);
        }

        private void LoadAndSpawn(AssetReference assetReference,TransformData transformData)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
            _asyncOperationHandles[assetReference] = op;
            op.Completed += (operation) =>
            {
                SpawnObjectFromLoadedReference(assetReference, transformData);
                if (_queuedSpawnRequests.ContainsKey(assetReference))
                {
                    while (_queuedSpawnRequests[assetReference]?.Any() == true)
                    {
                        var data = _queuedSpawnRequests[assetReference].Dequeue();
                        SpawnObjectFromLoadedReference(assetReference, data);
                    }
                }
            };
        }

        private void EnqueueSpawnForAfterInitialization(AssetReference assetReference,TransformData roadDataForSet)
        {
            if (!_queuedSpawnRequests.ContainsKey(assetReference))
                _queuedSpawnRequests[assetReference] = new Queue<TransformData>();
            _queuedSpawnRequests[assetReference].Enqueue(roadDataForSet);
        }

        private void SpawnObjectFromLoadedReference(AssetReference assetReference,TransformData transformData)
        {
            assetReference.InstantiateAsync().Completed += (asyncOperationHandle) =>
            {
                if (_spawnedObjects.ContainsKey(assetReference) == false)
                {
                    _spawnedObjects[assetReference] = new List<GameObject>();
                }
            
                _spawnedObjects[assetReference].Add(asyncOperationHandle.Result);
                var obj = asyncOperationHandle.Result;
                
                obj.transform.position = transformData.Position;
                obj.transform.localEulerAngles = transformData.Rotation;
                obj.transform.SetParent(transformData.Parent);
                
                var notify = obj.AddComponent<NotifyOnDestroy>();
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
        
        public class TransformData
        {
            public Vector3 Position;
            public Vector3 Rotation;
            public Transform Parent;
        }
    }
}
