using System.Numerics;
using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public class Shader : IDisposable
    {
        private readonly GL gl;
        private readonly string vertexPath;
        private readonly string fragmentPath;

        private uint handle;
        private bool disposedValue;

        private Dictionary<string, int> uniformLocations;

        public Shader(GL gl, string vertexPath, string fragmentPath)
        {
            this.gl = gl;
            this.vertexPath = vertexPath;
            this.fragmentPath = fragmentPath;
            uniformLocations = new Dictionary<string, int>();
        }

        public void Load()
        {
            var vertex = LoadShader(ShaderType.VertexShader, vertexPath);
            var fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
            handle = gl.CreateProgram();
            gl.AttachShader(handle, vertex);
            gl.AttachShader(handle, fragment);
            gl.LinkProgram(handle);
            gl.GetProgram(handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {gl.GetProgramInfoLog(handle)}");
            }
            gl.DetachShader(handle, vertex);
            gl.DetachShader(handle, fragment);
            gl.DeleteShader(vertex);
            gl.DeleteShader(fragment);
            CacheUniforms();
        }

        public void Bind() =>
            gl.UseProgram(handle);

        public void Unbind() =>
            gl.UseProgram(0);

        public void Dispose()
        {
            if (!disposedValue)
            {
                gl.DeleteProgram(handle);
                disposedValue = true;
            }
            GC.SuppressFinalize(this);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s.
        // Dynamically, we can omit the layout(location=X) lines in the vertex shader,
        // and use this in VertexAttribPointer instead of the hardcoded values.
        public int GetAttribLocation(string attribName) =>
            gl.GetAttribLocation(handle, attribName);

        public void SetInt(string name, int value) =>
            gl.Uniform1(uniformLocations[name], (uint) value);

        public void SetFloat(string name, float value) =>
            gl.Uniform1(uniformLocations[name], value);

        public void SetVector3(string name, Vector3 value) =>
            gl.Uniform3(uniformLocations[name], value);

        public unsafe void SetMatrix4(string name, Matrix4x4 matrix)
        {
            // Transpose determines whether or not the matrices should be transposed.
            // Since OpenTK uses row-major, whereas GLSL typically uses column-major,
            // we will almost always want to use true here.
            gl.UniformMatrix4(uniformLocations[name], 1, true, (float*) &matrix);
        }

        private uint LoadShader(ShaderType type, string path)
        {
            var src = File.ReadAllText(path);
            var shaderHandle = gl.CreateShader(type);
            gl.ShaderSource(shaderHandle, src);
            gl.CompileShader(shaderHandle);
            var infoLog = gl.GetShaderInfoLog(shaderHandle);
            if (!string.IsNullOrWhiteSpace(infoLog))
                throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");

            return shaderHandle;
        }

        private void CacheUniforms()
        {
            gl.GetProgram(handle, GLEnum.ActiveUniforms, out var numberOfUniforms);
            uniformLocations = new Dictionary<string, int>();
            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = gl.GetActiveUniform(handle, (uint) i, out _, out _);
                var location = gl.GetUniformLocation(handle, key);
                uniformLocations.Add(key, location);
            }
        }
    }
}
