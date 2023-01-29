using System.Numerics;

namespace ShinGen
{
    public sealed class Transform
    {
        public Vector3  Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;


        public Matrix4x4 ModelMatrix => Matrix4x4.CreateScale(Scale)
                                        * Matrix4x4.CreateFromYawPitchRoll(
                                            MathHelper.DegreesToRadians(Rotation.Y),
                                            MathHelper.DegreesToRadians(Rotation.X),
                                            MathHelper.DegreesToRadians(Rotation.Z))
                                        * Matrix4x4.CreateTranslation(Position);
    }
}
