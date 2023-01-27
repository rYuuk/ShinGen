namespace ShinGen
{
    public static class MathHelper
    {
        public static float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }

        public static float RadiansToDegrees(float radians)
        {
            return 180f / MathF.PI * radians;
        }

        public static float PiOver2 => MathF.PI / 2;
    }
}
