using System.Collections.Generic;
using System.Linq;
using GamePlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableManager
{
    public class RoadSpawner : SingletonComponent<RoadSpawner>
    {
        [SerializeField] private List<AssetReference> roadReference;

        private readonly Dictionary<AssetReference, List<GameObject>> _spawnedObjects = 
            new Dictionary<AssetReference, List<GameObject>>();
    
        /// The Queue holds requests to spawn an instanced that were made while we are already loading the asset
        /// They are spawned once the addressable is loaded, in the order requested
        private readonly Dictionary<AssetReference, Queue<RoadDataForSet>> _queuedSpawnRequests = 
            new Dictionary<AssetReference, Queue<RoadDataForSet>>();

        private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _asyncOperationHandles = 
            new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

        public void SpawnRoad(RoadDataForSet roadDataForSet, int roadType)
        {
            var index = roadType;

            if (index >= roadReference.Count)
                return;

            var assetReference = roadReference[index];

            if (assetReference.RuntimeKeyIsValid() == false)
            {
                Debug.Log("Invalid Key " + assetReference.RuntimeKey.ToString());
                return;
            }
            
            SpawnObject(assetReference,roadDataForSet);
        }

        private void SpawnObject(AssetReference assetReference,RoadDataForSet roadDataForSet)
        {
            if (_asyncOperationHandles.ContainsKey(assetReference))
            {
                if (_asyncOperationHandles[assetReference].IsDone)
                    SpawnObjectFromLoadedReference(assetReference, roadDataForSet);
                else
                    EnqueueSpawnForAfterInitialization(assetReference, roadDataForSet);
            
                return;
            }
            LoadAndSpawn(assetReference,roadDataForSet);
        }

        private void LoadAndSpawn(AssetReference assetReference,RoadDataForSet roadDataForSet)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
            _asyncOperationHandles[assetReference] = op;
            op.Completed += (operation) =>
            {
                SpawnObjectFromLoadedReference(assetReference, roadDataForSet);
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

        private void EnqueueSpawnForAfterInitialization(AssetReference assetReference,RoadDataForSet roadDataForSet)
        {
            if (!_queuedSpawnRequests.ContainsKey(assetReference))
                _queuedSpawnRequests[assetReference] = new Queue<RoadDataForSet>();
            _queuedSpawnRequests[assetReference].Enqueue(roadDataForSet);
        }

        private void SpawnObjectFromLoadedReference(AssetReference assetReference,RoadDataForSet roadDataForSet)
        {
            assetReference.InstantiateAsync(LevelGenerator.Instance.Get_RoadParent()).Completed += (asyncOperationHandle) =>
            {
                if (_spawnedObjects.ContainsKey(assetReference) == false)
                {
                    _spawnedObjects[assetReference] = new List<GameObject>();
                }
            
                _spawnedObjects[assetReference].Add(asyncOperationHandle.Result);
                var obj = asyncOperationHandle.Result;
                
                obj.transform.position = roadDataForSet.Position;
                obj.transform.localEulerAngles = roadDataForSet.Rotation;
                
                obj.transform.localScale = Vector3.one;
                obj.GetComponent<MeshRenderer>().material = RoadController.Instance.GetMaterial();
                
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
        
        public class RoadDataForSet
        {
            public Vector3 Position;
            public Vector3 Rotation;
        }
    }
}
