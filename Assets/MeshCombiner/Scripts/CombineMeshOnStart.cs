using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineMeshOnStart : MonoBehaviour
{
    private MeshCombiner _meshCombiner;

    private IEnumerator Start()
    {
        _meshCombiner = gameObject.AddComponent<MeshCombiner>();
        
        yield return new WaitForSeconds(8f);

        _meshCombiner.CreateMultiMaterialMesh = true;
        _meshCombiner.DestroyCombinedChildren = true;
        
        _meshCombiner.CombineMeshes(false);

        _meshCombiner.gameObject.isStatic = true;

    }
}
