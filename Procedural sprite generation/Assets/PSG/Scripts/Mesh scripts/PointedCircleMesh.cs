﻿using UnityEngine;
using System.Collections.Generic;

namespace PSG
{
    /// <summary>
    /// Circle with peak.
    /// If peak is within the circle radius, it
    /// degenerates to plain circle.
    /// 
    /// Colliders:
    ///     - Circle and Polygon (if {shift} exceeds circle)
    ///     - Circle (if shape is degenerated)
    /// </summary>
    public class PointedCircleMesh : MeshBase
    {
        //mesh data
        private List<Vector3> vertices;
        private List<int> triangles;
        private List<Vector2> uvs;

        //p-circle data
        private float radius;
        private Vector2 shift;
        private int sides;

        //colliders
        private CircleCollider2D C_CC2D;
        private PolygonCollider2D C_PC2D;

        #region Static Methods - building from values and from structure

        public static PointedCircleMesh AddPointedCircleMesh(Vector3 position, float radius, int sides, Vector2 shift, Material meshMatt = null, bool attachRigidbody = true)
        {
            MeshHelper.CheckMaterial(ref meshMatt);
            GameObject pointedCircle = new GameObject();
            pointedCircle.transform.position = position;
            PointedCircleMesh pointedCircleComponent = pointedCircle.AddComponent<PointedCircleMesh>();
            pointedCircleComponent.Build(radius, sides, shift, meshMatt);
            if (attachRigidbody)
            {
                pointedCircle.AddComponent<Rigidbody2D>();
            }
            return pointedCircleComponent;
        }

        public static PointedCircleMesh AddPointedCircleMesh(Vector3 position, PointedCircleStructure pointedCircleStructure, Material meshMatt = null, bool attachRigidbody = true)
        {
            return AddPointedCircleMesh(position, pointedCircleStructure.radius, pointedCircleStructure.sides, pointedCircleStructure.shift, meshMatt, attachRigidbody);
        }

        #endregion

        #region Public Build

        //assign variables, get components and build mesh
        public void Build(float radius, int sides, Vector2 shift, Material meshMatt = null)
        {
            MeshHelper.CheckMaterial(ref meshMatt);
            name = "PointedCircle";
            this.radius = radius;
            this.sides = sides;
            this.shift = shift;

            _Mesh = new Mesh();
            GetOrAddComponents();

            C_MR.material = meshMatt;

            if (BuildPointedCircle(radius, sides, shift))
            {
                UpdateMesh();
                UpdateCollider();
            }
        }

        void Build(PointedCircleStructure pointedCircleStructure, Material meshMatt = null)
        {
            Build(pointedCircleStructure.radius, pointedCircleStructure.sides, pointedCircleStructure.shift, meshMatt);
        }

        #endregion

        //build p-circle
        private bool BuildPointedCircle(float radius, int sides, Vector2 shift)
        {
            #region Validity Check

            if (sides < 2)
            {
                Debug.LogWarning("PointedCircleMesh::AddPointedCircle: radius can't be equal to zero!");
                return false;
            }
            if (radius == 0)
            {
                Debug.LogWarning("PointedCircleMesh::AddPointedCircle: radius can't be equal to zero!");
                return false;
            }
            if (radius < 0)
            {
                radius = -radius;
            }

            #endregion

            vertices = new List<Vector3>();
            triangles = new List<int>();

            float angleDelta = deg360 / sides;
            vertices.Add(shift);
            for (int i = 1; i < sides + 1; i++)
            {
                Vector3 vertPos = new Vector3(Mathf.Cos(i * angleDelta), Mathf.Sin(i * angleDelta)) * radius;
                vertices.Add(vertPos);
                triangles.Add(1 + i % sides);
                triangles.Add(1 + (i - 1) % sides);
                triangles.Add(0);
            }
            uvs = MeshHelper.UVUnwrap(vertices.ToArray());

            return true;
        }

        public PointedCircleStructure GetStructure()
        {
            return new PointedCircleStructure
            {
                radius = radius,
                shift = shift,
                sides = sides
            };
        }

        #region Abstract Implementation

        public override Vector3[] GetVertices()
        {
            return vertices.ToArray();
        }

        public override void GetOrAddComponents()
        {
            C_CC2D = gameObject.GetOrAddComponent<CircleCollider2D>();
            C_MR = gameObject.GetOrAddComponent<MeshRenderer>();
            C_MF = gameObject.GetOrAddComponent<MeshFilter>();
        }

        public override void UpdateCollider()
        {
            C_CC2D.radius = radius;

            if (radius < vertices[0].sqrMagnitude)
            {
                //not added in AddOrGetComponents
                C_PC2D = gameObject.GetOrAddComponent<PolygonCollider2D>();

                Vector2[] C_CC2D_vertices = new Vector2[3];

                float shiftedVertexAngle = Mathf.Atan2(vertices[0].y, vertices[0].x);

                C_CC2D_vertices[0] = vertices[0];
                C_CC2D_vertices[1] = new Vector2(Mathf.Cos(shiftedVertexAngle - Mathf.PI * 0.5f), Mathf.Sin(shiftedVertexAngle - Mathf.PI * 0.5f)) * radius;
                C_CC2D_vertices[2] = new Vector2(Mathf.Cos(shiftedVertexAngle + Mathf.PI * 0.5f), Mathf.Sin(shiftedVertexAngle + Mathf.PI * 0.5f)) * radius;

                C_PC2D.SetPath(0, C_CC2D_vertices);
            }
        }

        public override void UpdateMesh()
        {
            _Mesh.Clear();
            _Mesh.vertices = vertices.ToArray();
            _Mesh.triangles = triangles.ToArray();
            _Mesh.uv = uvs.ToArray();
            _Mesh.normals = MeshHelper.AddMeshNormals(vertices.Count);
            C_MF.mesh = _Mesh;
        }

        #endregion

    } 

    public struct PointedCircleStructure
    {
        public float radius;
        public Vector2 shift;
        public int sides;
    }
}