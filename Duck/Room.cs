using System;

namespace Duck
{
    class Room : ObjectsToDraw
    {
        public Room(MyShaderType shaderType) : base(shaderType)
        {
        }

        public override void DoAnimation(float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}
