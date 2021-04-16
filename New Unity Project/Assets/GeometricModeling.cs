using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HalfEdge;
namespace HalfEdge
{
    public class HalfEdge
    {
        public int index;
        public Vertex sourceVertex;
        public Face face;
        public HalfEdge prevEdge;
        public HalfEdge nextEdge;
        public HalfEdge twinEdge;

        private HalfEdge()
        {
            this.index = -1; ;
        }

        public HalfEdge(int index, Vertex sourceVertex, Face face)
        {
            this.index = index;
            this.sourceVertex = sourceVertex;
            this.face = face;
            this.twinEdge = new HalfEdge();
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", index, sourceVertex.index,face.index,prevEdge.index,nextEdge.index, twinEdge.index);
        }

        public override bool Equals(object obj)
        {
            return ((HalfEdge)obj).index == this.index;
        }
    }
    public class Vertex
    {
        public int index;
        public Vector3 position;
        public HalfEdge outgoingEdge;

        public Vertex(int index, Vector3 position)
        {
            this.index = index;
            this.position = position;
        }

        public void setOutgoingEdge(HalfEdge outgoingEdge)
        {
            this.outgoingEdge = outgoingEdge;
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}", index, position, outgoingEdge.index);
        }

        public override bool Equals(object obj)
        {
            return ((Vertex)obj).index == this.index;
        }
    }

    public class Face
    {
        public int index;
        public HalfEdge edge;

        public Face(int index)
        {
            this.index = index;
        }

        public void setEdge(HalfEdge edge)
        {
            this.edge = edge;
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}", index, edge.index);
        }

        public override bool Equals(object obj)
        {
            return ((Face)obj).index == this.index;
        }
    }

    public class HalfEdgeMesh
    {
        public List<Vertex> vertices = new List<Vertex>();
        public List<HalfEdge> edges = new List<HalfEdge>();
        public List<Face> faces = new List<Face>();

        public static HalfEdgeMesh ConvertFaceVertexMeshToHalfEdgeMesh(Mesh mesh)
        {
            Dictionary<long, HalfEdge> lft = new Dictionary<long, HalfEdge>();
            HalfEdgeMesh halfEdgeMesh = new HalfEdgeMesh();

            Vector3[] mesh_vertices = mesh.vertices;
            int[] quads = mesh.GetIndices(0);

            for (int i = 0; i < mesh_vertices.Length; i++)
            {
                halfEdgeMesh.vertices.Add(new Vertex(i, mesh_vertices[i]));
            }
            for (int i = 0; i < quads.Length/4; i++)
            {
                int pt1 = quads[i * 4];
                int pt2 = quads[i * 4 + 1];
                int pt3 = quads[i * 4 + 2];
                int pt4 = quads[i * 4 + 3];

                // Create new face
                Face face = new Face(i);
                // Add it to the list
                halfEdgeMesh.faces.Add(face);

                int count = halfEdgeMesh.edges.Count;
                //Create its halfedges
                HalfEdge he1 = new HalfEdge(count, halfEdgeMesh.vertices[pt1], face);
                HalfEdge he2 = new HalfEdge(count + 1, halfEdgeMesh.vertices[pt2], face);
                HalfEdge he3 = new HalfEdge(count + 2, halfEdgeMesh.vertices[pt3], face);
                HalfEdge he4 = new HalfEdge(count + 3, halfEdgeMesh.vertices[pt4], face);

                he1.prevEdge = he4;
                he1.nextEdge = he2;

                he2.prevEdge = he1;
                he2.nextEdge = he3;

                he3.prevEdge = he2;
                he3.nextEdge = he4;

                he4.prevEdge = he3;
                he4.nextEdge = he1;

                // Set twin halfedges
                setTwin(he1,lft);
                setTwin(he2, lft);
                setTwin(he3, lft);
                setTwin(he4, lft);

                //set outgoing edges for vertices
                halfEdgeMesh.vertices[pt1].outgoingEdge = he1;
                halfEdgeMesh.vertices[pt2].outgoingEdge = he2;
                halfEdgeMesh.vertices[pt3].outgoingEdge = he3;
                halfEdgeMesh.vertices[pt4].outgoingEdge = he4;

                // set face edge
                face.edge = he1;

                // Add halfedges
                halfEdgeMesh.edges.Add(he1);
                halfEdgeMesh.edges.Add(he2);
                halfEdgeMesh.edges.Add(he3);
                halfEdgeMesh.edges.Add(he4);
            }
                   
            return halfEdgeMesh;
        }

        private static void setTwin(HalfEdge he, Dictionary<long, HalfEdge> lft)
        {
            int[] indexes = { he.nextEdge.sourceVertex.index, he.sourceVertex.index };
            Array.Sort(indexes);
            long idTwin = (indexes[0] + indexes[1]) * (indexes[0] + indexes[1] + 1) / 2 + indexes[1];
            if (lft.ContainsKey(idTwin))
            {
                he.twinEdge = lft[idTwin];
                lft[idTwin].twinEdge = he;
                lft.Remove(idTwin);
            }
            else
            {
                lft.Add(idTwin, he);
            }
        }

        public static Mesh ConvertHalfEdgeMeshToFaceVertexMesh(HalfEdgeMesh halfEdgeMesh)
        {
            Vector3[] vertices = new Vector3[halfEdgeMesh.vertices.Count];
            for(int i = 0; i < halfEdgeMesh.vertices.Count;i++)
            {
                vertices[i] = halfEdgeMesh.vertices[i].position;
            }
            int[] quads = new int[halfEdgeMesh.edges.Count];
            for (int i = 0; i < halfEdgeMesh.edges.Count; i+=4)
            {
                HalfEdge current_edge = halfEdgeMesh.edges[i];
                quads[i] = current_edge.sourceVertex.index;
                current_edge = current_edge.nextEdge;
                quads[i+1] = current_edge.sourceVertex.index;
                current_edge = current_edge.nextEdge;
                quads[i+2] = current_edge.sourceVertex.index;
                current_edge = current_edge.nextEdge;
                quads[i+3] = current_edge.sourceVertex.index;
            }
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.SetIndices(quads, MeshTopology.Quads, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        public List<HalfEdge> getAdjacentEdges(Vertex vertex)
        {
            List<HalfEdge> adjacentEdges = new List<HalfEdge>();
            HalfEdge currentOutgoingEdge = vertex.outgoingEdge;
            Boolean edgeHasTwin = true;
            while (edgeHasTwin) {
                if (!adjacentEdges.Contains(currentOutgoingEdge) && currentOutgoingEdge.index != -1) {
                    adjacentEdges.Add(currentOutgoingEdge);
                    currentOutgoingEdge = currentOutgoingEdge.prevEdge;
                    adjacentEdges.Add(currentOutgoingEdge);
                    currentOutgoingEdge = currentOutgoingEdge.twinEdge;
                } else {
                    edgeHasTwin = false;
                }
            }
            if (currentOutgoingEdge != vertex.outgoingEdge) {
                currentOutgoingEdge = vertex.outgoingEdge.twinEdge;
                edgeHasTwin = true;
                while (edgeHasTwin) {
                    if (!adjacentEdges.Contains(currentOutgoingEdge) && currentOutgoingEdge.index != -1) {
                        adjacentEdges.Add(currentOutgoingEdge);
                        currentOutgoingEdge = currentOutgoingEdge.nextEdge;
                        adjacentEdges.Add(currentOutgoingEdge);
                        currentOutgoingEdge = currentOutgoingEdge.twinEdge;
                    } else {
                        edgeHasTwin = false;
                    }
                }
            }
            return adjacentEdges;
        }

        public int getValence(Vertex vertex)
        {
            List<HalfEdge> knownEdges = new List<HalfEdge>();
            HalfEdge currentOutgoingEdge = vertex.outgoingEdge;
            int cpt = 1;
            Boolean edgeHasTwin = true;
            while (edgeHasTwin) {
                if (!knownEdges.Contains(currentOutgoingEdge) && currentOutgoingEdge.index != -1) {
                    knownEdges.Add(currentOutgoingEdge);
                    currentOutgoingEdge = currentOutgoingEdge.prevEdge;
                    knownEdges.Add(currentOutgoingEdge);
                    currentOutgoingEdge = currentOutgoingEdge.twinEdge;
                    cpt++;
                } else {
                    edgeHasTwin = false;
                }
            }
            if (currentOutgoingEdge != vertex.outgoingEdge) {
                currentOutgoingEdge = vertex.outgoingEdge.twinEdge;
                edgeHasTwin = true;
                while (edgeHasTwin) {
                    if (!knownEdges.Contains(currentOutgoingEdge) && currentOutgoingEdge.index != -1) {
                        knownEdges.Add(currentOutgoingEdge);
                        currentOutgoingEdge = currentOutgoingEdge.nextEdge;
                        knownEdges.Add(currentOutgoingEdge);
                        cpt++;
                        currentOutgoingEdge = currentOutgoingEdge.twinEdge;
                    } else {
                        edgeHasTwin = false;
                    }
                }
            } else {
                cpt--;
            }
            return cpt;
        }

        public List<Face> getFacesWithSameVertex(Vertex vertex)
        {
            List<Face> faces = new List<Face>();
            foreach(HalfEdge edge in getAdjacentEdges(vertex))
            {
                if(!faces.Contains(edge.face))
                {
                    faces.Add(edge.face);
                }
            }
            return faces;
        }

        public override string ToString()
        {
            string str = "Vertices\t\t\t\tFaces\t\t\tHalfEdges\n";
            str += "Index\tPosition\tOutgoing Edges\t\tIndex\tStarting Edge\t\tIndex\tSource Vertex Index\tFace Index\tPrevious Edge Index\tNext Edge Index\tTwin Edge Index\n";
            for(int i = 0; i < edges.Count;i++)
            {
                if (i < vertices.Count) str += vertices[i] + "\t\t";
                else str += "\t\t\t\t";
                if (i < faces.Count) str += faces[i] + "\t\t";
                else str += "\t\t\t";
                str += edges[i] + "\n";
            }
            return str;
        }

    }
}

public class GeometricModeling : MonoBehaviour
{
    [SerializeField] MeshFilter mf;
    [SerializeField] bool drawGizmosFaces;
    [SerializeField] bool drawGizmosEdges;
    [SerializeField] bool drawGizmosVertices;
    [SerializeField] bool drawGizmosHalfEdges;

    Transform m_transform;
    
    private void Awake()
    {
        m_transform = transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        //mf.sharedMesh = CreateCube(Vector3.one);
        mf.sharedMesh = CreatePlaneXZMadeOfQuads(new Vector2(2, 1), 4, 2);
        //mf.sharedMesh = CreateRegularQuadPolygon(Vector2.one, 20);

        Debug.Log(exportMeshToExcel(mf.sharedMesh));
        HalfEdge.HalfEdgeMesh halfEdgeMesh = HalfEdge.HalfEdgeMesh.ConvertFaceVertexMeshToHalfEdgeMesh(mf.sharedMesh);
        Debug.Log(halfEdgeMesh);
        Mesh mesh = HalfEdge.HalfEdgeMesh.ConvertHalfEdgeMeshToFaceVertexMesh(halfEdgeMesh);
        Debug.Log(exportMeshToExcel(mesh));
        List<HalfEdge.HalfEdge> adjacentEdgesOfVertex = halfEdgeMesh.getAdjacentEdges(halfEdgeMesh.vertices[2]);
        Debug.Log(string.Join("\n", adjacentEdgesOfVertex));
        Debug.Log(string.Join("\n", halfEdgeMesh.getFacesWithSameVertex(halfEdgeMesh.vertices[2])));
        int valenceOfVertex = halfEdgeMesh.getValence(halfEdgeMesh.vertices[9]);
        Debug.Log(valenceOfVertex);
    }

    string exportMeshToExcel(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] quads = mesh.GetIndices(0);

        int nbLignes = Mathf.Max(vertices.Length, quads.Length / 4);

        List<string> strings = new List<string>();
        for(int i = 0; i < nbLignes; i++)
            strings.Add("");

        for (int i = 0; i < strings.Count; i++)
        {
            if (i < vertices.Length) strings[i] += i.ToString() + "\t" + vertices[i].ToString() + "\t\t";
            else strings[i] = "\t\t\t\t";

            if (i < quads.Length / 4) strings[i] += i.ToString() + "\t" + quads[4 * i].ToString()
                      + "\t" + quads[4 * i + 1].ToString()
                      + "\t" + quads[4 * i + 2].ToString()
                      + "\t" + quads[4 * i + 3].ToString();
        }

        return "Vertices\t\t\tFaces\n" + "index\tposition\t\tindex\tvertices\n" + string.Join("\n", strings);
    }

    private void DrawGizmosVertices(Mesh mesh)
    {
        Gizmos.color = Color.red;
        GUIStyle style = new GUIStyle();
        style.fontSize = 19;
        style.normal.textColor = Color.red;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 globalPos = m_transform.TransformPoint(vertices[i]);
            Gizmos.DrawSphere(globalPos, 0.01f);

            string str = i.ToString();
            Handles.Label(globalPos, str, style);
        }
    }

    private void DrawGizmosHalfEdge(HalfEdge.HalfEdgeMesh heMesh)
    {
        
        Gizmos.color = Color.green;
        GUIStyle style = new GUIStyle();
        style.fontSize = 19;
        style.normal.textColor = Color.green;
        List<HalfEdge.HalfEdge> edges  = heMesh.edges;
        for (int i = 0; i < edges.Count; i++)
        {
            Vector3 from = m_transform.TransformPoint(edges[i].sourceVertex.position);
            Vector3 to = m_transform.TransformPoint(edges[i].nextEdge.sourceVertex.position);
            Vector3 prev = m_transform.TransformPoint(edges[i].prevEdge.sourceVertex.position);

            string str = i.ToString();
            if (prev.x < from.x) from.x = (prev.x * 0.2f + from.x) / 1.2f;
            if (prev.z > from.z) from.z = (prev.z * 0.2f + from.z) / 1.2f;

            Handles.Label((from+to)/2, str, style);
        }
    }

    private void DrawGizmosFaces(Mesh mesh)
    {
        Gizmos.color = Color.blue;
        GUIStyle style = new GUIStyle();
        style.fontSize = 19;
        style.normal.textColor = Color.blue;
        style.alignment = TextAnchor.MiddleCenter;

        Vector3[] vertices = mesh.vertices;
        int[] quads = mesh.GetIndices(0);

        if (quads.Length % 4 != 0) return;
        for (int i = 0; i < quads.Length / 4; i++)
        {
            int index1 = quads[4 * i];
            int index2 = quads[4 * i + 1];
            int index3 = quads[4 * i + 2];
            int index4 = quads[4 * i + 3];

            Vector3 pt1 = m_transform.TransformPoint(vertices[index1]);
            Vector3 pt2 = m_transform.TransformPoint(vertices[index2]);
            Vector3 pt3 = m_transform.TransformPoint(vertices[index3]);
            Vector3 pt4 = m_transform.TransformPoint(vertices[index4]);

            Gizmos.DrawLine(pt1, pt2);
            Gizmos.DrawLine(pt2, pt3);
            Gizmos.DrawLine(pt3, pt4);
            Gizmos.DrawLine(pt4, pt1);

           

            string str = i.ToString() + "\n(" + index1.ToString() + ", " + index2.ToString() + ", " + index3.ToString() + ", " + index4.ToString() + ")";

            
            Handles.Label(.25f * (pt1 + pt2 + pt3 + pt4), str, style);
        }
    }

    private void DrawGizmosEdges(Mesh mesh)
    {
    }

    private void OnDrawGizmos()
    {
        if (mf && mf.sharedMesh)
        {
            if (drawGizmosFaces) DrawGizmosFaces(mf.sharedMesh);
            if (drawGizmosEdges) DrawGizmosEdges(mf.sharedMesh);
            if (drawGizmosVertices) DrawGizmosVertices(mf.sharedMesh);
            if (drawGizmosHalfEdges) DrawGizmosHalfEdge(HalfEdge.HalfEdgeMesh.ConvertFaceVertexMeshToHalfEdgeMesh(mf.sharedMesh));
        }
    }

    Mesh CreateCube(Vector3 size)
    {
        Mesh mesh = new Mesh();
        mesh.name = "cube";

        Vector3[] vertices = new Vector3[8] {
                              new Vector3(.5f*size.x,.5f*size.y, -.5f*size.z),
                              new Vector3(.5f*size.x,.5f*size.y, .5f*size.z),
                              new Vector3(.5f*size.x,-.5f*size.y, .5f*size.z),
                              new Vector3(.5f*size.x,-.5f*size.y, -.5f*size.z),
                              new Vector3(-.5f*size.x,.5f*size.y, .5f*size.z),
                              new Vector3(-.5f*size.x,.5f*size.y,-.5f*size.z),
                              new Vector3(-.5f*size.x,-.5f*size.y, -.5f*size.z),
                              new Vector3(-.5f*size.x,-.5f*size.y, .5f*size.z)};

        int[] quads = new int[6 * 4] {1 ,2 ,3 ,4,
                                      5 ,6 ,7 ,8,
                                      5 ,2 ,1 ,6,
                                      3 ,8 ,7 ,4,
                                      6 ,1 ,4 ,7,
                                      2 ,5 ,8 ,3}; // 

        for (int i = 0; i < quads.Length; i++) quads[i] -= 1;

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    Mesh CreatePlaneXZMadeOfQuads(Vector2 size, int nSegmentsX, int nSegmentsZ)
    {
        Mesh mesh = new Mesh();
        mesh.name = "planeXZMadeOfQuads";

        Vector3[] vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        int[] quads = new int[nSegmentsZ * nSegmentsX * 4]; // ?? triangles fois 3 vertices
        //Vector3[] normals = new Vector3[vertices.Length];
        //Vector2[] uv = new Vector2[vertices.Length];

        Vector2 halfSize = .5f * size;

        // vertices, normals & uv
        for (int i = 0; i < nSegmentsZ + 1; i++)
        {
            float kZ = (float)i / nSegmentsZ;
            int offset = i * (nSegmentsX + 1);

            for (int j = 0; j < nSegmentsX + 1; j++)
            {
                float kX = (float)j / nSegmentsX;
                vertices[offset + j] = new Vector3(
                  Mathf.Lerp(-halfSize.x, halfSize.x, kX), 0, Mathf.Lerp(-halfSize.y, halfSize.y, kZ));
                //normals[offset + j] = Vector3.up;
                //uv[offset + j] = new Vector2(kX, kZ);
            }
        }
        ////quads
        int index = 0;
        for (int i = 0; i < nSegmentsZ; i++)
        {
            int offset = i * (nSegmentsX + 1);
            for (int j = 0; j < nSegmentsX; j++)
            {
                quads[index++] = offset + j;  //P0
                quads[index++] = offset + j + nSegmentsX + 1; // P1
                quads[index++] = offset + j + nSegmentsX + 1 + 1; // P2
                quads[index++] = offset + j + 1; // P3
            }
        }
        mesh.vertices = vertices;
        //mesh.normals = normals;
        //mesh.uv = uv;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    Mesh CreateRegularQuadPolygon(Vector2 size, int nSectors)
    {
        Mesh mesh = new Mesh();
        mesh.name = "regularQuadPolygon" + nSectors;
        Vector3[] vertices = new Vector3[nSectors * 2 + 1];
        int[] quads = new int[nSectors * 4]; // ?? triangles fois 3 vertices
        // Vector3[] normals = new Vector3[vertices.Length];
        //Vector2[] uv = new Vector2[vertices.Length];

       Vector2 halfSize = .5f * size;

        float deltaAngle = Mathf.PI * 2 / nSectors;

        // vertices, normals & uv

        for (int i = 0; i < nSectors; i++)
        {
            float kZ = (float)i / nSectors;
            float angle = kZ * Mathf.PI * 2;
            vertices[2 * i + 0] = new Vector3(halfSize.x * Mathf.Cos(angle), 0, halfSize.y * Mathf.Sin(angle));
            vertices[2 * i + 1] = .5f * (new Vector3(halfSize.x * Mathf.Cos(angle + deltaAngle), 0, halfSize.y * Mathf.Sin(angle + deltaAngle)) + vertices[2 * i + 0]);

            // normals[2 * i + 0] = Vector3.up;
            //normals[ 2 * i + 1] = Vector3.up;

            //uv[2 * i + 0] = Vector2.zero;
            //uv[2 * i + 1] = Vector2.zero;
        }
        vertices[vertices.Length - 1] = Vector3.zero;
        //normals[vertices.Length - 1] = Vector3.up;
        //uv[vertices.Length - 1] = Vector2.zero;

        ////quads
        int index = 0;
        for (int i = 0; i < nSectors; i++)
        {
            quads[index++] = vertices.Length - 1;
            quads[index++] = (2 * i + 1) % (vertices.Length - 1);
            quads[index++] = (2 * i) % (vertices.Length - 1);
            quads[index++] = (2 * i - 1 + vertices.Length - 1) % (vertices.Length - 1);
        }

        mesh.vertices = vertices;
        // mesh.normals = normals;
        //mesh.uv = uv;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);
        mesh.RecalculateBounds();
        return mesh;
    }
}
