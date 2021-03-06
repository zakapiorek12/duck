﻿using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Duck
{
    public struct Normalized
    {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector2 texturePos;
    }

    public class Mesh
    {
        public Matrix4 ModelMatrix = Matrix4.Identity, InvertedModelMatrix;

        public uint[] IndexBuffer { get; set; }
        public Vector3[] VertexBuffer { get; set; }
        public Normalized[] NormalizedVertexBuffer { get; set; }
        public bool MainObject { get; set; }

        private int VAOId;
        private int positionVbo;
        private int normalsVbo;
        private int indicesVbo;
        private int textureVbo;

        public Vector4 surfaceColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
        public float materialSpecExponent = 32f;
        public Vector3 materialDiffuseSpecularColor = new Vector3(0.5f, 0.5f, 0.5f);
        public int isCube = 0;

        public Mesh(Vector3[] vertices, Normalized[] normalized, uint[] indices)
        {
            this.VertexBuffer = vertices;
            this.NormalizedVertexBuffer = normalized;
            this.IndexBuffer = indices;

            InitializeVAO();
        }

        public void BindVAO()
        {
            GL.BindVertexArray(VAOId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesVbo);
        }

        private void InitializeVAO()
        {
            CreateVbos();
            FillVbos();
            CreateElementBufferObject();

            GL.GenVertexArrays(1, out VAOId);
            GL.BindVertexArray(VAOId);

            //element array buffer jest wyjatkiem - nie trzeba podawac layoutu - wystarczy zbindowac buffer przy bindowaniu stanu do danego VAO

            GL.BindBuffer(BufferTarget.ArrayBuffer, positionVbo);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, normalsVbo);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, textureVbo);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);
        }

        private void CreateVbos()
        {
            positionVbo = GL.GenBuffer();
            normalsVbo = GL.GenBuffer();
            textureVbo = GL.GenBuffer();
        }

        public void UpdatePosition(float x, float y, float z, int vertNo)
        {
            NormalizedVertexBuffer[vertNo].vertex.X = x;
            NormalizedVertexBuffer[vertNo].vertex.Y = y;
            NormalizedVertexBuffer[vertNo].vertex.Z = z;
        }

        internal void UpdateNormal(Vector3 normal, int vertNo)
        {
            NormalizedVertexBuffer[vertNo].normal = normal;
        }

        public void FillVbos()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, positionVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * NormalizedVertexBuffer.Length),
                NormalizedVertexBuffer.Select((n) => n.vertex).ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, normalsVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * NormalizedVertexBuffer.Length),
                NormalizedVertexBuffer.Select((n) => n.normal).ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, textureVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector2.SizeInBytes * NormalizedVertexBuffer.Length),
                NormalizedVertexBuffer.Select((n) => n.texturePos).ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }


        private void CreateElementBufferObject()
        {
            indicesVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesVbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * IndexBuffer.Length),
                IndexBuffer, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void CalculateInverted()
        {
            InvertedModelMatrix = ModelMatrix.Inverted();
        }
    }
}
