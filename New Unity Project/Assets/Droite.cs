using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Droite : MonoBehaviour
{
    [SerializeField] public Transform pointA;
    [SerializeField] public Transform pointB;

    private void OnDrawGizmos()
    {
       
        Gizmos.DrawRay(pointA.position, (pointB.position - pointA.position) * 100000f);
        Gizmos.DrawRay(pointB.position, (pointA.position - pointB.position) * 100000f);
    }

    public Vector3 getVecteurDirecteur()
    {
        return pointB.position - pointA.position;
    }
}
