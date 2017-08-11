﻿using UnityEngine;
using System.Collections.Generic;

namespace PSG
{
    /// <summary>
    /// Simple ellipse shape.
    /// If both radiuses are equal, we consider it as a circle.
    /// 
    /// Colliders:
    ///     - Polygon
    /// </summary>
    public class EllipseMesh : MeshBase
    {
        //mesh data
        private List<Vector3> vertices;
        private List<int> triangles;
        private List<Vector2> uvs;

        //ellipse data
        private float radiusHorizontal;
        private float radiusVertical;
        private int sides;

        //collider
        private PolygonCollider2D C_PC2D;

        #region Static Methods - building from values and from structure

        public static EllipseMesh AddEllipseMesh(Vector3 position, float radiusHorizontal, float radiusVertical, int sides, Material meshMatt = null, bool attachRigidbody = true)
        {
            MeshHelper.CheckMaterial(ref meshMatt);
            GameObject ellipse = new GameObject();
            ellipse.transform.position = position;
            EllipseMesh ellipseComponent = ellipse.AddComponent<EllipseMesh>();
            ellipseComponent.Build(radiusHorizontal, radiusVertical, sides, meshMatt);
            if (attachRigidbody)
            {
                ellipse.AddComponent<Rigidbody2D>();
            }
            return ellipseComponent;
        }

        public static EllipseMesh AddEllipseMesh(Vector3 position, EllipseMeshStructure ellipseMeshStructure, Material meshMatt = null, bool attachRigidbody = true)
        {
            return AddEllipseMesh(position, ellipseMeshStructure.radiusHorizontal, ellipseMeshStructure.radiusVertical, ellipseMeshStructure.sides, meshMatt, attachRigidbody);
        }

        #endregion

        #region Public Build

        //assign variables, get components and build mesh
        public void Build(float radiusHorizontal, float radiusVertical, int sides, Material meshMatt = null)
        {
            MeshHelper.CheckMaterial(ref meshMatt);
            name = "Ellipse";
            this.radiusHorizontal = radiusHorizontal;
            this.radiusVertical = radiusVertical;
            this.sides = sides;

            _Mesh = new Mesh();
            GetOrAddComponents();

            C_MR.material = meshMatt;

            if (BuildEllipse(radiusHorizontal, radiusVertical, sides))
            {
                UpdateMesh();
                UpdateCollider();
            }
        }

        public void Build(EllipseMeshStructure ellipseMeshStructure, Material meshMatt = null)
        {
            Build(ellipseMeshStructure.radiusHorizontal, ellipseMeshStructure.radiusVertical, ellipseMeshStructure.sides, meshMatt);
        }

        #endregion

        //build an ellipse
        private bool BuildEllipse(float radiusA, float radiusB, int sides)
        {

            #region Validity Check

            if (sides < 2)
            {
                Debug.LogWarning("EllipseMesh::BuildEllipse: sides count can't be less than two!");
                return false;
            }
            if (radiusA == 0 || radiusB == 0)
            {
                Debug.LogWarning("EllipseMesh::BuildEllipse: radiuses can't be equal to zero!");
                return false;
            }
            if (radiusA < 0)
            {
                radiusA = -radiusA;
            }
            if (radiusB < 0)
            {
                radiusB = -radiusB;
            }

            #endregion

            vertices = new List<Vector3>();
            triangles = new List<int>();
            uvs = new List<Vector2>();

            vertices.Add(new Vector3(0, 0));
            uvs.Add(new Vector2(0.5f, 0.5f));
            float angleDelta = deg360 / sides;
            for (int i = 1; i < sides + 1; i++)
            {
                Vector3 vertPos = new Vector3(Mathf.Cos((i + 1) * angleDelta) * radiusA, Mathf.Sin((i + 1) * angleDelta) * radiusB);
                vertices.Add(vertPos);
                uvs.Add(new Vector3(vertPos.x / 2 / radiusA, vertPos.y / 2 / radiusB) + new Vector3(0.5f, 0.5f, 0));
                triangles.Add(1 + i % sides);
                triangles.Add(1 + (i - 1) % sides);
                triangles.Add(0);
            }

            return true;
        }

        public EllipseMeshStructure GetStructure()
        {
            return new EllipseMeshStructure
            {
                radiusHorizontal = radiusHorizontal,
                radiusVertical = radiusVertical,
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
            C_PC2D = gameObject.GetOrAddComponent<PolygonCollider2D>();
            C_MR = gameObject.GetOrAddComponent<MeshRenderer>();
            C_MF = gameObject.GetOrAddComponent<MeshFilter>();
        }

        public override void UpdateCollider()
        {
            Vector2[] points = new Vector2[sides + 1];
            float angleDelta = deg360 / sides;
            for (int i = 0; i < sides + 1; i++)
            {
                points[i] = new Vector3(Mathf.Cos((i + 1) * angleDelta) * radiusHorizontal, Mathf.Sin((i + 1) * angleDelta) * radiusVertical);
            }
            C_PC2D.points = points;
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

    public struct EllipseMeshStructure
    {
        public float radiusHorizontal;
        public float radiusVertical;
        public int sides;
    }
}