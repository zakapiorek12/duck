using System;
using OpenTK;

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

        internal Mesh GetWaterMesh(float width, Vector4 surfaceColor, int N)
        {
            Vector3[] vertices = new Vector3[N * N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    vertices[i * N + j] = new Vector3(i/(float)N, 0f, j/(float)N);

            Vector3 faceNormal = Vector3.UnitZ;
            Normalized[] normalized = new Normalized[N * N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    normalized[i * N + j] = new Normalized { normal = new Vector3(0f, 0f, 0f), vertex = vertices[i * N + j] };

            uint[] indices = new uint[normalized.Length];
            for (uint i = 0; i < indices.Length; i++)
                indices[i] = i;

            Mesh m = new Mesh(vertices, normalized, indices);
            m.surfaceColor = surfaceColor;
            m.materialDiffuseSpecularColor = surfaceColor.Xyz;
            return m;
        }
    }
}
