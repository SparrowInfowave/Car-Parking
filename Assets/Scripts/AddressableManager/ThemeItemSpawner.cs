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
    public class ThemeItemSpawner : SingletonComponent<ThemeItemSpawner>
    {
        [SerializeField] private AssetReference carThemeItem;
        [SerializeField] private AssetReference environmentThemeItem;
        [SerializeField] private AssetReference roadThemeItem;
        [SerializeField] private AssetReference trailThemeItem;
        [SerializeField] private AssetReference charecterThemeItem;

        private readonly Dictionary<AssetReference, List<GameObject>> _spawnedObjects =
            new Dictionary<AssetReference, List<GameObject>>();

        /// The Queue holds requests to spawn an instanced that were made while we are already loading the asset
        /// They are spawned once the addressable is loaded, in the order requested
        private readonly Dictionary<AssetReference, Queue<ThemeItemClass>> _queuedSpawnRequests =
            new Dictionary<AssetReference, Queue<ThemeItemClass>>();

        private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _asyncOperationHandles =
            new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

        public void SpawnItem(ThemeItemClass themeItemClass)
        {
            var assetReference = themeItemClass.ShopItemData.itemType switch
            {
                ShopItemData.ItemType.Car => carThemeItem,
                ShopItemData.ItemType.Environment => environmentThemeItem,
                ShopItemData.ItemType.Road => roadThemeItem,
                ShopItemData.ItemType.Trail => trailThemeItem,
                ShopItemData.ItemType.Charecter => charecterThemeItem,
                _ => throw new ArgumentOutOfRangeException(nameof(themeItemClass.ShopItemData.itemType),
                    themeItemClass.ShopItemData.itemType, null)
            };

            SpawnObject(assetReference, themeItemClass);
        }

        private void SpawnObject(AssetReference assetReference, ThemeItemClass themeItemClass)
        {
            if (_asyncOperationHandles.ContainsKey(assetReference))
            {
                if (_asyncOperationHandles[assetReference].IsDone)
                    SpawnObjectFromLoadedReference(assetReference, themeItemClass);
                else
                    EnqueueSpawnForAfterInitialization(assetReference, themeItemClass);

                return;
            }

            LoadAndSpawn(assetReference, themeItemClass);
        }

        private void LoadAndSpawn(AssetReference assetReference, ThemeItemClass themeItemClass)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
            _asyncOperationHandles[assetReference] = op;
            op.Completed += (operation) =>
            {
                SpawnObjectFromLoadedReference(assetReference, themeItemClass);
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

        private void EnqueueSpawnForAfterInitialization(AssetReference assetReference, ThemeItemClass themeItemClass)
        {
            if (!_queuedSpawnRequests.ContainsKey(assetReference))
                _queuedSpawnRequests[assetReference] = new Queue<ThemeItemClass>();
            _queuedSpawnRequests[assetReference].Enqueue(themeItemClass);
        }

        private void SpawnObjectFromLoadedReference(AssetReference assetReference, ThemeItemClass themeItemClass)
        {
            assetReference.InstantiateAsync(themeItemClass.Parent).Completed += (asyncOperationHandle) =>
            {
                if (_spawnedObjects.ContainsKey(assetReference) == false)
                {
                    _spawnedObjects[assetReference] = new List<GameObject>();
                }

                _spawnedObjects[assetReference].Add(asyncOperationHandle.Result);
                var obj = asyncOperationHandle.Result;
                obj.GetComponent<ShopThemeItemUiManager>().shopItemData = themeItemClass.ShopItemData;
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

        public class ThemeItemClass
        {
            public Transform Parent;
            public ShopItemData ShopItemData;
        }
    }
}