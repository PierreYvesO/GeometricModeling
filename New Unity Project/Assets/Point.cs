using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Point : MonoBehaviour
{
    [SerializeField] public string name;
    [SerializeField] public bool distDroite;
    [SerializeField] public Droite droite;
    [SerializeField] public bool distPlan;
    [SerializeField] public Plan plan;


    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 19;
        style.normal.textColor = Color.red;
        Vector3 textPos = transform.position + new Vector3(.3f, .3f, .3f);
        Handles.Label(textPos, name, style);
        Gizmos.DrawSphere(transform.position, .15f);

        if(distDroite)
        {
            Vector3 projection = Vector3.Project((transform.position - droite.pointA.position), (droite.pointB.position - droite.pointA.position)) + droite.pointA.position;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(projection, .1f);
            Gizmos.DrawLine(transform.position, projection);
            style.normal.textColor = Color.green;
            float dist = Vector3.Distance(transform.position, projection);
            textPos = (transform.position + projection) / 2 + new Vector3(.3f, .3f, .3f);
            Handles.Label(textPos, dist.ToString("#.00"), style);
        }

        if (distPlan)
        {
            Vector3 projection = Vector3.ProjectOnPlane((transform.position - droite.pointA.position), plan.getNormale()) + droite.pointA.position;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(projection, .1f);
            Gizmos.DrawLine(transform.position, projection);
            style.normal.textColor = Color.green;
            float dist = Vector3.Distance(transform.position, projection);
            textPos = (transform.position + projection) / 2 + new Vector3(.3f, .3f, .3f);
            Handles.Label(textPos, dist.ToString("#.00"), style);
        }



    }
}

[CustomEditor(typeof(Point))]
public class MyScriptEditor : Editor
{
    override public void OnInspectorGUI()
    {
        var point = (Point)target;
        point.name = EditorGUILayout.TextField("Name", point.name);
        point.distDroite = EditorGUILayout.Toggle("Calculer la distance avec une droite", point.distDroite);
        point.distPlan = EditorGUILayout.Toggle("Calculer la distance avec un plan", point.distPlan);

        if (point.distDroite)
            point.droite = (Droite)EditorGUILayout.ObjectField("Droite", point.droite, typeof(Droite), true);
        if (point.distPlan)
            point.plan = (Plan)EditorGUILayout.ObjectField("Plan", point.plan, typeof(Plan), true);

    }
}
