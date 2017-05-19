using System;
using OpenTK;
using System.Collections.Generic;

namespace Duck
{
    class MeshLoader
    {
        Mesh[] mesh;

        public Mesh GetRectangleMesh(float width, float height, Vector4 surfaceColor)
        {
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-width/2.0f, -height/2.0f, 0.0f),
                new Vector3(-width/2.0f, height/2.0f, 0.0f),
                new Vector3(width/2.0f, -height/2.0f, 0.0f),
                new Vector3(width/2.0f, height/2.0f, 0.0f)
            };
            Vector3 faceNormal = Vector3.UnitZ;
            Normalized[] normalized = new Normalized[]
            {
                //front face
                new Normalized() {normal = faceNormal, vertex = vertices[0], texturePos = new Vector2(0.0f, 0.0f)},
                new Normalized() {normal = faceNormal, vertex = vertices[2], texturePos = new Vector2(0.0f, 1.0f)},
                new Normalized() {normal = faceNormal, vertex = vertices[1], texturePos = new Vector2(1.0f, 0.0f)},
                new Normalized() {normal = faceNormal, vertex = vertices[1], texturePos = new Vector2(1.0f, 0.0f)},
                new Normalized() {normal = faceNormal, vertex = vertices[2], texturePos = new Vector2(0.0f, 1.0f)},
                new Normalized() {normal = faceNormal, vertex = vertices[3], texturePos = new Vector2(1.0f, 1.0f)}
            };
            uint[] indices = new uint[normalized.Length];
            for (uint i = 0; i < indices.Length; i++)
                indices[i] = i;

            Mesh m = new Mesh(vertices, normalized, indices);
            m.surfaceColor = surfaceColor;
            m.materialDiffuseSpecularColor = surfaceColor.Xyz;
            return m;
        }

        internal Mesh GetWaterMesh(float width, Vector4 surfaceColor, uint N)
        {
            float shift = width * N / 2;
            Vector3[] vertices = new Vector3[N * N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    vertices[i * N + j] = new Vector3(i * width - shift, 0f, j * width - shift);

            Vector3 norm = new Vector3(0f, 1f, 0f);
            Vector3 faceNormal = Vector3.UnitZ;
            Normalized[] normalized = new Normalized[N * N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    normalized[i * N + j] = new Normalized { normal = norm, vertex = vertices[i * N + j] };

            List<uint> indices = new List<uint>();
            for (uint i = 0; i < N - 1; i++)
                for (uint j = 0; j < N - 1; j++)
                {
                    indices.Add(i * N + j);
                    indices.Add((i + 1) * N + j);
                    indices.Add((i + 1) * N + (j + 1));
                    indices.Add(i * N + (j + 1));
                }



            Mesh m = new Mesh(vertices, normalized, indices.ToArray());
            m.surfaceColor = surfaceColor;
            m.materialDiffuseSpecularColor = surfaceColor.Xyz;
            return m;
        }

        public Mesh GetCube()
        {
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(1f, 1f, 1f),
                new Vector3(1f, -1f, 1f),
                new Vector3(-1f, -1f, 1f),
                new Vector3(-1f, 1f, 1f),
                new Vector3(1f, 1f, -1f),
                new Vector3(1f, -1f, -1f),
                new Vector3(-1f, -1f, -1f),
                new Vector3(-1f, 1f, -1f),
            };
            Vector3 frontNormal = Vector3.UnitZ,
                    backNormal = -Vector3.UnitZ,
                    leftNormal = Vector3.UnitX,
                    rightNormal = -Vector3.UnitX,
                    topNormal = -Vector3.UnitY,
                    bottomNormal = Vector3.UnitY;
        
            Normalized[] normalized = new Normalized[]
            {
                //front face
                new Normalized() {normal = frontNormal, vertex = vertices[4]},
                new Normalized() {normal = frontNormal, vertex = vertices[5]},
                new Normalized() {normal = frontNormal, vertex = vertices[6]},
                new Normalized() {normal = frontNormal, vertex = vertices[7]},

                //back face
                new Normalized() {normal = backNormal, vertex = vertices[3]},
                new Normalized() {normal = backNormal, vertex = vertices[2]},
                new Normalized() {normal = backNormal, vertex = vertices[1]},
                new Normalized() {normal = backNormal, vertex = vertices[0]},
            };
            uint[] indices = new uint[normalized.Length];
            for (uint i = 0; i < indices.Length; i++)
                indices[i] = i;

            Mesh m = new Mesh(vertices, normalized, indices);
            m.isCube = 1;
            return m;
        }
    }
}
