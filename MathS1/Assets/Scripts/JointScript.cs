using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointScript : MonoBehaviour
{
    [SerializeField] private GameObject jointGameObject;
    [SerializeField] private Material jointMat;
    [SerializeField] private BoneScript bone1;
    [SerializeField] private BoneScript bone2;
    [SerializeField] private float tolerance = 1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CreateJoint();
        }
    }

    private void CreateJoint()
    {
        var possibleJoints = new List<Tuple<Vector3, Vector3>>
        {
            new(bone1.maximumExtreme, bone2.maximumExtreme),
            new(bone1.minimumExtreme, bone2.minimumExtreme),
            new(bone1.maximumExtreme, bone2.minimumExtreme),
            new(bone1.minimumExtreme, bone2.maximumExtreme)
        };
        var isSphere = false;
        var smallestMagnitudeIndex = 0;
        var smallestMagnitude = (possibleJoints[smallestMagnitudeIndex].Item1 - possibleJoints[0].Item2).magnitude;
        for (var i = 0; i < possibleJoints.Count; i++)
        {
            if ((possibleJoints[i].Item1 - possibleJoints[i].Item2).magnitude < tolerance)
            {
                isSphere = true;
            }

            if (!((possibleJoints[i].Item1 - possibleJoints[i].Item2).magnitude < smallestMagnitude)) continue;
            smallestMagnitude = (possibleJoints[i].Item1 - possibleJoints[i].Item2).magnitude;
            smallestMagnitudeIndex = i;
        }
        if (isSphere)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = (possibleJoints[0].Item1 - possibleJoints[0].Item2) / 2;
        }
        else
        {
            var instantiatedjoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            instantiatedjoint .transform.position = (possibleJoints[smallestMagnitudeIndex].Item1 - possibleJoints[smallestMagnitudeIndex].Item2) / 2;
            var scaleY = (possibleJoints[smallestMagnitudeIndex].Item1 - possibleJoints[smallestMagnitudeIndex].Item2).magnitude / 2;
            instantiatedjoint .transform.localScale = new Vector3(1, scaleY, 1);
            instantiatedjoint .transform.LookAt(possibleJoints[smallestMagnitudeIndex].Item1 - possibleJoints[smallestMagnitudeIndex].Item2);
            instantiatedjoint .transform.forward = instantiatedjoint .transform.up;
            instantiatedjoint .GetComponent<MeshRenderer>().material = jointMat;
        }
        
    }
}
