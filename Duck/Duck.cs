using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duck
{
    class Duck : ObjectsToDraw
    {
        Water water;

        public Duck(MyShaderType shaderType, Water water) : base(shaderType)
        {
            this.water = water;
            MeshLoader ml = new MeshLoader();
            meshes = new Mesh[1] { ml.LoadMesh("duck") };
            meshes[0].ModelMatrix = Matrix4.CreateScale(0.001f);
        }

        public override void DoAnimation(float deltaTime)
        {
            //throw new NotImplementedException();
        }
    }
}
