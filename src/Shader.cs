using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GL = OpenTK.Graphics.OpenGL4.GL;

namespace OpenGLEngine
{
    public class Shader : IDisposable
    {
        private int rendererID;
        private bool disposedValue;

        public Shader(string vertexPath, string fragmentPath)
        {
            var vertexShader = Compile(ShaderType.VertexShader, vertexPath);
            var fragmentShader = Compile(ShaderType.FragmentShader, fragmentPath);
            Create(vertexShader, fragmentShader);
        }

        public void Bind()
        {
            GL.UseProgram(rendererID);
        }

        public void Unbind()
        {
            GL.UseProgram(0);
        }

        public void Dispose()
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(rendererID);
                disposedValue = true;
            }
            GC.SuppressFinalize(this);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s.
        // Dynamically, we can omit the layout(location=X) lines in the vertex shader,
        // and use this in VertexAttribPointer instead of the hardcoded values.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(rendererID, attribName);
        }

        public void SetInt(string name, int value)
        {
            var location = GL.GetUniformLocation(rendererID, name);
            GL.Uniform1(location, value);
        }

        public void SetMatrix4(string name, Matrix4 matrix)
        {
            var location = GL.GetUniformLocation(rendererID, name);
            // Transpose determines whether or not the matrices should be transposed.
            // Since OpenTK uses row-major, whereas GLSL typically uses column-major,
            // we will almost always want to use true here.
            GL.UniformMatrix4(location, true, ref matrix);
        }

        private int Compile(ShaderType type,string filePath)
        {
            var shaderSource = File.ReadAllText(filePath);
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, shaderSource);
            
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code == (int) All.True) 
                return shader;
            
            // Compile failed, get information about the error.
            var infoLog = GL.GetShaderInfoLog(shader);
            GL.DeleteShader(shader);
            throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
        }

        private void Create(int vertexShader, int fragmentShader)
        {
            rendererID = GL.CreateProgram();

            GL.AttachShader(rendererID, vertexShader);
            GL.AttachShader(rendererID, fragmentShader);

            GL.LinkProgram(rendererID);
            GL.GetProgram(rendererID, GetProgramParameterName.LinkStatus, out var success);
            if (success == 0)
            {
                var infoLog = GL.GetShaderInfoLog(rendererID);
                Console.WriteLine(infoLog);
            }
            
            GL.DetachShader(rendererID, vertexShader);
            GL.DetachShader(rendererID, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
        }
    }
}
