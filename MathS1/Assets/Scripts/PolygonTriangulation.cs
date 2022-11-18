using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//Polygon triangulation or Art Gallery Problem


    /// The side of the mesh
    public enum VertexType
    {
        Start = 0,
        End = 1,
        Split = 2,
        Merge = 3,
        Regular = 4
    }
    

    public class PolygonTriangulation
    {
        class SortedVertex
        {
            public Vector2 _vertex;
            public LinkedListNode<Vector2> _node;
            public VertexType _type;

            public SortedVertex(LinkedListNode<Vector2> node, VertexType type)
            {
                _node = node;
                _vertex = node.Value;
                _type = type;
            }
        }

        class Edge
        {
            public bool _downWay;
            public bool _upWay;
            public Vector2 _vecA;
            public Vector2 _vecB;
            public int _pointA;
            public int _pointB;

            public Edge(Vector2 vecA, Vector2 vecB)
            {
                _vecA = vecA;
                _vecB = vecB;
                _downWay = true;
                _upWay = true;
            }

            public Edge(int pointA, int pointB)
            {
                _pointA = pointA;
                _pointB = pointB;
                _downWay = true;
                _upWay = true;
            }
        }

        class SortVector2 : IComparer<Vector2>, IComparer<SortedVertex>
        {
            int IComparer<Vector2>.Compare(Vector2 A, Vector2 B)
            {
                if(A.y == B.y)
                {
                    if(A.x == B.x)
                    {
                        return 0;
                    }else if(A.x < B.x)
                    {
                        return -1; 
                    }else{
                        return 1;
                    }
                }else if(A.y > B.y)
                {
                    return -1; 
                }else{
                    return 1;
                }
            }

            //Descending list form max Y to min Y
            int IComparer<SortedVertex>.Compare(SortedVertex A, SortedVertex B)
            {
                return ((IComparer<Vector2>)this).Compare(A._vertex, B._vertex);
            }
        }

        class SortEdge : IComparer<Edge>
        {
            //Descending list form max Y to min Y
            int IComparer<Edge>.Compare(Edge A, Edge B)
            {
                Vector2 highestA = Vector2.zero;
                Vector2 highestB = Vector2.zero;
                Vector2 lowestA = Vector2.zero;
                Vector2 lowestB = Vector2.zero;

                if(A._vecA.y > A._vecB.y)
                {
                    highestA = A._vecA;
                    lowestA = A._vecB;
                }else{
                    highestA = A._vecB;
                    lowestA = A._vecA;
                }

                if(B._vecA.y > B._vecB.y)
                {
                    highestB = B._vecA;
                    lowestB = B._vecB;
                }else{
                    highestB = B._vecB;
                    lowestB = B._vecA;
                }

                if(highestA.y == highestB.y)
                {
                    bool right = false;

                    //If edges are not directly above each other or are crossing each other
                    if(((highestA.x > lowestA.x) && (highestB.x < lowestB.x)) || ((highestA.x < lowestA.x) && (highestB.x > lowestB.x)))
                    {
                        return 0;

                    }else if((highestA.x < lowestA.x) && (highestB.x < lowestB.x))//If lowest points are on the right
                    {
                        right = true;
                    }else if((highestA.x > lowestA.x) && (highestB.x > lowestB.x))//If lowest points are on the left
                    {
                        right = false;
                    }

                    if(highestA.x == highestB.x)
                    {
                        byte side = Orientation(highestA, lowestB, lowestA);

                        if(side == 0)
                        {
                            return 0;
                        }else if((side == 1 && right == true) || (side == 2 && right == false))
                        {
                            return -1;
                        }else{
                            return 1;
                        }
                    }else if((highestA.x > highestB.x && right == true) || (highestA.x < highestB.x && right == false))
                    {
                        return -1;
                    }else{
                        return 1;
                    }
                    
                }else if(highestA.y > highestB.y)
                {
                    return -1;
                }else{
                    return 1;
                }
            }
        }

        class EdgeEqualityComparer : IEqualityComparer<Edge>
        {
            public bool Equals(Edge A, Edge B)
            {
                if(A._vecA == B._vecA && A._vecB == B._vecB)
                    return true;
                else
                    return false;
            }

            public int GetHashCode(Edge edge)
            {
                if(edge == null)
                {
                    return 0;
                }else{
                    return (edge._vecA.GetHashCode() ^ edge._vecB.GetHashCode());
                }
            }
        }

        //Returns the type of vertex
        //A left point, B point to eveluate, C right point
        //End or Start vertex only happen after a split, a merge, or the start (until we find our first start)
        static public VertexType getVertexType(Vector2 vertexA, Vector2 vertexB, Vector2 vertexC, bool clockwise)
        {
            if((vertexA.y < vertexB.y && vertexC.y < vertexB.y) || (vertexA.y < vertexB.y && vertexC.y == vertexB.y) || (vertexA.y == vertexB.y && vertexC.y < vertexB.y))
            {
                byte side = Orientation(vertexA, vertexB, vertexC);

                if((clockwise == false && side == 1) || (clockwise == true && side == 2))
                {
                    return VertexType.Start;
                }else{
                    return VertexType.Split;
                }
            }else if((vertexA.y > vertexB.y && vertexC.y > vertexB.y) || (vertexA.y == vertexB.y && vertexC.y > vertexB.y) || (vertexA.y > vertexB.y && vertexC.y == vertexB.y))
            {
                byte side = Orientation(vertexA, vertexB, vertexC);

                if((clockwise == false && side == 1) || (clockwise == true && side == 2))
                {
                    return VertexType.End;
                }else{
                    return VertexType.Merge;
                }

            }else{
                return VertexType.Regular;
            }            
        }

        //Seidel algorithm
        static public void DecomposeToMonotone(in LinkedList<Vector2> originalPolygon, out LinkedList<LinkedList<Vector2>> monotonePolygons)
        {
            //A polygon with 4 or less vertices can only be monotone
            if(originalPolygon.Count <= 4)
            {   
                monotonePolygons = new LinkedList<LinkedList<Vector2>>();
                monotonePolygons.AddLast(originalPolygon);

                return;
            }

            //Set type of each vertex and create the priority list
            SortedVertex[] priorityList = new SortedVertex[originalPolygon.Count];

            Vector2 vertexA = Vector2.zero;
            Vector2 vertexB = originalPolygon.Last.Value;
            Vector2 vertexC = originalPolygon.First.Value;

            bool clockwise = PolygonTriangulation.isClockwise(in originalPolygon);

            int i = 0;
            LinkedListNode<Vector2> originalPolygonNode = originalPolygon.First;

            for(originalPolygonNode = originalPolygon.First; originalPolygonNode != null; originalPolygonNode = originalPolygonNode.Next, i++)
            {
                vertexA = vertexB;
                vertexB = vertexC;
                vertexC = (originalPolygonNode.Next == null ? originalPolygon.First : originalPolygonNode.Next).Value;

                VertexType type = getVertexType(vertexA, vertexB, vertexC, clockwise);

                priorityList[i] = new SortedVertex(originalPolygonNode, type);
            }

            Array.Sort(priorityList, 0, priorityList.Length, new SortVector2());
            //priorityList.Sort(PolygonTriangulation.SortY);

            //Main part of the decomposition, creates the new edges
            Queue<SortedVertex> priorityQueue = new Queue<SortedVertex>(priorityList);

            //The value is the helper vertex of that edge
            Dictionary<Edge, SortedVertex> hashTable = new Dictionary<Edge, SortedVertex>(new EdgeEqualityComparer());

            LinkedList<Edge> edges = new LinkedList<Edge>();

            while(priorityQueue.Count > 0)
            {
                SortedVertex currentVertex = priorityQueue.Dequeue();
                Edge key;
                SortedVertex helper;
                Vector2 firstVec;
                Vector2 secondVec;
                
                //TryGetValue this function will garanty that this part won't bug 
                //even if we don't get a valid polygon in input of the fuction
                switch(currentVertex._type)
                {
                    case VertexType.Start:
                        if(clockwise == true)
                        {
                            firstVec = (currentVertex._node.Previous == null ? originalPolygon.Last : currentVertex._node.Previous).Value;
                            secondVec = currentVertex._vertex;
                        }else{
                            firstVec = currentVertex._vertex;
                            secondVec = (currentVertex._node.Next == null ? originalPolygon.First : currentVertex._node.Next).Value;
                        }
                        hashTable.Add(new Edge(firstVec, secondVec), currentVertex);
                        break;

                    case VertexType.End:
                        //Ceate an the edge if the edge which ends with the current vertex has an helper which is a merge vertex  
                        if(clockwise == true)
                        {
                            firstVec = currentVertex._vertex;
                            secondVec = (currentVertex._node.Next == null ? originalPolygon.First : currentVertex._node.Next).Value;
                        }else{
                            firstVec = (currentVertex._node.Previous == null ? originalPolygon.Last : currentVertex._node.Previous).Value;
                            secondVec = currentVertex._vertex;
                        }

                        key = new Edge(firstVec, secondVec);

                        if(hashTable.TryGetValue(key, out helper))
                        {
                            if(helper._type == VertexType.Merge)
                            {
                                edges.AddLast(new Edge(currentVertex._vertex, helper._vertex));
                            }

                            hashTable.Remove(key);
                        }

                        break;

                    case VertexType.Split:
                        //Set this vertex as an helper
                        key = FindLeftEdge(in hashTable, in originalPolygon, currentVertex);

                        if(hashTable.TryGetValue(key, out helper))
                        {
                            edges.AddLast(new Edge(currentVertex._vertex, hashTable[key]._vertex));

                            hashTable[key] = currentVertex;

                            if(clockwise == true)
                            {
                                firstVec = (currentVertex._node.Previous == null ? originalPolygon.Last : currentVertex._node.Previous).Value;
                                secondVec = currentVertex._vertex;
                            }else{
                                firstVec = currentVertex._vertex;
                                secondVec = (currentVertex._node.Next == null ? originalPolygon.First : currentVertex._node.Next).Value;
                            }
                            
                            hashTable.Add(new Edge(firstVec, secondVec), currentVertex);
                        }
                        break; 

                    case VertexType.Merge:
                        //Ceate an the edge if edge which ends with the current vertex if his helper is a merge vertex 
                        
                        if(clockwise == true)
                        {
                            firstVec = currentVertex._vertex;
                            secondVec = (currentVertex._node.Next == null ? originalPolygon.First : currentVertex._node.Next).Value;
                        }else{
                            firstVec = (currentVertex._node.Previous == null ? originalPolygon.Last : currentVertex._node.Previous).Value;
                            secondVec = currentVertex._vertex;
                        }
                        
                        key = new Edge(firstVec, secondVec);

                        if(hashTable.TryGetValue(key, out helper))
                        {
                            if(helper._type == VertexType.Merge)
                            {
                                edges.AddLast(new Edge(currentVertex._vertex, helper._vertex));
                            }
                        }


                        //Ceate an the edge if the edge on the left of this point if his helper is a merge vertex 
                        hashTable.Remove(key);

                        key = FindLeftEdge(in hashTable, in originalPolygon, in currentVertex);

                        if(hashTable.TryGetValue(key, out helper))
                        {
                            if(helper._type == VertexType.Merge)
                            {
                                edges.AddLast(new Edge(currentVertex._vertex, helper._vertex));
                            }

                            hashTable[key] = currentVertex;
                        }                        
                        break; 
                    
                    case VertexType.Regular:
                        //If the interior of P is on the right or left of this point
                        secondVec = (currentVertex._node.Next == null ? originalPolygon.First : currentVertex._node.Next).Value;

                        float side = 0f;

                        if(secondVec.y != currentVertex._vertex.y)
                        {
                            side = secondVec.y - currentVertex._vertex.y;
                        }else{
                            secondVec = (currentVertex._node.Previous == null ? originalPolygon.Last : currentVertex._node.Previous).Value;
                            side = currentVertex._vertex.y - secondVec.y;
                        }

                        //On the right
                        if((side > 0 && clockwise == true) || (side < 0 && clockwise == false))
                        {
                            if(clockwise == true)
                            {
                                firstVec = currentVertex._vertex;
                                secondVec = (currentVertex._node.Next == null ? originalPolygon.First : currentVertex._node.Next).Value;
                            }else{
                                firstVec = (currentVertex._node.Previous == null ? originalPolygon.Last : currentVertex._node.Previous).Value;
                                secondVec = currentVertex._vertex;
                            }
                            
                            key = new Edge(firstVec, secondVec);

                            if(hashTable.TryGetValue(key, out helper))
                            {
                                if(helper._type == VertexType.Merge)
                                {
                                    edges.AddLast(new Edge(currentVertex._vertex, helper._vertex));
                                }

                                hashTable.Remove(key);
                            }

                            if(clockwise == true)
                            {
                                firstVec = (currentVertex._node.Previous == null ? originalPolygon.Last : currentVertex._node.Previous).Value;
                                secondVec = currentVertex._vertex;
                            }else{
                                firstVec = currentVertex._vertex;
                                secondVec = (currentVertex._node.Next == null ? originalPolygon.First : currentVertex._node.Next).Value;
                            }

                            hashTable.Add(new Edge(firstVec, secondVec), currentVertex);

                        }else{//On the left

                            key = FindLeftEdge(in hashTable, in originalPolygon, in currentVertex);

                            if(hashTable.TryGetValue(key, out helper))
                            {
                                if(helper._type == VertexType.Merge)
                                {
                                    edges.AddLast(new Edge(currentVertex._vertex, helper._vertex));
                                }

                                hashTable[key] = currentVertex;
                            }
                        }
                        break; 
                }
            }

            //Create the monotone polygons from the edges and polygon points
            PolygonTriangulation.EdgesToPolygons(originalPolygon, edges, out monotonePolygons);

            return;
        }

        //Find the directly left edge
        static private Edge FindLeftEdge(in Dictionary<Edge, SortedVertex> hashTable, in LinkedList<Vector2> originalPolygon, in SortedVertex currentVertex)
        {   
            Edge leftEdge = new Edge(Vector2.zero, Vector2.zero);
            float lastX = Single.NegativeInfinity;

            Dictionary<Edge, SortedVertex>.KeyCollection keyColl = hashTable.Keys;

            //Each string is composed of to index
            foreach(Edge key in keyColl)
            {
                Vector2 intersection;

                //I believe negative infinity created a bug with segment intersection
                if(SegmentIntersection(key._vecA, key._vecB, new Vector2(-99999999999, currentVertex._vertex.y), currentVertex._vertex, out intersection) == true
                || ((key._vecA.y == key._vecB.y) && (key._vecA.y == currentVertex._vertex.y)))//Regular vertex may be colinear
                {
                    if(intersection.x > lastX)
                    {
                        lastX = intersection.x;
                        leftEdge = key;
                    }
                }
            }

            return leftEdge;
        }

        //Returns the point of intersection between the two segment, if there's one
        static public bool SegmentIntersection(Vector2 A, Vector2 B, Vector2 I, Vector2 P, out Vector2 result)
        {
            result = new Vector2(0, 0);
            Vector2 AB = new Vector2((A.x - B.x), (A.y - B.y));
            Vector2 IP = new Vector2((I.x - P.x), (I.y - P.y));

            float det = AB.x * IP.y - AB.y * IP.x;

            if(det == 0)
            {
                //Parallele ou colinéaire
                return false;
            }else {

                if (CollisionSegSeg(A, B, I, P)) {
                    float t1 = ((A.x * B.y - A.y * B.x) * (I.x - P.x) - (A.x - B.x) * (I.x * P.y - I.y * P.x)) / det;
                    float t2 = ((A.x * B.y - A.y * B.x) * (I.y - P.y) - (A.y - B.y) * (I.x * P.y - I.y * P.x)) / det;

                    result.x = t1;
                    result.y = t2;

                    return true;
                }

                return false;
            }
        }

        static public bool CollisionDroiteSeg(Vector2 A, Vector2 B, Vector2 O, Vector2 P)
        {
            Vector2 AO = new Vector2(0, 0);
            Vector2 AP = new Vector2(0, 0); 
            Vector2 AB = new Vector2(0, 0);
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

        static public bool CollisionSegSeg(Vector2 A, Vector2 B, Vector2 O, Vector2 P)
        {
            if (CollisionDroiteSeg(A, B, O, P) == false)
                return false;
            if (CollisionDroiteSeg(O, P, A, B) == false)
                return false;
            return true;
        }

        //Triangulate a y-monotone polygon in O(n)
        static public LinkedList<Vector2> TriangulateMonotonePolygon(LinkedList<Vector2> polygon, ref LinkedList<Vector2> triangles)
        {
            int i = 0;

            if(polygon.Count < 3)
            {
                //If we can't make a single triangle return an empty list
                return triangles;
            }else if(polygon.Count == 3)
            {
                LinkedListNode<Vector2> polygonNode;

                for(polygonNode = polygon.First; polygonNode != null; polygonNode = polygonNode.Next)
                {
                    triangles.AddLast(polygonNode.Value);
                }

                return triangles;
            }

            Vector2[] polygonArray = new Vector2[polygon.Count];
            polygon.CopyTo(polygonArray, 0);

            //Sort the polygon in y decreasing
            Vector2[] sortedPolygon = new Vector2[polygonArray.Length];
            polygonArray.CopyTo(sortedPolygon, 0);
            Array.Sort(sortedPolygon, 0, sortedPolygon.Length, new SortVector2());

            float maxY = sortedPolygon[0].y;
            float minY = sortedPolygon[sortedPolygon.Length-1].y;

            int firstIndex = Array.IndexOf(polygonArray, sortedPolygon[0]);
            int secondIndex = Array.IndexOf(polygonArray, sortedPolygon[sortedPolygon.Length-1]);
            bool startLeftChain;

            if(firstIndex > secondIndex)
            {
                int a = firstIndex;
                firstIndex = secondIndex;
                secondIndex = a;
            }

            if(polygonArray[firstIndex].x > polygonArray[firstIndex-1 < 0 ? polygonArray.Length-1 : firstIndex-1].x)
            {
                startLeftChain = true;
            }else{
                startLeftChain = false;
            }

            bool clockwise = PolygonTriangulation.isClockwise(in polygon);

            //Get the left and right chain of the polygon
            List<Vector2> rightChain = new List<Vector2>();
            List<Vector2> leftChain = new List<Vector2>();

            if(startLeftChain == true)
            {
                leftChain.AddRange(new List<Vector2>(polygonArray).GetRange(0, (int)firstIndex+1));
                rightChain.AddRange(new List<Vector2>(polygonArray).GetRange((int)firstIndex, (int)secondIndex - (int)firstIndex + 1));
                leftChain.AddRange(new List<Vector2>(polygonArray).GetRange((int)secondIndex, polygonArray.Length - (int)secondIndex));
            }else{
                rightChain.AddRange(new List<Vector2>(polygonArray).GetRange(0, (int)firstIndex + 1));
                leftChain.AddRange(new List<Vector2>(polygonArray).GetRange((int)firstIndex, (int)secondIndex - (int)firstIndex + 1));
                rightChain.AddRange(new List<Vector2>(polygonArray).GetRange((int)secondIndex, polygonArray.Length - (int)secondIndex));
            }

            //Find the edges we need to add
            Stack stack = new Stack();

            stack.Push(sortedPolygon[0]);
            stack.Push(sortedPolygon[1]);

            LinkedList<Edge> edges = new LinkedList<Edge>();

            for(i = 2;  i < sortedPolygon.Length-1; i++)
            {
                bool left = leftChain.Contains(sortedPolygon[i]);
                bool right = rightChain.Contains((Vector2)stack.Peek());

                //If the top stack vertex and the current vertex is on different chains
                if(left == right)// && Math.Abs(indexOfCurrentVertex - polygon.IndexOf((Vector2)stack.Peek())) > 1)
                {
                    while(stack.Count > 0)
                    {
                        if(stack.Count > 1)
                        {
                            edges.AddLast(new Edge(sortedPolygon[i], (Vector2)stack.Pop()));
                        }else{
                            stack.Pop();
                        }
                    }

                    stack.Push(sortedPolygon[i-1]);
                    stack.Push(sortedPolygon[i]);
                
                }else{
                
                    bool inside = true;

                    Vector2 lastPopped = (Vector2)stack.Pop(); 

                    //While the edge connecting the vertex at top stack and the current vertex, is inside the polygon
                    //Add the edges
                    while(inside == true && stack.Count != 0)
                    {
                        Vector2 toPop = (Vector2)stack.Peek();

                        //Check collision with popped vertex-current vertex segment and with polygon segments
                        int i2 = 0;

                        do{
                            if(CollisionSegSeg(toPop, sortedPolygon[i], polygonArray[i2], polygonArray[(i2 + 1) >= polygonArray.Length ? 0 : (i2 + 1)]) == true)
                            {
                                inside = false;
                            }

                            i2++;

                        }while(inside == true && i2 < sortedPolygon.Length);

                        //If no collision was found
                        //Check if were completely inside or outside
                        if(inside == true)
                        {
                            int indexOfToPop = Array.IndexOf(polygonArray, toPop);
                            int indexOfCurrentVertex = Array.IndexOf(polygonArray, sortedPolygon[i]);
                            
                            //float side = 0;
                            byte side = 0;
                            int pivotIndex;

                            if(indexOfToPop < indexOfCurrentVertex)
                            {
                                pivotIndex = (indexOfToPop + 1) >= polygon.Count ? 0 : (indexOfToPop + 1);
                                //side = Vector2.SignedAngle(polygon[pivotIndex] - sortedPolygon[i], polygon[pivotIndex] - toPop);
                                
                            }else{
                                pivotIndex = (indexOfCurrentVertex + 1) >= polygon.Count ? 0 : (indexOfCurrentVertex + 1);
                                //side = Vector2.SignedAngle(polygon[pivotIndex] - sortedPolygon[i], polygon[pivotIndex] - toPop);
                            }

                            side = Orientation(polygonArray[pivotIndex], sortedPolygon[i], toPop);

                            if((side < 0 && clockwise == true) || (side > 0 && clockwise == false) || side == 0 || side == 180)//Inside
                            {
                                edges.AddLast(new Edge(sortedPolygon[i], toPop));
                                lastPopped = toPop;

                                stack.Pop();
                            }else{
                                inside = false;
                            }
                        }
                    }

                    stack.Push(lastPopped);
                    stack.Push(sortedPolygon[i]);
                }
            }
            
            stack.Pop();

            Vector2 lastVertex = sortedPolygon[sortedPolygon.Length-1];
            
            //Connect the last vertex to all the vertices left inthe stack axcept the last one
            while(stack.Count > 1)
            {
                edges.AddLast(new Edge(lastVertex, (Vector2)stack.Pop()));
            }

            //stack.Pop();

            LinkedList<LinkedList<Vector2>> polygons;
            

            PolygonTriangulation.EdgesToPolygons(polygonArray, edges, out polygons);
            
            LinkedListNode<LinkedList<Vector2>> polygonsListNode;

            for(polygonsListNode = polygons.First; polygonsListNode != null; polygonsListNode = polygonsListNode.Next)
            {
                LinkedListNode<Vector2> polygonsVectorNode;

                if(isClockwise(polygonsListNode.Value) == true)
                {
                    for(polygonsVectorNode = polygonsListNode.Value.First; polygonsVectorNode != null; polygonsVectorNode = polygonsVectorNode.Next)
                    {
                        triangles.AddLast(polygonsVectorNode.Value);
                    }
                }else{
                    for(polygonsVectorNode = polygonsListNode.Value.Last; polygonsVectorNode != null; polygonsVectorNode = polygonsVectorNode.Previous)
                    {
                        triangles.AddLast(polygonsVectorNode.Value);
                    }
                }



                /*Vector2 vecA = polygonsListNode.Value.First.Value;
                Vector2 vecB = polygonsListNode.Value.First.Next.Value;
                Vector2 vecC = polygonsListNode.Value.First.Next.Next.Value;

                if(vecB.x < vecA.x || (vecB.x == vecA.x && vecC.x < vecB.x))
                {
                    triangles.AddLast(vecA);
                    triangles.AddLast(vecB);
                    triangles.AddLast(vecC);
                }else{
                    triangles.AddLast(vecC);
                    triangles.AddLast(vecB);
                    triangles.AddLast(vecA);
                }*/

            }

            return triangles;
        }

        //Takes a clockwise orderes list of polygon points, and a list of edges
        //And returns a list of polygons 
        static private void EdgesToPolygons(in LinkedList<Vector2> polygon, LinkedList<Edge> edges, out LinkedList<LinkedList<Vector2>> polygonsCreated)
        {
            
            if(edges.Count == 0)
            {
                polygonsCreated = new LinkedList<LinkedList<Vector2>>();
                polygonsCreated.AddLast(new LinkedList<Vector2>(polygon));
                return;
            }

            Vector2[] polygonArray = new Vector2[polygon.Count];
            polygon.CopyTo(polygonArray, 0);
            
            EdgesToPolygons(polygonArray, edges, out polygonsCreated);
        }

        static private void EdgesToPolygons(in Vector2[] polygon, LinkedList<Edge> edges, out LinkedList<LinkedList<Vector2>> polygonsCreated)
        {
            polygonsCreated = new LinkedList<LinkedList<Vector2>>();

            if(edges.Count == 0)
            {
                polygonsCreated.AddLast(new LinkedList<Vector2>(polygon));
                return;
            }

            //Breadth First Search
            Queue<int> frontier;
            Dictionary<int, int> cameFrom;
            SortedDictionary<Vector2, int> futureFrontier = new SortedDictionary<Vector2, int>(new SortVector2());
            int current = -1;
            Queue<Edge> ropeQueue = new Queue<Edge>();
            Edge currentRope;
            LinkedList<Edge> blacklist = new LinkedList<Edge>();
            int increment = 0;
            Edge topRope = edges.First.Value;

            LinkedListNode<Edge> edgesNode;
            Dictionary<Vector2, int> hashTable = new Dictionary<Vector2, int>(polygon.Length);

            for(int i = 0; i < polygon.Length; i++)
            {
                hashTable.Add(polygon[i], i);
            }

            for(edgesNode = edges.First; edgesNode != null; edgesNode = edgesNode.Next)
            {
                edgesNode.Value._pointA = hashTable[edgesNode.Value._vecA];
                edgesNode.Value._pointB = hashTable[edgesNode.Value._vecB];
            }

            //Remove the new edges that are already part of the polygon
            edgesNode = edges.First;

            while(edgesNode != null)
            {
                bool skip = false;

                if(Math.Abs(edgesNode.Value._pointA - edgesNode.Value._pointB) == 1 || Math.Abs(edgesNode.Value._pointA - edgesNode.Value._pointB) == polygon.Length-1)
                {
                    if(edgesNode.Previous != null)
                    {
                        edgesNode = edgesNode.Previous;
                        edges.Remove(edgesNode.Next);
                    }else{
                        skip = true;
                        edges.Remove(edgesNode);
                        edgesNode = edges.First;
                    }
                    
                }

                if(skip == false)   edgesNode = edgesNode.Next;
            }

            IComparer<Edge> comparer = new SortEdge();

            for(edgesNode = edges.First; edgesNode != null; edgesNode = edgesNode.Next)
            {
                if(comparer.Compare(edgesNode.Value, topRope) == 1)
                {
                    topRope = edgesNode.Value;
                }
            }

            currentRope = topRope;

            while(edges.Count+1 > polygonsCreated.Count)
            {
                //Initiatalise Breadth First Search
                int goal = -1;
                int start = -1;
                bool found = false;
                frontier = new Queue<int>();
                cameFrom = new Dictionary<int, int>();

                //Start at one of the ropes in a side that we have never taken
                if(currentRope._upWay == true)
                {
                    goal = Math.Min(currentRope._pointA, currentRope._pointB);
                    start = Math.Max(currentRope._pointA, currentRope._pointB);

                }else{
                    goal = Math.Max(currentRope._pointA, currentRope._pointB);
                    start = Math.Min(currentRope._pointA, currentRope._pointB);
                }

                frontier.Enqueue(start);
                cameFrom.Add(start, -1);

                //Start main part Breadth First Search
                while(frontier.Count > 0 && found == false)
                {
                    current = (int)frontier.Dequeue();
                    int next = -1;

                    //For each rope
                    for(edgesNode = edges.First; edgesNode != null; edgesNode = edgesNode.Next)
                    {
                        if(edgesNode.Value._pointA == current)
                        {
                            next = edgesNode.Value._pointB;

                        }else if(edgesNode.Value._pointB == current)
                        {
                            next = edgesNode.Value._pointA;
                        }

                        //Check if it's not blacklisted
                        if(next != -1 && ((current < next && edgesNode.Value._upWay == true) || (current > next && edgesNode.Value._downWay == true)) && (next != goal || current != start))
                        {
                            //If we found a path
                            if(next == goal)
                            {
                                found = true;
                                cameFrom.Add(next, current);
                                break;
                            
                            }else if(cameFrom.ContainsKey(next) == false){//Checked that we are not coming back on our feet and that it's not a cycle
                                futureFrontier.Add(polygon[next], next);
                                //frontier.Enqueue(next);
                                cameFrom.Add(next, current);
                            }
                        }
                    }

                    if(found == false)
                    {
                        for(int i = -1; i <= 1; i+=2)
                        {
                            //Increment or decrement from one point
                            next = current + i;

                            if(next == -1)
                            {
                                next = polygon.Length-1;
                            }else if(next == polygon.Length)
                            {
                                next = 0;
                            }

                            //Search if this edge has been blacklisted
                            bool blacklisted = false;
                            
                            LinkedListNode<Edge> blacklistNode = blacklist.First;

                            while(blacklisted == false && blacklistNode != null)
                            {
                                if((blacklistNode.Value._pointA == current && blacklistNode.Value._pointB == next) || (blacklistNode.Value._pointB == current && blacklistNode.Value._pointA == next))
                                {
                                    blacklisted = true;
                                }

                                blacklistNode = blacklistNode.Next;
                            }

                            //If the next point is not blacklisted
                            if(blacklisted == false)
                            {
                                //If we found a cycle
                                if(next == goal)
                                {
                                    found = true;
                                    cameFrom.Add(next, current);
                                    break;
                                
                                }else if(cameFrom.ContainsKey(next) == false && next != -1){//Checked that we are not coming back on our feet and that it's not a cycle
                                    //frontier.Enqueue(next);
                                    futureFrontier.Add(polygon[next], next);
                                    cameFrom.Add(next, current);
                                }
                            }
                        }

                        //Add the sorted frontier to the frontier
                        SortedDictionary<Vector2, int>.ValueCollection valueColl = futureFrontier.Values;

                        foreach(int newPoint in valueColl)
                        {
                            frontier.Enqueue(newPoint);
                        }
                    }

                    futureFrontier.Clear();
                }

                //Ban goal to start
                if((goal - start) < 0)
                {
                    currentRope._upWay = false;
                }else{
                    currentRope._downWay = false;
                }

                //Compute found path Breadth First Search
                current = goal;
                polygonsCreated.AddLast(new LinkedList<Vector2>());

                do{

                    int from = cameFrom[current];

                    found = false;

                    //Blacklisting
                    if((Math.Abs(from - current) == 1 ) || (Math.Abs(from - current) == polygon.Length-1))
                    {
                        blacklist.AddLast(new Edge(from, current));

                    }else{
                        edgesNode = edges.First;

                        while(found == false && edgesNode != null)
                        {
                            if((edgesNode.Value._pointA == current && edgesNode.Value._pointB == from) || (edgesNode.Value._pointB == current && edgesNode.Value._pointA == from))
                            {
                                found = true;
                                ropeQueue.Enqueue(edgesNode.Value);

                                if((from - current) < 0)
                                {
                                    edgesNode.Value._upWay = false;
                                }else{
                                    edgesNode.Value._downWay = false;
                                }
                            }

                            edgesNode = edgesNode.Next;
                        }
                    }

                    polygonsCreated.Last.Value.AddLast(polygon[current]);

                    current = from;
                    
                }while(current != start);

                //Add start
                polygonsCreated.Last.Value.AddLast(polygon[current]);

                increment++;

                //There can only be two polygon for each rope
                if(increment == 2)
                {
                    increment = 1;
                    if(edges.Count+1 != polygonsCreated.Count)
                    {
                        currentRope = ropeQueue.Dequeue();
                    }
                }
            }
        }

        static public bool isClockwise(in LinkedList<Vector2> polygon)
        {
            float sum = 0;
            LinkedListNode<Vector2> polygonNode = polygon.First;

            for(polygonNode = polygon.First; polygonNode != null; polygonNode = polygonNode.Next)
            {
                Vector2 nextVec = (polygonNode.Next == null ? polygon.First : polygonNode.Next).Value;

                sum += (nextVec.x - polygonNode.Value.x) * (nextVec.y + polygonNode.Value.y);
            }

            //Clockwise if sum > 0
            //CounterClockwise if sum < 0
            return (sum > 0);
        }

        // 0 --> a, b and c are colinear
        // 1 --> c is on the right
        // 2 --> c is on the left
        static public byte Orientation(Vector2 a, Vector2 b, Vector2 c)
        {
            double ABx = b.x - a.x;
            double ABy = b.y - a.y;

            double ACx = c.x - a.x;
            double ACy = c.y - a.y;

            double val = ACx * ABy - ACy * ABx;

            if (val == 0) return 0; // collinéire 
            return (byte)((val < 0) ? 1 : 2); // sens horaire ou sens anti-hoaraire
        }

    }
