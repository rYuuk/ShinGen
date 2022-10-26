using OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace OpenGLEngine
{
    public class Shader : IDisposable
    {
        private readonly int program;
        private bool disposedValue;

        public Shader(string vertexPath, string fragmentPath)
        {
            var vertexShaderSource = File.ReadAllText(vertexPath);
            var fragmentShaderSource = File.ReadAllText(fragmentPath);

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);

            CompileShader(vertexShader);
            CompileShader(fragmentShader);

            program = GL.CreateProgram();

            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(program);
                Console.WriteLine(infoLog);
            }

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

        }

        ~Shader()
        {
            GL.DeleteProgram(program);
        }

        public void Use()
        {
            GL.UseProgram(program);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(program);

                disposedValue = true;
            }
        }

        private void CompileShader(int shaderHandle)
        {
            GL.CompileShader(shaderHandle);
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shaderHandle);
                Console.WriteLine(infoLog);
            }
        }
    }
}
