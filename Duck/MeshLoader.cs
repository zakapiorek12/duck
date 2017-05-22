using System;
using OpenTK;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Duck
{

    class MeshLoader
    {
        Mesh[] mesh;

        public Mesh GetWaterMesh(float width, Vector4 surfaceColor, uint N)
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

                //left face
                new Normalized() {normal = leftNormal, vertex = vertices[6] },
                new Normalized() {normal = leftNormal, vertex = vertices[2] },
                new Normalized() {normal = leftNormal, vertex = vertices[3] },
                new Normalized() {normal = leftNormal, vertex = vertices[7] },

                //right face
                new Normalized() {normal = rightNormal, vertex = vertices[1] },
                new Normalized() {normal = rightNormal, vertex = vertices[5] },
                new Normalized() {normal = rightNormal, vertex = vertices[4] },
                new Normalized() {normal = rightNormal, vertex = vertices[0] },

                //top face
                new Normalized() {normal = topNormal, vertex = vertices[4] },
                new Normalized() {normal = topNormal, vertex = vertices[7] },
                new Normalized() {normal = topNormal, vertex = vertices[3] },
                new Normalized() {normal = topNormal, vertex = vertices[0] },

                //bottom face
                new Normalized() {normal = bottomNormal, vertex = vertices[2] },
                new Normalized() {normal = bottomNormal, vertex = vertices[6] },
                new Normalized() {normal = bottomNormal, vertex = vertices[5] },
                new Normalized() {normal = bottomNormal, vertex = vertices[1] },
            };
            uint[] indices = new uint[normalized.Length];
            for (uint i = 0; i < indices.Length; i++)
                indices[i] = i;

            Mesh m = new Mesh(vertices, normalized, indices);
            m.isCube = 1;
            return m;
        }

        public Mesh LoadMesh(string name)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"mesh", name + ".txt");

            List<Vector3> vertexes = new List<Vector3>();
            List<Normalized> normalized = new List<Normalized>();
            List<uint> indexes = new List<uint>();

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NegativeSign = "-";

            using (FileStream fileStream = File.OpenRead(path))
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                string line = streamReader.ReadLine();
                uint vertexCount = uint.Parse(line);
                for (int i = 0; i < vertexCount; i++)
                {
                    line = streamReader.ReadLine();
                    string[] xyz = line.Split(' ');
                    float x = float.Parse(xyz[0], nfi);
                    float y = float.Parse(xyz[1], nfi);
                    float z = float.Parse(xyz[2], nfi);

                    float nx = float.Parse(xyz[3], nfi);
                    float ny = float.Parse(xyz[4], nfi);
                    float nz = float.Parse(xyz[5], nfi);

                    float tx = float.Parse(xyz[6], nfi);
                    float ty = float.Parse(xyz[7], nfi);

                    normalized.Add(new Normalized
                    {
                        normal = new Vector3(nx, ny, nz),
                        vertex = new Vector3(x, y, z),
                        texturePos = new Vector2(tx, ty)
                    });
                }

                line = streamReader.ReadLine();
                uint trainglesCount = uint.Parse(line);
                for (int i = 0; i < trainglesCount; i++)
                {
                    line = streamReader.ReadLine();
                    string[] abc = line.Split(' ');
                    indexes.Add(uint.Parse(abc[0]));
                    indexes.Add(uint.Parse(abc[1]));
                    indexes.Add(uint.Parse(abc[2]));
                }
            }

            return new Mesh(new Vector3[0], normalized.ToArray(), indexes.ToArray());
        }
    }
}
