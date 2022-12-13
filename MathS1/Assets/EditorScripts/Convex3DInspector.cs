using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace EditorScripts
{
    [CustomEditor(typeof(Convex3D))]
    public class Convex3DInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var convex3D = (Convex3D)target;
            
            if (GUILayout.Button("Add Point"))
            {
                convex3D.AddPoint();
            }
        
            if (GUILayout.Button("Reset Mesh"))
            {
                Vector3[] vertices = new Vector3[4];
                int[] indices = new int[3*4]{0,1,2, 1,3,2, 0,2,3, 0,3,1};//Need to always swap between clockwise and counterclockwise index

                vertices[0] = new Vector3(1, 0, 0);
                vertices[1] = new Vector3(0, 1, 0);
                vertices[2] = new Vector3(1, 1, 0);
                vertices[3] = new Vector3(1, 0, 1);
        
                convex3D.meshFilter.sharedMesh.SetVertices(vertices);
                convex3D.meshFilter.sharedMesh.SetIndices(indices, MeshTopology.Triangles, 0);
            }
            
            convex3D.meshFilter = (MeshFilter)EditorGUILayout.ObjectField("Mesh filter", convex3D.meshFilter, typeof(MeshFilter), true);
            convex3D.meshRenderer = (MeshRenderer)EditorGUILayout.ObjectField("Mesh renderer", convex3D.meshRenderer, typeof(MeshRenderer), true);
            convex3D.meshTransform = (Transform)EditorGUILayout.ObjectField("Mesh transform", convex3D.meshTransform, typeof(Transform), true);
            convex3D.pointToAdd = (Transform)EditorGUILayout.ObjectField("Point to add", convex3D.pointToAdd, typeof(Transform), true);
            convex3D.sphere = (Transform)EditorGUILayout.ObjectField("Sphere", convex3D.sphere, typeof(Transform), true);
        }
    }
}
