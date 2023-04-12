using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Random = UnityEngine.Random;

public class ClickDuplicate : MonoBehaviour
{
    void OnMouseDown()
    {
        var copy = Instantiate(gameObject);
        copy.transform.position += (new Vector3(Random.value, .5f, Random.value) * 2 - Vector3.one) * 2f;
        copy.transform.rotation = Quaternion.Euler(Random.insideUnitSphere * 360f);
        var m = copy.GetComponent<ProBuilderMesh>();

        bool makeUnique = !Input.GetKey(KeyCode.LeftShift);  
        if(makeUnique)
            m.MakeUnique();
        
        foreach (var face in m.Extrude(new[] { m.faces[3] }, ExtrudeMethod.FaceNormal, .25f))
            m.SetFaceColor(face, makeUnique ? Color.green : Color.red);

        m.ToMesh();
        m.Refresh();
    }
}
