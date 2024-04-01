using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GamePlay
{
    public class LightController : SingletonComponent<LightController>
    {
        [SerializeField] private Transform blockPlane;
        [SerializeField] private GameObject sideLamp;

        private readonly float _distanceToMaintain = 150f;
        public void SetLamp()
        {
            var list = new List<Vector3>();
            var bound = blockPlane.GetComponent<BoxCollider>().bounds;
            var ypos = blockPlane.position.y;
            var offSet = 10;
            
            var topLeft = new Vector3(bound.min.x, ypos, bound.max.z) + new Vector3(1,0,-1)*offSet;
            var topRight = new Vector3(bound.max.x, ypos, bound.max.z)+ new Vector3(-1,0,-1)*offSet;
            var bottomLeft = new Vector3(bound.min.x, ypos, bound.min.z)+ new Vector3(1,0,1)*offSet;
            var bottomRight = new Vector3(bound.max.x, ypos, bound.min.z)+ new Vector3(-1,0,1)*offSet;
            
            list.Add(topLeft);
            list.Add(topRight);
            list.Add(bottomLeft);
            list.Add(bottomRight);
            
            list.AddRange(GiveBetweenPosition(topLeft,bottomLeft));
            list.AddRange(GiveBetweenPosition(topLeft,topRight));
            list.AddRange(GiveBetweenPosition(topRight,bottomRight));
            list.AddRange(GiveBetweenPosition(bottomLeft,bottomRight));
            foreach (var item in list)
            {
                Instantiate(sideLamp, item, quaternion.identity, this.transform);
            }
        }

        private List<Vector3> GiveBetweenPosition(Vector3 pos1, Vector3 pos2)
        {
            var isHorizontal = Mathf.Abs(pos1.z - pos2.z) < 3;
            Vector3 upperLight;

            if (isHorizontal)
                upperLight = pos1.x > pos2.x ? pos1 : pos2;
            else
                upperLight = pos1.z > pos2.z ? pos1 : pos2;

            var totalDistance = isHorizontal ? Mathf.Abs(pos1.x - pos2.x) : Mathf.Abs(pos1.z - pos2.z);
            var list = new List<Vector3>();
            
            var addedLight = 0;

            if (isHorizontal)
                addedLight = Mathf.CeilToInt(Mathf.Abs(pos1.x - pos2.x) /_distanceToMaintain) - 1;
            else
                addedLight = Mathf.CeilToInt(Mathf.Abs(pos1.z - pos2.z) /_distanceToMaintain) - 1;

            if (addedLight == 0) return list;
            
            var distance = totalDistance / (addedLight + 1);
            var dir = isHorizontal ? Vector3.left : Vector3.back;
            
            for (var i = 0; i < addedLight; i++)
            {
                var pos = upperLight + (dir*(distance*(i+1)));
                list.Add(pos);
            }

            return list;
        }
    }
}
