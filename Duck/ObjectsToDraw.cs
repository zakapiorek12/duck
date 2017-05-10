using OpenTK;

namespace Duck
{
    abstract public class ObjectsToDraw
    {
        protected Mesh[] meshes;
        protected MyShaderType shaderType;
        protected Vector4 surfaceColor = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);

        public ObjectsToDraw(MyShaderType shaderType)
        {
            this.shaderType = shaderType;
        }

        abstract public void DoAnimation(float deltaTime);
        
        public void AddOnScene()
        {
            foreach (Mesh m in meshes)
                GLRenderer.AddMeshToDraw(m, shaderType);
            GLRenderer.AddAnimatedObject(this);
        }
    }
}
