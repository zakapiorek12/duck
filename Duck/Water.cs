﻿using OpenTK;
using System;
using System.Drawing;

namespace Duck
{
    class Water : ObjectsToDraw
    {
        private float h, c, dt;
        private int N, Ndiv2;
        private float[,] dumpingFactor, firstMap, secondMap;
        private int mainMapNo = 0;
        private Random rand = new Random();

        private float[,] MainMap
        {
            get
            {
                if (mainMapNo == 0)
                    return firstMap;
                else
                    return secondMap;
            }
        }

        private float[,] AssistantMap
        {
            get
            {
                if (mainMapNo == 1)
                    return firstMap;
                else
                    return secondMap;
            }
        }

        public void ChangeMainMap()
        {
            mainMapNo = (mainMapNo + 1)&1;
        }

        public Water(MyShaderType shaderType) : base(shaderType)
        {
            N = 256;
            Ndiv2 = N / 2;
            h = 2f/( N - 1);
            c = 1f;
            dt = 1f/N;

            surfaceColor = new Vector4(.5f, .5f, .5f, .5f);//(0f, .2f, .9f, 1f);

            firstMap = new float[N, N];
            secondMap = new float[N, N];
            dumpingFactor = new float[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    dumpingFactor[i, j] = GetDumpingFactor(i, j);

            MeshLoader ml = new MeshLoader();
            meshes = new Mesh[1] { ml.GetWaterMesh(2f/ N, surfaceColor, (uint)N) };
        }

        private float GetDumpingFactor(int i, int j)
        {
            int invI = N - i, invJ = N - j;
            int max = Math.Min(i, Math.Min(invI, Math.Min(j, invJ)));

            float l = 2f * max / (float)N;
            return 0.95f * Math.Min(1f, l / .2f);
        }

        public override void DoAnimation(float deltaTime)
        {
            if (rand.NextDouble() < 0.7)
                RandomDropOfWater();
            CalculateWave(deltaTime);
        }

        private void CalculateWave(float deltaTime)
        {
            float A = (c * c * dt * dt) / (h * h);
            float B = 2 - 4 * A;

            for (int i = 1; i < N - 1; i++)
                for (int j = 1; j < N - 1; j++)
                {
                    float neighbourSum = AssistantMap[i - 1, j] + AssistantMap[i + 1, j] + AssistantMap[i, j - 1] + AssistantMap[i, j + 1];
                    MainMap[i, j] = dumpingFactor[i, j] * (A * neighbourSum + B * AssistantMap[i, j] - MainMap[i, j]);
                    //meshes[0].UpdatePosition(i / (float)N, MainMap[i, j], j / (float)N, i * N + j);
                }

            for (int i = 1; i < N - 1; i++)
                for (int j = 1; j < N - 1; j++)
                {
                    Vector3 dx = new Vector3(1, (MainMap[i + 1, j] - MainMap[i - 1, j]) * 10, 0);
                    Vector3 dz = new Vector3(0, (MainMap[i, j + 1] - MainMap[i, j - 1]) * 10, 1);
                    Vector3 normal = -Vector3.Cross(dx, dz).Normalized();

                    meshes[0].UpdateNormal(normal, i * N + j);
                }

            meshes[0].FillVbos();
            ChangeMainMap();
        }

        private void RandomDropOfWater()
        {
            Disturb(rand.Next(1, N - 1), rand.Next(1, N - 1));
        }

        public void Disturb(int x, int y)
        {
            MainMap[x, y] = (float)-rand.NextDouble() / 10f;// -.05f;
        }
    }
}
