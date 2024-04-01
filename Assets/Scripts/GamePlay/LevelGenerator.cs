using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AddressableManager;
using Manager;
using Newtonsoft.Json;
using ThemeSelection;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public enum VehicleOrientation
    {
        Horizontal,
        Verticle,
    }

    public enum RotateType
    {
        Default,
        Reverse
    }

    [RequireComponent(typeof(GetCar), typeof(GamePlayManager), typeof(UnblockCarAddWall))]
    public class LevelGenerator : SingletonComponent<LevelGenerator>
    {
        [SerializeField] private CameraFit cameraFit;
        
        [SerializeField] private Transform plane,
            parkingSign,
            blockPlane,
            fenceplane,
            leftPart,
            rightPart,
            upperPart,
            barricade;

        [Space(20)] [SerializeField] private Transform lowerPart;
        [SerializeField] private Transform lowerPartLeft;
        [SerializeField] private Transform lowerPartRight;
        [SerializeField] GameObject collisionParticle;
        [SerializeField] private List<Vector3> _pathPoints = new List<Vector3>();
        [HideInInspector] public List<VehicleController> vehicleControllers = new List<VehicleController>();
        [SerializeField] private Transform obstacleObjectParent, carObjectParent, movingObstacleParent, roadParent;

        [HideInInspector] public LevelData _levelData;
        [HideInInspector] public UnblockCarChallengeData _UnblockCarChallengeData;

        public const float HalfUnit = 5f;
        public const float FullUnit = 10f;
        public const float OneAndHalfUnit = 15f;

        public RotateType rotateType = RotateType.Reverse;

        [HideInInspector] public bool isTutorial = false;
        
        [HideInInspector] public bool isHintTutorial = false;

        private float _cameraHorizontalFov = 15;

        private float offSetInY = -4f;

        private void Start()
        {
            StartCoroutine(Generate_Level());
        }

        private void Update()
        {
            if (_levelData != null && _levelData.CarData.Count == 0)
            {
                var obj = vehicleControllers.Find(x => x.carNumber == _levelData.CarId[0]);
                obj.GetComponent<MeshRenderer>().material = null;
            }
        }

        private IEnumerator Generate_Level()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();

            if (CarSpawner.Instance == null)
            {
                var addSpawner = Addressables.LoadAssetAsync<GameObject>(new AssetLabelReference { labelString = "Spawner" });

                addSpawner.Completed += addToGenerator => { Instantiate(addToGenerator.Result, Vector3.zero, quaternion.identity); };

                yield return new WaitUntil(() => addSpawner.IsDone);
            }

            GetCar.Instance.Set_Car_Data();
            Clear_level();

            yield return new WaitForSeconds(0.1f);

            _levelData = GetLevelData();
            GameManager.Instance.levelOrChallengeCompleteReward = CalculateReward();

            #region Set Plane

            plane.localScale = Get_Vector(_levelData.PlaneScale);

            var pos = plane.position;
            pos.y = offSetInY;
            plane.position = pos;
            
            var addedScale = Vector3.zero;
            if (GameManager.Instance.challengeType == CurrentChallengeType.UnblockChallenge)
                addedScale = Vector3.one;

            plane.localScale += addedScale;

            blockPlane.localScale = Get_Vector(_levelData.PlaneScale) + Vector3.one * 10;
            var fenceScale = blockPlane.localScale.z + 10;
            fenceplane.localScale = new Vector3(fenceScale + 10, 1, fenceScale);

            #endregion

            //Set default camera at level generate
            Set_Camera(plane.transform.localScale);
            cameraFit.horizontalFOV += LoadingPanelController.Instance.zoomSize;

            yield return new WaitForSeconds(0.1f);

            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                case CurrentChallengeType.BossChallenge:
                    Add_Obstacle(plane, _levelData);
                    break;
                case CurrentChallengeType.UnblockChallenge:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            yield return new WaitUntil(EnvironmentObjectSpawner.Instance.CheckAllLoaded);

            #region Add Road

            var planeSize = plane.transform.localScale;
            _pathPoints.Clear();

            var planeBound = plane.GetComponent<BoxCollider>().bounds;
            var max = planeBound.max;
            var min = planeBound.min;


            //Down Road
            for (int i = 0; i < (int)planeSize.x; i++)
            {
                _pathPoints.Add(new Vector3(min.x + HalfUnit + (i * FullUnit), min.y, (min.z - OneAndHalfUnit)));
            }

            #region BottomRightCorner

            var cornerPointBottomRight = new Vector3(max.x, max.y, min.z);
            _pathPoints.Add(new Vector3(cornerPointBottomRight.x + OneAndHalfUnit, cornerPointBottomRight.y,
                cornerPointBottomRight.z - OneAndHalfUnit));

            #endregion

            //Right Road
            for (int i = (int)planeSize.z; i > -1; i--)
            {
                _pathPoints.Add(new Vector3(max.x + OneAndHalfUnit, max.y, (max.z + HalfUnit) - (i * FullUnit)));
            }

            #region TopRightCorner

            _pathPoints.Add(new Vector3(max.x + OneAndHalfUnit, max.y, max.z + OneAndHalfUnit));

            #endregion

            //Up Road
            //reverse loop for left to right instantiate 
            for (int i = 0; i < (int)planeSize.x; i++)
            {
                _pathPoints.Add(new Vector3(max.x - HalfUnit - (i * FullUnit), max.y, (max.z + OneAndHalfUnit)));
            }

            #region TopLeftCorner

            var cornerPointTopLeft = new Vector3(min.x, max.y, max.z);
            _pathPoints.Add(new Vector3(cornerPointTopLeft.x - OneAndHalfUnit, cornerPointTopLeft.y,
                cornerPointTopLeft.z + OneAndHalfUnit));

            #endregion

            //Left Road
            for (int i = 0; i < (int)planeSize.z + 40; i++)
            {
                _pathPoints.Add(new Vector3(min.x - OneAndHalfUnit, min.y, (max.z + HalfUnit) - (i * FullUnit)));
            }

            #region BottomLeftCorner

            //pathPoints.Add(new Vector3(min.x - oneAndHalfUnit, min.y, min.z - oneAndHalfUnit));

            #endregion

            yield return new WaitForEndOfFrame();

            #region Add Road prefab

            var roadWidth = RoadController.Instance.Get_RoadWidth();
            //RightRoad
            RoadController.Instance.LeftOrRightRoadAdd(planeBound, planeBound.max.x + (roadWidth / 2f));
            yield return new WaitUntil(() => RoadSpawner.Instance.CheckAllLoaded());

            //LeftRoad
            RoadController.Instance.LeftOrRightRoadAdd(planeBound, planeBound.min.x - (roadWidth / 2f));
            yield return new WaitUntil(() => RoadSpawner.Instance.CheckAllLoaded());

            //Uproad
            RoadController.Instance.UpOrDownRoadAdd(planeBound, planeBound.max.z + (roadWidth / 2f) * 2f);
            yield return new WaitUntil(() => RoadSpawner.Instance.CheckAllLoaded());

            //DownRoad
            RoadController.Instance.UpOrDownRoadAdd(planeBound, planeBound.min.z);
            yield return new WaitUntil(() => RoadSpawner.Instance.CheckAllLoaded());

            //out road
            RoadController.Instance.Out_Road(planeBound, planeBound.min.x - (roadWidth / 2f));
            yield return new WaitUntil(() => RoadSpawner.Instance.CheckAllLoaded());

            RoadController.Instance.Add_Road_Corner(planeBound);
            yield return new WaitUntil(() => RoadSpawner.Instance.CheckAllLoaded());

            #endregion

            #endregion

            #region LightAdd

            if (LightController.Instance != null)
            {
                LightController.Instance.SetLamp();
                yield return new WaitForEndOfFrame();
            }

            #endregion

            yield return new WaitForSeconds(0.1f);

            #region Add Moving Obstacle

            if (_levelData.MovingObstaclePath.Count > 1)
            {
                Add_MovingObstacle(Set_According_Single_Path(new List<Float_Vector>(_levelData.MovingObstaclePath)));
            }

            var path = new List<Float_Vector>();
            var addedObstacle = 0;
            foreach (var pathItem in _levelData.MovingObstaclePaths)
            {
                path.Add(pathItem);
                if (path.Count == _levelData.MovingObstaclePathsCount[addedObstacle])
                {
                    Add_MovingObstacle(new List<Float_Vector>(path));
                    addedObstacle++;
                    path.Clear();
                }
            }

            #endregion

            yield return new WaitForSeconds(0.1f);

            #region Set Outer Environment

            parkingSign.transform.position = planeBound.max;

            leftPart.position = new Vector3(planeBound.min.x - 90, leftPart.position.y, planeBound.min.z + 290);

            rightPart.position = new Vector3(planeBound.max.x + 90, rightPart.position.y, planeBound.max.z - 30);

            lowerPart.position = new Vector3(lowerPart.position.x, lowerPart.position.y, planeBound.min.z - 150);

            upperPart.position = new Vector3(upperPart.position.x, upperPart.position.y, planeBound.max.z + 150);

            //Set Barricade.
            barricade.position =
                new Vector3(planeBound.min.x - (roadWidth + 2f), -6f, planeBound.min.z - roadWidth - 3f);

            Lower_Part_Set_Up(planeBound.min.x - OneAndHalfUnit);

            #endregion

            yield return new WaitForSeconds(0.1f);

            #region Add Car

            var _carCount = 0;

            foreach (var carDataNew in _levelData.CarData)
            {
                CarSpawner.Instance.SpawnCar(
                    new CarSpawner.CarDataForSet { CarDataNew = carDataNew, index = _carCount });
                _carCount++;
            }

            yield return new WaitUntil(() => CarSpawner.Instance.CheckAllLoaded());
            
            var vehicleController = FindObjectsOfType<VehicleController>();

            foreach (var item in vehicleController)
            {
                item.SetStartPos();
            }

            #endregion

            #region Add Wall for UnblockCar

            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                case CurrentChallengeType.BossChallenge:
                    break;
                case CurrentChallengeType.UnblockChallenge:
                    UnblockCarAddWall.Instance.Add_Wall(plane);
                    yield return new WaitUntil(() => EnvironmentObjectSpawner.Instance.CheckAllLoaded());
                    CheckForUnlockChallengeTutorial();
                    UnblockCarAddWall.Instance.CreatePath();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            #endregion

            yield return new WaitForEndOfFrame();

            if (rotateType == RotateType.Reverse)
            {
                Reverse_level();
                yield return new WaitForSeconds(0.1f);
            }

            MakeStaticObjects(upperPart.GetComponentsInChildren<MeshRenderer>().Select(x => x.gameObject)
                .Where(x => !IsMoveAble(x)));
            MakeStaticObjects(lowerPart.GetComponentsInChildren<MeshRenderer>().Select(x => x.gameObject)
                .Where(x => !IsMoveAble(x)));
            MakeStaticObjects(leftPart.GetComponentsInChildren<MeshRenderer>().Select(x => x.gameObject)
                .Where(x => !IsMoveAble(x)));
            MakeStaticObjects(rightPart.GetComponentsInChildren<MeshRenderer>().Select(x => x.gameObject)
                .Where(x => !IsMoveAble(x)));
            MakeStaticObjects(roadParent.GetComponentsInChildren<MeshRenderer>().Select(x => x.gameObject));
            MakeStaticObjects(obstacleObjectParent.GetComponentsInChildren<MeshRenderer>().Select(x => x.gameObject)
                .Where(x => !IsMoveAble(x)));

            var time = _levelData.ChallengeTimeMultiplier;

            GameManager.Instance.targetMoves = time + Mathf.FloorToInt(time / 10f);
            GameManager.Instance.targetTime = Mathf.CeilToInt(time * 2.6f);

            if (GamePlayController.Instance != null)
                GamePlayController.Instance.Set_Move_Time_Data();

            yield return new WaitForSeconds(0.2f);
            GameManager.Instance.Disable_Loading_Panel();
        }

        private bool IsMoveAble(GameObject obj)
        {
            return obj.CompareTag("Moveable");
        }

        public void SetCarData(GameObject carObj, CarSpawner.CarDataForSet carDataForSet)
        {
            carObj.GetComponent<MeshRenderer>().material = GetCar.Instance.GetMaterial(carDataForSet.index);
            carObj.GetComponent<MeshFilter>().mesh =
                GetCar.Instance.Get_Mesh(GetCar.Instance.Get_CarType(Get_Vector(carDataForSet.CarDataNew.Scale)));

            if (GameManager.Instance.challengeType == CurrentChallengeType.UnblockChallenge)
            {
                if (carDataForSet.index == _UnblockCarChallengeData.trueCarIndex)
                    carObj.GetComponent<MeshRenderer>().material = GetCar.Instance.GetUnblockCarUniqueMaterial();
            }


            carObj.name = carDataForSet.index.ToString();
            var pos = Get_Vector(carDataForSet.CarDataNew.Position);
            pos.y = offSetInY;
            carObj.transform.position = pos;

            var vehicleController = carObj.GetComponent<VehicleController>();

            //Set orientation
            if (carDataForSet.CarDataNew.Scale.x > carDataForSet.CarDataNew.Scale.z)
            {
                carObj.transform.localEulerAngles = new Vector3(0, 90, 0);
                vehicleController.Set_Orientation(VehicleOrientation.Horizontal);
            }
            else
            {
                vehicleController.Set_Orientation(VehicleOrientation.Verticle);
            }

            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                case CurrentChallengeType.BossChallenge:
                    vehicleController.carNumber = _levelData.CarId[carDataForSet.index];
                    break;
                case CurrentChallengeType.UnblockChallenge:
                    vehicleController.carNumber = carDataForSet.index;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Lower_Part_Set_Up(float leftRoadX)
        {
            var roadWidth = RoadController.Instance.Get_RoadWidth();
            lowerPartLeft.position = new Vector3((leftRoadX - roadWidth / 2f), lowerPartLeft.position.y,
                lowerPartLeft.position.z);
            lowerPartRight.position = new Vector3(leftRoadX + roadWidth / 2f, lowerPartRight.position.y,
                lowerPartRight.position.z);
        }

        public void SetCameraAtEnd()
        {
            Set_Camera(plane.transform.localScale);
        }

        private void Set_Camera(Vector3 planeScale)
        {
            const int min = 12;
            var planeWidth = planeScale.x;
            if (planeWidth < min)
                planeWidth = min;

            var planeHeight = planeScale.z;
            if (planeHeight < min)
                planeHeight = min;


            var width = (Mathf.Cos(
                             Mathf.Abs(90 - Mathf.Abs(cameraFit.transform.localEulerAngles.y)) * (Mathf.PI / 180f)) *
                         planeHeight) *
                        2;
            var widthToAdd = planeWidth + ((width * 6) / 20f);

            var cameraFitTransform = cameraFit.transform;
            var pos = cameraFitTransform.position;
            pos.x = (0.6875f * planeWidth) + -104.43f;
            cameraFitTransform.position = pos;

            cameraFit._adjustMode = CameraFit.Mode.Dynamic;
            cameraFit.mainAreaWidth = widthToAdd;
            cameraFit.mainAreaHeight = planeHeight;
            cameraFit.horizontalFOV = widthToAdd;
            _cameraHorizontalFov = widthToAdd;
        }

        public float GetCameraSize()
        {
            //Set_Camera(plane.transform.localScale);
            return cameraFit.horizontalFOV;
        }

        public float GetDefaultCameraPosForLevel()
        {
            return _cameraHorizontalFov;
        }

        private void Add_Obstacle(Component pl1, LevelData levelData)
        {
            var planeBoxCollider = pl1.GetComponent<BoxCollider>();
            var obstacles = Resources.LoadAll<GameObject>("Prefab/InnerObstacle").ToList();

            var wall = obstacles.Find(x => x.name.Contains("wall"));
            obstacles.Remove(wall);

            foreach (var item in levelData.WallData)
            {
                var obsPos = Get_Vector(item);

                if (Is_Near_Boundary_Of_Plane(planeBoxCollider.bounds, obsPos))
                {
                    var pos = Get_Vector(item);
                    pos.y = offSetInY;
                    var rotationOfWall = Vector3.zero;
                    if (Is_Near_Upper_Or_Lower_Line(planeBoxCollider.bounds.min.z, planeBoxCollider.bounds.max.z,
                            pos.z))
                        rotationOfWall = new Vector3(0, 90, 0);

                    EnvironmentObjectSpawner.Instance.SpawnWall(new EnvironmentObjectSpawner.TransformData
                    {
                        Position = pos,
                        Rotation = rotationOfWall,
                        Parent = obstacleObjectParent
                    });
                }
                else
                {
                    var pos = Get_Vector(item);
                    pos.y = offSetInY;
                    EnvironmentObjectSpawner.Instance.SpawnedEnvironmentObject(
                        new EnvironmentObjectSpawner.TransformData
                        {
                            Position = pos,
                            Rotation = Vector3.zero,
                            Parent = obstacleObjectParent
                        });
                }
            }
        }

        private void Add_MovingObstacle(List<Float_Vector> path)
        {
            CharecterSpawner.Instance.SpawnCharecter(ThemeSavedDataManager.CharecterThemeNumber,new CharecterSpawner.CharecterData
            {
                parent = movingObstacleParent,
                pathData = path
            });
        }

        private List<Float_Vector> Set_According_Single_Path(IEnumerable<Float_Vector> pathListVector)
        {
            var pathList = pathListVector.Select(Get_Vector).ToList();

            var checkCount = 1;
            CheckPoint:

            var newPathList = new List<Vector3> { pathList[0] };

            while (newPathList.Count < pathList.Count)
            {
                var nearest = pathList[0];
                var distance = 1000f;
                foreach (var item in pathList)
                {
                    if (item == newPathList.Last() || newPathList.Contains(item))
                        continue;

                    if (Vector3.Distance(newPathList.Last(), item) < distance)
                    {
                        distance = Vector3.Distance(newPathList.Last(), item);
                        nearest = item;
                    }
                }

                if (distance > 15 && checkCount < 20)
                {
                    pathList.Reverse();
                    checkCount++;
                    goto CheckPoint;
                }

                newPathList.Add(nearest);
            }

            return newPathList.Select(Get_FloatVector).ToList();
        }

        public Vector3 Get_Vector(Float_Vector floatVector)
        {
            return new Vector3(floatVector.x, floatVector.y, floatVector.z);
        }

        private Float_Vector Get_FloatVector(Vector3 vector3)
        {
            return new Float_Vector { x = vector3.x, y = vector3.y, z = vector3.z };
        }

        public List<Vector3> Get_PathPoints()
        {
            return _pathPoints;
        }

        public int Get_Nearest_PathPoint_Index(Vector3 position)
        {
            var pathNumber = 0;
            float distance = 1000;

            foreach (var item in _pathPoints
                         .Where(item => Vector3.Distance(position, item) <= distance))
            {
                distance = Vector3.Distance(position, item);
                pathNumber = _pathPoints.IndexOf(item);
            }

            return pathNumber;
        }

        private string DataPath()
        {
            return "Data/" + GameManager.Instance.challengeType.ToString() + "Data/" +
                   GameManager.Instance.challengeType.ToString();
        }

        private const float CheckDistance = 11;

        private bool Is_Near_Upper_Or_Lower_Line(float minz, float maxz, float wallPos)
        {
            return Mathf.Abs(minz - wallPos) <= CheckDistance || Mathf.Abs(maxz - wallPos) <= CheckDistance;
        }

        private bool Is_Near_Boundary_Of_Plane(Bounds bounds, Vector3 posOfObstacle)
        {
            return (Mathf.Abs(bounds.min.z - posOfObstacle.z) <= CheckDistance ||
                    Mathf.Abs(bounds.max.z - posOfObstacle.z) <= CheckDistance ||
                    Mathf.Abs(bounds.min.x - posOfObstacle.x) <= CheckDistance ||
                    Mathf.Abs(bounds.max.x - posOfObstacle.x) <= CheckDistance);
        }

        public void Make_Collision_Effect(Vector3 position)
        {
            Vector3 rotation;

            switch (TouchDetectController.Inst.currentSwipeDirection)
            {
                case SwipeDirection.Left:
                case SwipeDirection.Right:
                    rotation = new Vector3(0, 0, 0);
                    break;
                case SwipeDirection.Up:
                case SwipeDirection.Down:
                    rotation = new Vector3(0, 90, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var obj = Instantiate(collisionParticle, position,
                Quaternion.Euler(rotation), this.transform);
            Destroy(obj, 1f);
        }

        public Vector3 Get_Barricade_Position()
        {
            return barricade.position;
        }

        public Transform Get_RoadParent()
        {
            return roadParent;
        }

        public Transform Get_CarParent()
        {
            return carObjectParent;
        }

        public Bounds Get_PlaneBound()
        {
            return plane.GetComponent<BoxCollider>().bounds;
        }

        private void Clear_level()
        {
            Clear_All_Child(roadParent.gameObject);
            Clear_All_Child(movingObstacleParent.gameObject);
            Clear_All_Child(carObjectParent.gameObject);
            Clear_All_Child(obstacleObjectParent.gameObject);
        }

        private void Clear_All_Child(GameObject obj)
        {
            for (var i = 0; i < obj.transform.childCount; i++)
            {
                Destroy(obj.transform.GetChild(i).gameObject);
            }
        }

        private void Reverse_level()
        {
            Reverse_ObjectParents(carObjectParent);
            Reverse_ObjectParents(obstacleObjectParent);
        }

        private void Reverse_ObjectParents(Transform objTransform)
        {
            for (var i = 0; i < objTransform.childCount; i++)
            {
                var objTra = objTransform.GetChild(i);
                var reversePos = GamePlayManager.Instance.Get_Reverse_Position(objTra.position);
                objTra.position = reversePos;
            }
        }

        private void MakeStaticObjects(IEnumerable<GameObject> gameObjects)
        {
            foreach (var item in gameObjects)
            {
                item.isStatic = true;
            }
        }

        private LevelData GetLevelData()
        {
            LevelData levelData = null;

            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                {
                    var levelNumb = CommonGameData.LevelNumber;
                    var totalLevel = GeneralDataManager.Instance.totalLevel;

                    rotateType = (levelNumb / totalLevel) % 2 == 0
                        ? RotateType.Reverse
                        : RotateType.Default;

                    const int tempNumber = 250;
                    var fileNumber = 0;

                    if (levelNumb < totalLevel)
                        fileNumber = levelNumb;
                    if (levelNumb >= totalLevel)
                        fileNumber = (tempNumber + ((levelNumb % totalLevel) % (totalLevel - tempNumber)));

                    levelData = JsonConvert.DeserializeObject<LevelData>(Resources
                        .Load<TextAsset>(DataPath() + fileNumber).text);

                    break;
                }
                case CurrentChallengeType.BossChallenge:
                {
                    var currentChallenge = GameManager.Instance.currentBossChallengeNumber;
                    var totalChallenge = GeneralDataManager.Instance.totalBossChallenge;

                    rotateType = (currentChallenge / totalChallenge) % 2 == 0
                        ? RotateType.Reverse
                        : RotateType.Default;

                    levelData = JsonConvert.DeserializeObject<LevelData>(Resources
                        .Load<TextAsset>(DataPath() + currentChallenge % totalChallenge).text);
                    break;
                }
                case CurrentChallengeType.UnblockChallenge:
                {
                    var currentChallenge = GameManager.Instance.currentChallengeNumber;
                    var totalChallenge = GeneralDataManager.Instance.totalChallenge;

                    rotateType = RotateType.Default;

                    _UnblockCarChallengeData = JsonConvert.DeserializeObject<UnblockCarChallengeData>(Resources
                        .Load<TextAsset>(DataPath() + currentChallenge % totalChallenge).text);


                    levelData = new LevelData();
                    levelData.PlaneScale = _UnblockCarChallengeData.PlaneScale;
                    levelData.CarData = _UnblockCarChallengeData.CarData;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return levelData;
        }

        private int CalculateReward()
        {
            var reward = 0;
            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                    if (CommonGameData.LevelNumber < 50)
                        reward += _levelData.CarId.Sum(item => Random.Range(6, 9));
                    else
                        reward += _levelData.CarId.Sum(item => Random.Range(8, 12));
                    break;
                case CurrentChallengeType.BossChallenge:
                    reward = GameManager.Instance.levelOrChallengeCompleteReward;
                    break;
                case CurrentChallengeType.UnblockChallenge:
                    reward = _levelData.CarData.Sum(item => Random.Range(7, 11));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return reward;
        }

        public void CheckForLevelTutorial()
        {
            if(GameManager.Instance.challengeType != CurrentChallengeType.Level)return;
            if (CommonGameData.LevelNumber != 0) return;
            
            isTutorial = true;
            gameObject.AddComponent<FirstLevelTutorial>();
        }

        private void CheckForUnlockChallengeTutorial()
        {
            if(GameManager.Instance.challengeType != CurrentChallengeType.UnblockChallenge)return;
            if (ChallengeDataManager.UnblockChallengeNumber != 1 || GeneralDataManager.Instance.isUnlockCarTutorialShowed) return;
            
            UnlockCarGive_Hint();
        }
        
        public void UnlockCarGive_Hint()
        {
            GamePlayManager.Instance.ResetLevelInGamePlay();
            isHintTutorial = true;
            GeneralDataManager.Instance.isUnlockCarTutorialShowed = true;
            gameObject.AddComponent<HintController>();
            GamePlayController.Instance.Check_HintObj();
        }
        public void UnlockCarRemoveHint()
        {
            if (HintController.Instance != null)
            {
                Destroy(HintController.Instance);
            }
        }
    }

    public class LevelData
    {
        public string Name;
        public Float_Vector PlaneScale;
        public List<CarDataNew> CarData = new List<CarDataNew>();
        public int[] CarId;
        public List<Float_Vector> WallData = new List<Float_Vector>();
        public List<Float_Vector> MovingObstaclePath = new List<Float_Vector>();
        public List<Float_Vector> MovingObstaclePaths = new List<Float_Vector>();
        public int[] MovingObstaclePathsCount;
        public int ChallengeTimeMultiplier;
    }

    public class CarDataNew
    {
        public Float_Vector Position;
        public Float_Vector Scale;
    }

    public class Float_Vector
    {
        public float x;
        public float y;
        public float z;
    }

    public class UnblockCarChallengeData
    {
        public Float_Vector PlaneScale;
        public List<CarDataNew> CarData = new List<CarDataNew>();
        public int trueCarIndex;
        public List<HintData> hintDataList;
    }

    public class HintData
    {
        public int index = 0;
        public int direction = 0;
    }
}