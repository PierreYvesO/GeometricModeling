using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plan : MonoBehaviour
{

    [SerializeField] Droite normale = null;

    [SerializeField] Transform pointA = null;
    [SerializeField] Transform pointB = null;
    [SerializeField] Transform pointC = null;

    [SerializeField] int taille = 5;

    [SerializeField] Color color = Color.blue;



    private void Update()
    {
        if (normale)
            transform.position = normale.pointA.position;
        else
            transform.position = (pointA.position + pointB.position + pointC.position) / 3;

    }

    private void OnDrawGizmos()
    {
        Vector3 centre, lookAt;

        if (normale)
        {
            centre = normale.pointA.position;
            lookAt = normale.getVecteurDirecteur();
        }
        else
        {
            centre = (pointA.position + pointB.position + pointC.position) / 3;
            Vector3 vectDirecteurAB = pointA.position - pointB.position;
            Vector3 vectDirecteurAC = pointA.position - pointC.position;
            lookAt = Vector3.Cross(vectDirecteurAB, vectDirecteurAC);
        }

        Gizmos.color = color;
        var matrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(centre, Quaternion.LookRotation(lookAt), Vector3.one);
        for (int i = -taille; i <= taille; i++)
        {
            Gizmos.DrawLine(new Vector3(i, -taille, 0), new Vector3(i, taille, 0));
            Gizmos.DrawLine(new Vector3(-taille, i, 0), new Vector3(taille, i, 0));
        }
        Gizmos.matrix = matrix;
    }

    public Vector3 getNormale()
    {
        if (normale)
            return normale.getVecteurDirecteur();
        else
        {
            Vector3 vectDirecteurAB = pointA.position - pointB.position;
            Vector3 vectDirecteurAC = pointA.position - pointC.position;
            return Vector3.Cross(vectDirecteurAB, vectDirecteurAC);
        }
            
    }
}
