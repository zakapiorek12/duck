using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Reflection;
using System.Drawing.Imaging;

namespace Duck
{
    public class GLRenderer
    {
        private Matrix4 projectionMatrix;

        private Dictionary<MyShaderType, ShaderProgram> shaders = new Dictionary<MyShaderType, ShaderProgram>();

        private float ambientCoefficient = 1.0f;
        private Vector3 lightColor = new Vector3(0.9f, 0.8f, 0.8f);
        private Vector3 lightPosition = new Vector3(-1.0f, 4.0f, 2.0f);

        private static List<Mesh>[] meshesToDraw;
        private static List<ObjectsToDraw> objectsToDraw;

        MeshLoader meshLoader = new MeshLoader();

        private int plateTextureID, particleTextureID;

        static GLRenderer()
        {
            objectsToDraw = new List<ObjectsToDraw>();
            meshesToDraw = new List<Mesh>[Enum.GetValues(typeof(MyShaderType)).Length];
            for (int i = 0; i < meshesToDraw.Length; i++)
                meshesToDraw[i] = new List<Mesh>();
        }

        public GLRenderer(int viewPortWidth, int viewportHeight)
        {
            LoadShaders();
            //LoadTexture("plate.jpg", ref plateTextureID);
            //LoadTexture("iskra.jpg", ref particleTextureID);
            CreateProjectionMatrix(viewPortWidth, viewportHeight);
            CreateScene();

            GL.ClearColor(Color.Black);
        }

        private void LoadShaders()
        {
            ShaderProgram shaderProgram = new ShaderProgram("shaders/WaterVS.vert",
                "shaders/WaterPS.vert", true);
            shaders.Add(MyShaderType.WATER, shaderProgram);
        }

        public void CreateProjectionMatrix(int viewportWidth, int viewportHeight)
        {
            float far = (float)20f;
            float near = (float)0.1f;
            float fov = (float)(90f / 180f * Math.PI);
            float e = (float)(1f / Math.Tan(fov / 2f));
            float a = (float)viewportHeight / (float)viewportWidth;

            projectionMatrix = new Matrix4(
                e, 0, 0, 0,
                0, e / a, 0, 0,
                0, 0, -(far + near) / (far - near), -2f * far * near / (far - near),
                0, 0, -1, 0);
            projectionMatrix.Transpose();
        }

        private void CreateScene()
        {
            Water water = new Water(MyShaderType.WATER);
            water.AddOnScene();

            /*
            float floorYOffset = -1.0f;
            float roomSize = 10.0f;
            Vector4 roomColor = new Vector4(0.2f, 0.8f, 0.2f, 1.0f);
            Mesh floor = meshLoader.GetRectangleMesh(roomSize, roomSize, roomColor);
            floor.ModelMatrix = Matrix4.CreateRotationX((float)(-Math.PI / 2.0f)) * Matrix4.CreateTranslation(0, floorYOffset, 0);
            GLRenderer.AddMeshToDraw(floor, MyShaderType.PHONG_LIGHT);


            Mesh ceiling = meshLoader.GetRectangleMesh(roomSize, roomSize, roomColor);
            ceiling.ModelMatrix = Matrix4.CreateRotationX((float)(Math.PI / 2.0f)) * Matrix4.CreateTranslation(0, roomSize + floorYOffset, 0);
            GLRenderer.AddMeshToDraw(ceiling, MyShaderType.PHONG_LIGHT);

            Mesh left = meshLoader.GetRectangleMesh(roomSize, roomSize, roomColor);
            left.ModelMatrix = Matrix4.CreateRotationY((float)(Math.PI / 2.0f)) * Matrix4.CreateTranslation(-roomSize / 2.0f, roomSize / 2.0f + floorYOffset, 0);
            GLRenderer.AddMeshToDraw(left, MyShaderType.PHONG_LIGHT);

            Mesh right = meshLoader.GetRectangleMesh(roomSize, roomSize, roomColor);
            right.ModelMatrix = Matrix4.CreateRotationY((float)(-Math.PI / 2.0f)) * Matrix4.CreateTranslation(roomSize / 2.0f, roomSize / 2.0f + floorYOffset, 0);
            GLRenderer.AddMeshToDraw(right, MyShaderType.PHONG_LIGHT);

            Mesh front = meshLoader.GetRectangleMesh(roomSize, roomSize, roomColor);
            front.ModelMatrix = Matrix4.CreateTranslation(0, roomSize / 2.0f + floorYOffset, -roomSize / 2.0f);
            GLRenderer.AddMeshToDraw(front, MyShaderType.PHONG_LIGHT);//*/
        }

        private void LoadTexture(string name, ref int id)
        {
            Bitmap texture = new Bitmap(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "textures", name));

            GL.GenTextures(1, out id);
            GL.BindTexture(TextureTarget.Texture2D, id);

            BitmapData data = texture.LockBits(new System.Drawing.Rectangle(0, 0, texture.Width, texture.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            texture.UnlockBits(data);
        }

        public void DoScene(float deltaTime, Camera camera)
        {
            foreach (ObjectsToDraw otd in objectsToDraw)
                otd.DoAnimation(deltaTime);

            GL.DepthMask(true);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            RenderParticles(deltaTime, camera, true);
            RenderParticles(deltaTime, camera, false);

            //tutaj dodac renderowanie przez nowe shadery
        }

        private void RenderParticles(float deltaTime, Camera camera, bool onlyStencil)
        {
            ShaderProgram activeShader = shaders[MyShaderType.WATER];
            GL.UseProgram(activeShader.ProgramID);

            GL.BindTexture(TextureTarget.Texture2D, particleTextureID);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            BindCameraAndProjectionToShaders(camera, activeShader);
            BindLightDataToShaders(activeShader);

            GL.DepthMask(false);
            foreach (Mesh m in meshesToDraw[(int)MyShaderType.WATER])
                DrawMesh(m, activeShader, PrimitiveType.Points);

            GL.Flush();
        }

        private void DrawMesh(Mesh m, ShaderProgram shader, PrimitiveType mode = PrimitiveType.Triangles)
        {
            m.BindVAO();
            BindMeshMaterialDataToShaders(m, shader);

            GL.UniformMatrix4(shader.GetUniform("object_matrix"), false, ref m.ModelMatrix);
            GL.DrawElements(mode, m.IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);
        }

        private void BindCameraAndProjectionToShaders(Camera camera, ShaderProgram shader)
        {
            GL.UniformMatrix4(shader.GetUniform("cameraview_matrix"), false, ref camera.ResultMatrix);
            Matrix4 cameraModelMatrix = camera.ResultMatrix.Inverted();
            GL.UniformMatrix4(shader.GetUniform("cameraModel_matrix"), false, ref cameraModelMatrix);
            GL.UniformMatrix4(shader.GetUniform("projection_matrix"), false, ref projectionMatrix);
        }
        private void BindLightDataToShaders(ShaderProgram shader)
        {
            GL.Uniform3(shader.GetUniform("lightColor"), lightColor);
            GL.Uniform3(shader.GetUniform("lightPosition"), lightPosition);
            GL.Uniform1(shader.GetUniform("ambientCoefficient"), ambientCoefficient);
        }

        private void BindMeshMaterialDataToShaders(Mesh m, ShaderProgram shader)
        {
            GL.Uniform1(shader.GetUniform("materialSpecExponent"), m.materialSpecExponent);
            GL.Uniform3(shader.GetUniform("specularColor"), m.materialDiffuseSpecularColor);
            GL.Uniform4(shader.GetUniform("surfaceColor"), m.surfaceColor);
            GL.Uniform1(shader.GetUniform("isPlate"), m.isPlate);
        }

        public static void AddMeshToDraw(Mesh m, MyShaderType shaderType)
        {
            meshesToDraw[(int)shaderType].Add(m);
        }

        public static void AddAnimatedObject(ObjectsToDraw otd)
        {
            objectsToDraw.Add(otd);
        }

        public static void RemoveMeshToDraw(Mesh m)
        {
            for (int i = 0; i < (int)Enum.GetValues(typeof(MyShaderType)).Length; i++)
                meshesToDraw[i].Remove(m);
        }

        public static void RemoveAnimatedObject(ObjectsToDraw otd)
        {
            objectsToDraw.Remove(otd);
        }
    }
}
