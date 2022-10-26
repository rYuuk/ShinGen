using OpenTK.Mathematics;

namespace OpenGLEngine
{
    // TODO could have also managed the player input inside the camera class,
    // TODO a lot of the properties could have been made into functions.
    public class Camera
    {
        // These vectors are directions pointing outwards from the camera to define how it is rotated.
        private Vector3 front = -Vector3.UnitZ;

        private Vector3 up = Vector3.UnitY;

        private Vector3 right = Vector3.UnitX;

        // Rotation around the X axis (radians)
        private float pitch;

        // Rotation around the Y axis (radians),
        // Without this, it would be started rotated 90 degrees right.
        private float yaw = -MathHelper.PiOver2;

        // The field of view of the camera (radians)
        private float fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio, float speed, float sensitivity)
        {
            Position = position;
            AspectRatio = aspectRatio;
            Speed = speed;
            Sensitivity = sensitivity;
        }

        public readonly float Speed;
        public readonly float Sensitivity;

        // The position of the camera
        public Vector3 Position { get; set; }

        // This is simply the aspect ratio of the viewport, used for the projection matrix.
        public float AspectRatio { private get; set; }

        public Vector3 Front => front;

        public Vector3 Up => up;

        public Vector3 Right => right;


        // Convert from degrees to radians as soon as the property is set to improve performance.
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set
            {
                // Clamp the pitch value between -89 and 89 to prevent the camera from going upside down.
                var angle = MathHelper.Clamp(value, -89f, 89f);
                pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // Convert from degrees to radians as soon as the property is set to improve performance.
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(yaw);
            set
            {
                yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // The field of view (FOV) is the vertical angle of the camera view.
        // Convert from degrees to radians as soon as the property is set to improve performance.
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                fov = MathHelper.DegreesToRadians(angle);
            }
        }

        // Get the view matrix using the LookAt function
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + front, up);
        }

        // Get the projection matrix using the CreatePerspectiveFieldOfView function.
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, AspectRatio, 0.01f, 100f);
        }

        // This updates the direction vertices.
        private void UpdateVectors()
        {
            // The front matrix is calculated using some basic trigonometry.
            front.X = MathF.Cos(pitch) * MathF.Cos(yaw);
            front.Y = MathF.Sin(pitch);
            front.Z = MathF.Cos(pitch) * MathF.Sin(yaw);

            // The vectors should be normalized, as otherwise it would get some funky results.
            front = Vector3.Normalize(front);

            // Calculate both the right and the up vector using cross product.
            // Note that this is calculating the right from the global up; this behaviour might
            // not be needed for all cameras so keep, like if we do not want a FPS camera.
            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }
    }
}
