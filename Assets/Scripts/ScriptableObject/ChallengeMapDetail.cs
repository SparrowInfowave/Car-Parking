using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ChallengeMap", fileName = "ChallengeMapData")]
public class ChallengeMapDetail : ScriptableObject
{
    public List<ChallengeMapDataClass> challengeMapData;
    
}

[System.Serializable]
public class ChallengeMapDataClass
{
    public int mapIndex = 1;
    public bool isRightSideBuilding = true;
}