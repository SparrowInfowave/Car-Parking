using System.Collections.Generic;
using System.Linq;
using AddressableManager;
using Manager;
using UnityEngine;
using static GamePlay.LevelGenerator;

namespace GamePlay
{
    public class UnblockCarAddWall : SingletonComponent<UnblockCarAddWall>
    {

        private float offSetInY = -4f;
        public void Add_Wall(Component pl1)
        {
            var bounds = pl1.GetComponent<BoxCollider>().bounds;
            var horizontalDistance = Mathf.Abs(bounds.min.x - bounds.max.x);
            var verticleDistance = Mathf.Abs(bounds.min.z - bounds.max.z);

            //BottomWall
            for (int i = 0; i < Mathf.FloorToInt(horizontalDistance / 10)-1; i++)
            {
                var pos = new Vector3(bounds.min.x + FullUnit + (i * FullUnit), offSetInY, bounds.min.z);
                var rot = new Vector3(0, 90, 0);
                EnvironmentObjectSpawner.Instance.SpawnWall(new EnvironmentObjectSpawner.TransformData
                {
                    Position = pos,
                    Rotation = rot
                });
            }

            //TopWall
            for (int i = 0; i < Mathf.FloorToInt(horizontalDistance / 10)-1; i++)
            {
                var pos =
                    new Vector3(bounds.min.x + FullUnit + (i * FullUnit), offSetInY, bounds.max.z);
                var rot = new Vector3(0, 90, 0);
                EnvironmentObjectSpawner.Instance.SpawnWall(new EnvironmentObjectSpawner.TransformData
                {
                    Position = pos,
                    Rotation = rot
                });
            }

            //LeftWall
            for (int i = 0; i < Mathf.FloorToInt(verticleDistance / 10)-1; i++)
            {
                var pos =
                    new Vector3(bounds.min.x, offSetInY, bounds.min.z + FullUnit + (i * FullUnit));
                var rot = Vector3.zero;

                EnvironmentObjectSpawner.Instance.SpawnWall(new EnvironmentObjectSpawner.TransformData
                {
                    Position = pos,
                    Rotation = rot
                });
            }

            //RightWall
            for (int i = 0; i < Mathf.FloorToInt(verticleDistance / 10)-1; i++)
            {
                var pos =
                    new Vector3(bounds.max.x, offSetInY, bounds.min.z + FullUnit + (i * FullUnit));
                var rot = Vector3.zero;
                EnvironmentObjectSpawner.Instance.SpawnWall(new EnvironmentObjectSpawner.TransformData
                {
                    Position = pos,
                    Rotation = rot
                });
            }
            
            AddCorner(pl1);
        }

        private void AddCorner(Component pl1)
        {
            var bounds = pl1.GetComponent<BoxCollider>().bounds;
            
            //topLeft
            EnvironmentObjectSpawner.Instance.SpawnWallCorner(new EnvironmentObjectSpawner.TransformData
            {
                Position = new Vector3(bounds.min.x, offSetInY, bounds.max.z),
                Rotation = new Vector3(0, 180, 0)
            });
            
            //topRight
            EnvironmentObjectSpawner.Instance.SpawnWallCorner(new EnvironmentObjectSpawner.TransformData
            {
                Position = new Vector3(bounds.max.x, offSetInY, bounds.max.z),
                Rotation = new Vector3(0, -90, 0)
            });
            
            //BottomLeft
            EnvironmentObjectSpawner.Instance.SpawnWallCorner(new EnvironmentObjectSpawner.TransformData
            {
                Position = new Vector3(bounds.min.x, offSetInY, bounds.min.z),
                Rotation = new Vector3(0, 90, 0)
            });
            
            //BottomRight
            EnvironmentObjectSpawner.Instance.SpawnWallCorner(new EnvironmentObjectSpawner.TransformData
            {
                Position = new Vector3(bounds.max.x, offSetInY, bounds.min.z),
                Rotation = new Vector3(0, 0, 0)
            });
        }

        public void CreatePath()
        {
            var outCarPos = FindObjectsOfType<VehicleController>().ToList()
                .Find(x => x.carNumber == LevelGenerator.Instance._UnblockCarChallengeData.trueCarIndex).transform
                .position;
            
            var minZ = outCarPos.z - 15;
            var maxZ = outCarPos.z + 15;
            var wallObjects = GameObject.FindGameObjectsWithTag("Wall");
            var inWayWall = wallObjects.Where(x =>
            {
                Vector3 position;
                return (position = x.transform.position).z > minZ && position.z < maxZ && position.x > 0;
            }).ToList();

            foreach (var wallObj in inWayWall)
            {
                Destroy(wallObj);
            }
        }
    }
}