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
        private int cubeTextureNr = 0;

        private Dictionary<MyShaderType, ShaderProgram> shaders = new Dictionary<MyShaderType, ShaderProgram>();

        private float ambientCoefficient = 1.0f;
        private Vector3 lightColor = new Vector3(0.9f, 0.8f, 0.8f);
        private Vector3 lightPosition = new Vector3(0.0f, 1.0f, 0.0f);

        private static List<Mesh>[] meshesToDraw;
        private static List<ObjectsToDraw> objectsToDraw;

        MeshLoader meshLoader = new MeshLoader();

        private int cubeTextureID;
        private int duckTextureID;

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
            CreateProjectionMatrix(viewPortWidth, viewportHeight);
            CreateScene();
            CreateCubeTexture(ref cubeTextureID);
            LoadTexture("ducktex.jpg", ref duckTextureID);

            GL.ClearColor(Color.Black);
        }

        private void LoadShaders()
        {
            ShaderProgram shaderProgram = new ShaderProgram("shaders/WaterVS.vert",
                "shaders/WaterPS.vert", true);
            shaders.Add(MyShaderType.WATER, shaderProgram);

            shaderProgram = new ShaderProgram("shaders/DuckVS.vert",
                "shaders/DuckPS.vert", true);
            shaders.Add(MyShaderType.DUCK, shaderProgram);
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

            MeshLoader ml = new MeshLoader();
            Mesh cube = ml.GetCube();
            AddMeshToDraw(cube, MyShaderType.CUBE);

            Duck duck = new Duck(MyShaderType.DUCK, water);
            duck.AddOnScene();
        }

        private void LoadTexture(string name, ref int id)
        {
            Bitmap texture = new Bitmap(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "texture", name));

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

        public void CreateCubeTexture(ref int id)
        {
            GL.GenTextures(1, out id);
            GL.ActiveTexture(TextureUnit.Texture0 + cubeTextureNr);
            GL.BindTexture(TextureTarget.TextureCubeMap, id);

            string root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string[] paths = new string[] { "posx", "negx", "posy", "negy", "posz", "negz" };

            foreach (int i in new int[] { 0, 1, 2, 3, 4, 5 })
            {
                Bitmap texture = new Bitmap(Path.Combine(root, "texture", paths[i] + ".jpg"));
                BitmapData data = texture.LockBits(new System.Drawing.Rectangle(0, 0, texture.Width, texture.Height),
                     ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, texture.Width, texture.Height,
                    0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                texture.UnlockBits(data);
                texture.Dispose();
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);


            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            //GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

        }

        public void DoScene(float deltaTime, Camera camera)
        {
            foreach (ObjectsToDraw otd in objectsToDraw)
                otd.DoAnimation(deltaTime);
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            RenderWater(camera);
            RenderCube(camera);
            RenderDuck(camera);

            //tutaj dodac renderowanie przez nowe shadery
        }

        private void RenderWater(Camera camera)
        {
            ShaderProgram activeShader = shaders[MyShaderType.WATER];
            GL.UseProgram(activeShader.ProgramID);
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.Uniform1(activeShader.GetUniform("cubeSampler"), cubeTextureNr);
            GL.ActiveTexture(TextureUnit.Texture0 + cubeTextureNr);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeTextureID);

            BindCameraAndProjectionToShaders(camera, activeShader);
            BindLightDataToShaders(activeShader);

            foreach (Mesh m in meshesToDraw[(int)MyShaderType.WATER])
                DrawMesh(m, activeShader, PrimitiveType.Quads);

            GL.Flush();
        }

        private void RenderCube(Camera camera)
        {
            ShaderProgram activeShader = shaders[MyShaderType.WATER];
            GL.UseProgram(activeShader.ProgramID);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Cw);

            GL.Enable(EnableCap.TextureCubeMap);

            GL.Uniform1(activeShader.GetUniform("cubeSampler"), cubeTextureNr);
            GL.ActiveTexture(TextureUnit.Texture0 + cubeTextureNr);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeTextureID);

            BindCameraAndProjectionToShaders(camera, activeShader);
            BindLightDataToShaders(activeShader);

            foreach (Mesh m in meshesToDraw[(int)MyShaderType.CUBE])
                DrawMesh(m, activeShader, PrimitiveType.Quads);

            GL.Flush();
        }

        private void RenderDuck(Camera camera)
        {
            ShaderProgram activeShader = shaders[MyShaderType.DUCK];
            GL.UseProgram(activeShader.ProgramID);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            GL.Enable(EnableCap.Texture2D);

            //GL.Uniform1(activeShader.GetUniform("cubeSampler"), cubeTextureNr);
            //GL.ActiveTexture(TextureUnit.Texture0 + cubeTextureNr);
            //GL.BindTexture(TextureTarget.TextureCubeMap, cubeTextureID);

            BindCameraAndProjectionToShaders(camera, activeShader);
            BindLightDataToShaders(activeShader);

            foreach (Mesh m in meshesToDraw[(int)MyShaderType.DUCK])
                DrawMesh(m, activeShader, PrimitiveType.Triangles);

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
            GL.Uniform1(shader.GetUniform("isCube"), m.isCube);
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
