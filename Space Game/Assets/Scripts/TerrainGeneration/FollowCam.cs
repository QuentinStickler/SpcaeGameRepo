using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class FollowCam : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 distance = new Vector3(0f,2f,-10f);
    [SerializeField] float distanceDamp = 10f;

    private Transform myT;

    private void Awake()
    {
        myT = transform;
    }

    private void LateUpdate()
    {
        Vector3 toPos = target.position + (target.rotation * distance);
        myT.position = Vector3.Lerp(myT.position, toPos, distanceDamp * Time.deltaTime);

        Quaternion toRot = Quaternion.LookRotation(target.position - myT.position, target.up);
        myT.rotation = Quaternion.Slerp(myT.rotation, toRot,distanceDamp * Time.deltaTime);
    }
}
