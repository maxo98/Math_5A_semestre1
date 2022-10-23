using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Delaunay : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Transform meshTransform;

    public bool voronoi;

    public Material voronoiMat;

    public GameObject point;

    public Transform pointToAdd;

    // Start is called before the first frame update
    public void Start()
    {
        DelaunayTriangulation();

        if(pointToAdd) AddPointToDelaunay(meshTransform.InverseTransformPoint(pointToAdd.position));

        if(voronoi == true) VoronoiFromDelaunay();
    }

    // Update is called once per frame
    public void Update()
    {
        /*Mesh mesh = meshFilter.mesh;

        p1.position = meshTransform.TransformPoint(mesh.vertices[mesh.triangles[0+3*i]]);
        p2.position = meshTransform.TransformPoint(mesh.vertices[mesh.triangles[1+3*i]]);
        p3.position = meshTransform.TransformPoint(mesh.vertices[mesh.triangles[2+3*i]]);

        cube.position = meshTransform.TransformPoint(GetCircleCenter(mesh.vertices[mesh.triangles[0+3*i]], 
        mesh.vertices[mesh.triangles[1+3*i]], 
        mesh.vertices[mesh.triangles[2+3*i]]));*/
        //Debug.Log(cube.position);
    }

    public void DelaunayTriangulation()
    {
        int[] triangles = meshFilter.mesh.triangles;
        Vector3[] vertices = meshFilter.mesh.vertices;

        //for each triangles
        for(int i = 0; i < triangles.Length; i+=3)
        {
            
            Vector3 circleCenter = GetCircleCenter(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]]);
            float sqrRadius = (circleCenter - vertices[triangles[i]]).sqrMagnitude;

            int cpt = 0;
            bool found = false;

            //check delaunay condition for each vertex
            while(cpt < vertices.Length && found == false)
            {
                if(sqrRadius > (circleCenter - vertices[cpt]).sqrMagnitude)
                {
                    //Check that it's not a point of the current triangle
                    if(vertices[triangles[i]] != vertices[cpt] && vertices[triangles[i+1]] != vertices[cpt] && vertices[triangles[i+2]] != vertices[cpt])
                    {
                        //Debug.Log("Found 1");
                        found = true;
                    }else{
                        cpt++;
                    }
                }else{
                    cpt++;
                }
            }

            //If we need to flip it
            if(found == true)
            {
                bool reset = false;

                //Search to which triangle the vertex belongs
                for(int i2 = i; i2 < triangles.Length && reset == false; i2++)
                {
                    if(triangles[i2] == cpt)
                    {
                        //Search the common edge
                        int triangleIndex = i2 - (i2 % 3);

                        int p1 = -1;
                        int p2 = -1;
                        int pA3 = -1;

                        for(int i3 = 0; i3 < 3; i3++)
                        {
                            bool found2 = false;

                            for(int i4 = 0; i4 < 3 && found2 == false; i4++)
                            {
                                //Debug.Log(i + " " + triangleIndex);
                                //Debug.Log(i + " " + triangleIndex + " " + triangles[i + i3] + " " + triangles[triangleIndex + i4]);

                                if(triangles[i + i3] == triangles[triangleIndex + i4])
                                {
                                    if(p1 == -1)
                                    {
                                        p1 = triangles[i + i3];
                                        found2 = true;
                                    }else{
                                        p2 = triangles[i + i3];
                                        found2 = true;
                                    }
                                }
                            }
            
                            //Get isolated point of triangle A
                            if(found2 == false)
                            {
                                pA3 = triangles[i + i3];
                            }
                        }

                        //If the triangles do have a common egde
                        if(p2 != -1)
                        {
                            //Debug.Log("Found 2");

                            //Search for isolated point of triangle B
                            int pB3 = -1;
                            bool found2 = false;

                            for(int i3 = 0; i3 < 3 && found2 == false; i3++)
                            {
                                //Debug.Log(p1 + " " + p2 + " " + triangles[triangleIndex + i3] + " " + triangleIndex);
                                if(triangles[triangleIndex + i3] != p1 && triangles[triangleIndex + i3] != p2)
                                {
                                    pB3 = triangles[triangleIndex + i3];
                                    found2 = true;
                                }
                            }

                            //Debug.Log("result " + p1 + " " + p2 + " " + pA3 + " " + pB3);

                            //Do the backflip !
                            triangles[i] = pA3;
                            triangles[i+1] = pB3;
                            triangles[i+2] = p1;

                            triangles[triangleIndex] = p2;
                            triangles[triangleIndex+1] = pA3;
                            triangles[triangleIndex+2] = pB3;

                            //Start the search again
                            reset = true;
                            i = -3;
                        }
                    }
                }
            }
        }

        DoubleFaceIndices(ref triangles);

        meshFilter.mesh.triangles = triangles;
    }

    public void AddPointToDelaunay(Vector3 newPoint)
    {
        //Remove double side
        List<int> triangles = new List<int>(meshFilter.mesh.triangles);
        triangles.RemoveRange(meshFilter.mesh.triangles.Length/2, meshFilter.mesh.triangles.Length/2);
        List<Vector3> vertices = new List<Vector3>(meshFilter.mesh.vertices);

        Queue<(int, int)> segments = new Queue<(int, int)>();

        bool insideTriangle = false;

        //Check if the point is inside a triangle
        for(int i = 0; i < triangles.Count && insideTriangle == false; i+=3)
        {
            List<Vector3> triangle = new List<Vector3>();

            triangle.Add(vertices[triangles[i]]);
            triangle.Add(vertices[triangles[i+1]]);
            triangle.Add(vertices[triangles[i+2]]);

            if(InsidePolygon(newPoint, triangle) == true)
            {
                insideTriangle = true;
                segments.Enqueue((triangles[i], triangles[i+1]));
                segments.Enqueue((triangles[i+1], triangles[i+2]));
                segments.Enqueue((triangles[i+2], triangles[i]));
                triangles.RemoveRange(i, 3);
            }
        }

        if(insideTriangle != true)
        {
            //List visible vertices
            HashSet<int> visibleVertices = new HashSet<int>();

            for(int cpt = 0; cpt < vertices.Count; cpt++)
            {
                bool intersection = false;

                for(int i = 0; i < triangles.Count && insideTriangle == false; i+=3)
                {
                    Vector3 dummy;

                    if(SegmentIntersection(vertices[cpt], newPoint, vertices[triangles[i]], vertices[triangles[i+1]], out dummy))
                    {
                        intersection = true;
                        break;
                    }

                    if(SegmentIntersection(vertices[cpt], newPoint, vertices[triangles[i+2]], vertices[triangles[i+1]], out dummy))
                    {
                        intersection = true;
                        break;
                    }

                    if(SegmentIntersection(vertices[cpt], newPoint, vertices[triangles[i]], vertices[triangles[i+2]], out dummy))
                    {
                        intersection = true;
                        break;
                    }
                }

                if(intersection == false)
                {
                    visibleVertices.Add(cpt);
                }
            }

            //List visible segments
            for(int i = 0; i < triangles.Count && insideTriangle == false; i+=3)
            {
                if(visibleVertices.Contains(triangles[i]) && visibleVertices.Contains(triangles[i+1]))
                {
                    segments.Enqueue((triangles[i], triangles[i+1]));
                }

                if(visibleVertices.Contains(triangles[i+2]) && visibleVertices.Contains(triangles[i+1]))
                {
                    segments.Enqueue((triangles[i+1], triangles[i+2]));
                }

                if(visibleVertices.Contains(triangles[i]) && visibleVertices.Contains(triangles[i+2]))
                {
                    segments.Enqueue((triangles[i], triangles[i+2]));
                }
            }
        }

        while(segments.Count != 0)
        {
            (int, int) segment = segments.Dequeue();

            int i = FindEdge(segment, triangles);

            if(i == -1)
            {
                //Add new triangle
                triangles.Add(segment.Item1);
                triangles.Add(segment.Item2);
                triangles.Add(vertices.Count);
            }else{
                Vector3 circleCenter = GetCircleCenter(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]]);
                float sqrRadius = (circleCenter - vertices[triangles[i]]).sqrMagnitude;

                //If new point inside circle of triangle
                if(sqrRadius > (circleCenter - newPoint).sqrMagnitude)
                {
                    segments.Enqueue((triangles[i], triangles[i+1]));
                    segments.Enqueue((triangles[i+1], triangles[i+2]));
                    segments.Enqueue((triangles[i+2], triangles[i]));
                    triangles.RemoveRange(i, 3);
                
                }else{
                    //Add new triangle
                    triangles.Add(segment.Item1);
                    triangles.Add(segment.Item2);
                    triangles.Add(vertices.Count);
                }
            }
        }

        vertices.Add(newPoint);

        int[] triangleArray = triangles.ToArray();

        DoubleFaceIndices(ref triangleArray);

        meshFilter.mesh.SetVertices(vertices.ToArray());
        meshFilter.mesh.triangles = triangleArray;
    }

    public void VoronoiFromDelaunay()
    {
        //Destroy children
        foreach (Transform child in meshTransform.transform) {
            GameObject.Destroy(child.gameObject);
        }

        //Remove double side
        List<int> temp = new List<int>(meshFilter.mesh.triangles);
        temp.RemoveRange(meshFilter.mesh.triangles.Length/2, meshFilter.mesh.triangles.Length/2);
        int[] triangles = temp.ToArray();
        Vector3[] vertices = meshFilter.mesh.vertices;

        List<Vector3> lineVertices = new List<Vector3>();
        List<int> lineIndices = new List<int>();

        HashSet<(int, int)> outsideSeg = new HashSet<(int, int)>();
        
        for(int i = 0; i < triangles.Length; i+=3)
        {
            //Add the center of the circle to the vertices
            Vector3 center = GetCircleCenter(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]]);
            center.z = vertices[triangles[i]].z;
            
            int centerIndex = lineVertices.Count;
            lineVertices.Add(center);

            List<Vector3> triangle = new List<Vector3>();
            triangle.Add(vertices[triangles[i]]);
            triangle.Add(vertices[triangles[i+1]]);
            triangle.Add(vertices[triangles[i+2]]);

            AddVoronoiLine(ref lineVertices, ref lineIndices, vertices[triangles[i]], vertices[triangles[i+1]], centerIndex);
            //If center of the circle is on the wrong side of the triangle segment flip the voronoi segment
            VoronoiFlip(center, triangle, 0, 1, ref lineIndices, ref lineVertices);
            outsideSeg.Add((centerIndex, lineIndices[lineIndices.Count-1]));

            AddVoronoiLine(ref lineVertices, ref lineIndices, vertices[triangles[i+1]], vertices[triangles[i+2]], centerIndex);
            VoronoiFlip(center, triangle, 1, 2, ref lineIndices, ref lineVertices);
            outsideSeg.Add((centerIndex, lineIndices[lineIndices.Count-1]));

            AddVoronoiLine(ref lineVertices, ref lineIndices, vertices[triangles[i]], vertices[triangles[i+2]], centerIndex);
            VoronoiFlip(center, triangle, 2, 0, ref lineIndices, ref lineVertices);
            outsideSeg.Add((centerIndex, lineIndices[lineIndices.Count-1]));
        }

        //List outside seg by remove inside seg
        for(int index = 0; index < lineIndices.Count; index+=2)
        {
            int count = 0;

            for(int i = 0; i < lineIndices.Count; i+=2)
            {
                if(lineIndices[i+1] == lineIndices[index+1])
                {
                    count++;
                }
            }

            if(count > 1)
            {
                outsideSeg.Remove((lineIndices[index], lineIndices[index+1]));
            }
        }

        //Handle special cases by shortening or removing, if not special elongate the outside edge
        foreach ((int, int) seg in outsideSeg) 
        {
            bool found = false;

            for(int i = 0; i < lineIndices.Count && found == false; i+=2)
            {

                if((lineIndices[i], lineIndices[i+1]) == seg)
                {
                    int newEnd = -1;
                    bool hasCollided = false;
                    Vector3 collision = Vector3.zero;
                    float sqrDist = 0;

                    for(int cpt = 0; cpt < lineIndices.Count; cpt+=2)
                    {
                        if(outsideSeg.Contains((lineIndices[cpt], lineIndices[cpt+1])) == false && cpt != i)
                        {
                            if(lineIndices[cpt] != lineIndices[i] && OnSegment(lineVertices[lineIndices[i]], lineVertices[lineIndices[cpt]], lineVertices[lineIndices[i+1]]) == true)
                            {
                                if(newEnd == -1 || (lineVertices[newEnd] - lineVertices[lineIndices[i]]).sqrMagnitude >  (lineVertices[lineIndices[cpt]] - lineVertices[lineIndices[i+1]]).sqrMagnitude)
                                {
                                    newEnd = lineIndices[cpt];
                                }
                            }

                            Vector3 newCollision;
                            bool collide = SegmentIntersection(lineVertices[lineIndices[cpt]], lineVertices[lineIndices[cpt+1]], lineVertices[lineIndices[i]], lineVertices[lineIndices[i+1]], out newCollision);

                            if(collide == true && (hasCollided == false || sqrDist >  (newCollision - lineVertices[lineIndices[i]]).sqrMagnitude))
                            {
                                sqrDist = (newCollision - lineVertices[lineIndices[i]]).sqrMagnitude;
                                collision = newCollision;
                                hasCollided = true;
                            }
                        }
                    }

                    if(newEnd != -1 && ((sqrDist > (lineVertices[newEnd] - lineVertices[lineIndices[i]]).sqrMagnitude) || (hasCollided == false)))
                    {
                        lineIndices[i+1] = newEnd;

                    }else if(hasCollided == true)
                    {
                        //segToSegTri.Remove((lineIndices[i2-1], lineIndices[i2]));
                        lineIndices.RemoveRange(i, 2);
                    }else{//Elongate outside edge

                        lineVertices[lineIndices[i+1]] += (lineVertices[lineIndices[i+1]] - lineVertices[lineIndices[i]]) * 100;
                    }

                    found = true;
                }
            }
        }

        //Creates cubes children to represent cell cores
        int offset = lineVertices.Count;

        lineVertices.InsertRange(lineVertices.Count, vertices);

        List<int> cellCoreIndices = new List<int>();

        for(int i = 0; i < vertices.Length; i++)
        {
            Instantiate(point, meshTransform.TransformPoint(vertices[i]), new Quaternion(), meshTransform);

            cellCoreIndices.Add(offset + i);
        }

        //Attribute the vertices and indices to the mesh
        meshFilter.mesh.Clear();
        meshFilter.mesh.subMeshCount = 2;
        meshFilter.mesh.SetVertices(lineVertices);
        meshFilter.mesh.SetIndices(lineIndices, MeshTopology.Lines, 0);
        meshFilter.mesh.SetIndices(cellCoreIndices, MeshTopology.Points, 1);

        meshRenderer.material = voronoiMat;
    }

    public void AddVoronoiLine(ref List<Vector3> lineVertices, ref List<int> lineIndices, in Vector3 vertA, in Vector3 vertB, int centerIndex)
    {
        Vector3 lineMiddle = (vertA + vertB);
        lineMiddle.x /= 2;
        lineMiddle.y /= 2;
        lineMiddle.z = vertA.z;

        int lineIndex = lineVertices.FindIndex(0, lineVertices.Count-1, d => d == lineMiddle);

        if(lineIndex == -1)
        {
            lineIndex = lineVertices.Count;
            lineVertices.Add(lineMiddle);
        }

        lineIndices.Add(centerIndex);
        lineIndices.Add(lineIndex);
    }

    //Reutns the index of first triangle continaing the edge or -1
    public int FindEdge((int, int) edge, in List<int> triangles)
    {
        int i = 0;

        while(i < triangles.Count)
        {
            if((triangles[i], triangles[i+1]) == edge || (triangles[i+1], triangles[i]) == edge)
            {
                return i;

            }else if((triangles[i+1], triangles[i+2]) == edge || (triangles[i+2], triangles[i+1]) == edge)
            {
                return i;

            }else if((triangles[i], triangles[i+2]) == edge || (triangles[i+2], triangles[i]) == edge)
            {
                return i;
            }
                
            i+=3;
        }

        return -1;
    }

    //If center of the circle is on the wrong side of the triangle segment flip the voronoi segment
    public bool VoronoiFlip(in Vector3 center, in List<Vector3> triangle, int a, int b, ref List<int> lineIndices, ref List<Vector3> lineVertices)
    {
        //Debug.Log(IsInsideTriangle(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]], center));
        Vector3 lineMiddle = (center + lineVertices[lineIndices[lineIndices.Count-1]]);
        lineMiddle.x /= 2;
        lineMiddle.y /= 2;
        lineMiddle.z = center.z;

        //if(IsInsideTriangle(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]], lineMiddle) == false)
        bool clockwise = isClockwise(triangle);
        int orientation = Orientation(triangle[a], triangle[b], center);

        if((clockwise == true && orientation == 1) || (clockwise == false && orientation == 2))
        {
            lineVertices.Add((center - lineVertices[lineVertices.Count-1]) * 500 + center);
            lineIndices[lineIndices.Count-1] = lineVertices.Count - 1;

            return true;
        }

        return false;
    }

    public bool isClockwise(in List<Vector3> polygon)
    {
        float sum = 0;

        for(int i = 0; i < polygon.Count; i++)
        {
            Vector3 nextVec = (i+1) < polygon.Count ? polygon[i+1] : polygon[0];

            sum += (nextVec.x - polygon[i].x) * (nextVec.y + polygon[i].y);
        }

        //Clockwise if sum > 0
        //CounterClockwise if sum < 0
        return (sum > 0);
    }

    // 0 --> a, b and c are colinear
    // 1 --> c is on the right
    // 2 --> c is on the left
    public byte Orientation(Vector3 a, Vector3 b, Vector3 c)
    {
        double ABx = b.x - a.x;
        double ABy = b.y - a.y;

        double ACx = c.x - a.x;
        double ACy = c.y - a.y;

        double val = ACx * ABy - ACy * ABx;

        if (val == 0) return 0; // collinéire 
        return (byte)((val < 0) ? 1 : 2); // sens horaire ou sens anti-hoaraire
    }

    public Vector3 GetCircleCenter(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 pos = Vector3.zero;

        float x12 = p1.x - p2.x;
        float x13 = p1.x - p3.x;
    
        float y12 = p1.y - p2.y;
        float y13 = p1.y - p3.y;
    
        float y31 = p3.y - p1.y;
        float y21 = p2.y - p1.y;
    
        float x31 = p3.x - p1.x;
        float x21 = p2.x - p1.x;
    
        float sx13 = p1.x*p1.x - p3.x*p3.x;
    
        float sy13 = p1.y*p1.y - p3.y*p3.y;
    
        float sx21 = p2.x*p2.x - p1.x*p1.x;
        float sy21 = p2.y*p2.y - p1.y*p1.y;
    
        pos.y = -((sx13) * (x12) + (sy13) * (x12) + (sx21) * (x13) + (sy21) * (x13)) / (2 * ((y31) * (x12) - (y21) * (x13)));
        pos.x = -((sx13) * (y12) + (sy13) * (y12) + (sx21) * (y13) + (sy21) * (y13)) / (2 * ((x31) * (y12) - (x21) * (y13)));

        return pos;
    }

    public void DoubleFaceIndices(ref int[] indices)
    {
        int n = 3;

        if((indices.Length % n) != 0)
        {
            return;
        }

        int indicesLength = indices.Length;

        Array.Resize(ref indices, indices.Length*2);

        for(int i = 0; i < indicesLength; i += n)
        {
            for(int cpt = 0; cpt < n; cpt++)
            {
                indices[indicesLength+i+cpt] = indices[i+n-1-cpt];
            }
        }
    }

    public bool InsidePolygon(Vector3 point, List<Vector3> polygon)
    {
        byte orient = Orientation(polygon[polygon.Count-1], polygon[0], point);

        for(int i = 0; i < polygon.Count-1; i++)
        {
            if(orient != Orientation(polygon[i], polygon[i+1], point))
            {
                return false;
            }
        }

        return true;
    }

    
    public bool SegmentIntersection(in Vector3 A, in Vector3 B, in Vector3 I, in Vector3 P, out Vector3 result)
    {
        Vector3 AB = new Vector3((A.x - B.x), (A.y - B.y), 0);
        Vector3 IP = new Vector3((I.x - P.x), (I.y - P.y), 0);

        float det = AB.x * IP.y - AB.y * IP.x;

        result = Vector3.zero;

        if(det == 0)
        {
            //Parallel or colinear
            return false;
        }else {

            if (CollisionSegSeg(A, B, I, P)) {
                float t1 = ((A.x * B.y - A.y * B.x) * (I.x - P.x) - (A.x - B.x) * (I.x * P.y - I.y * P.x)) / det;
                float t2 = ((A.x * B.y - A.y * B.x) * (I.y - P.y) - (A.y - B.y) * (I.x * P.y - I.y * P.x)) / det;

                result = new Vector3(t1, t2, A.z);

                return true;
            }

            return false;
        }
    }

    public bool CollisionDroiteSeg(in Vector3 A, in Vector3 B, in Vector3 O, in Vector3 P)
    {
        Vector3 AO = Vector3.zero, AP = Vector3.zero, AB = Vector3.zero;
        AB.x = B.x - A.x;
        AB.y = B.y - A.y;
        AP.x = P.x - A.x;
        AP.y = P.y - A.y;
        AO.x = O.x - A.x;
        AO.y = O.y - A.y;
        if ((AB.x * AP.y - AB.y * AP.x) * (AB.x * AO.y - AB.y * AO.x) < 0)
            return true;
        else
            return false;
    }

    public bool CollisionSegSeg(in Vector3 A, in Vector3 B, in Vector3 O, in Vector3 P)
    {
        if (CollisionDroiteSeg(A, B, O, P) == false)
            return false;  // inutile d'aller plus loin si le segment [OP] ne touche pas la droite (AB)
        if (CollisionDroiteSeg(O, P, A, B) == false)
            return false;
        return true;
    }

    //Check if Q is segment PR
    bool OnSegment(Vector3 p, Vector3 q, Vector3 r)
    {
        if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) && q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
            return true;
        return false;
    }
}
