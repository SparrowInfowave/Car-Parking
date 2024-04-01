using AddressableManager;
using ThemeSelection;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay
{
    public class RoadController : SingletonComponent<RoadController>
    {
        private float roadWidth = 30;
        private Material _roadMaterial;
        private readonly string _objectPath = "Road/";
        private float _offsetInY = -4f;
        
        private void Start()
        {
            _roadMaterial = Resources.Load<Material>(_objectPath + "RoadMaterial");
            _roadMaterial.SetTexture("_BaseMap",GetTexture());
        }

        public Material GetMaterial()
        {
            return _roadMaterial;
        }

        private Texture GetTexture()
        {
            return Resources.Load<Texture>(_objectPath + "RoadTexture/road-texture" +
                                           ThemeSavedDataManager.RoadThemeNumber);
        }
        

        private GameObject Get_Object(string objectName)
        {
            return Resources.Load<GameObject>(_objectPath + objectName);
        }

        public void LeftOrRightRoadAdd(Bounds planeBound, float xPos)
        {
            var height = Mathf.CeilToInt((planeBound.max.z - planeBound.min.z) / 10f);
            for (int i = 0; i < height; i++)
            {
                RoadSpawner.Instance.SpawnRoad(new RoadSpawner.RoadDataForSet
                {
                    Position =  new Vector3(xPos, _offsetInY, planeBound.max.z - (i*10)),
                    Rotation =  Vector3.zero
                },0);
            }
        }

        public void UpOrDownRoadAdd(Bounds planeBound, float zPos)
        {
            var width = Mathf.CeilToInt((planeBound.max.x - planeBound.min.x) / 10f);
            for (int i = 0; i < width; i++)
            {
                RoadSpawner.Instance.SpawnRoad(new RoadSpawner.RoadDataForSet
                {
                    Position =  new Vector3(planeBound.min.x + (i*10), _offsetInY, zPos - 15),
                    Rotation =  new Vector3(0, -90, 0) 
                },0);
            }
        }

        public void Out_Road(Bounds planeBound, float xPos)
        {
            for (int i = 0; i < 30; i++)
            {
                RoadSpawner.Instance.SpawnRoad(new RoadSpawner.RoadDataForSet
                {
                    Position = new Vector3(xPos, _offsetInY, (planeBound.min.z - roadWidth) - (i*10)),
                    Rotation =  Vector3.zero
                },0);
                
            }
        }

        public void Add_Road_Corner(Bounds planebound)
        {
            
            //TopLeft
            RoadSpawner.Instance.SpawnRoad(new RoadSpawner.RoadDataForSet
            {
                Position = new Vector3(planebound.min.x, _offsetInY, planebound.max.z),
                Rotation =  new Vector3(0,180,0)
            },1);

            //TopRight
            RoadSpawner.Instance.SpawnRoad(new RoadSpawner.RoadDataForSet
            {
                Position = new Vector3(planebound.max.x, _offsetInY, planebound.max.z),
                Rotation =  new Vector3(0, -90, 0)
            },1);

            //BottomRight
            RoadSpawner.Instance.SpawnRoad(new RoadSpawner.RoadDataForSet
            {
                Position = new Vector3(planebound.max.x, _offsetInY, planebound.min.z),
                Rotation =  Vector3.zero
            },1);

            //BottomLeft
            RoadSpawner.Instance.SpawnRoad(new RoadSpawner.RoadDataForSet
            {
                Position = new Vector3(planebound.min.x - roadWidth, _offsetInY, planebound.min.z),
                Rotation =  Vector3.zero
            },2);
        }

        public float Get_RoadWidth()
        {
            return roadWidth;
        }
    }
}