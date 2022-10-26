﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GL = OpenTK.Graphics.OpenGL4.GL;

namespace OpenGLEngine
{
    public class Shader : IDisposable
    {
        public int Handle;
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

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(Handle);
                Console.WriteLine(infoLog);
            }

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
        }

        ~Shader() =>
            GL.DeleteProgram(Handle);

        public void Use() =>
            GL.UseProgram(Handle);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s.
        // Dynamically, we can omit the layout(location=X) lines in the vertex shader,
        // and use this in VertexAttribPointer instead of the hardcoded values.
        public int GetAttribLocation(string attribName) =>
            GL.GetAttribLocation(Handle, attribName);

        public void SetInt(string name, int value)
        {
            var location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }

        public void SetMatrix4(string name, Matrix4 matrix)
        {
            var location = GL.GetUniformLocation(Handle, name);
            // Transpose determines whether or not the matrices should be transposed.
            // Since OpenTK uses row-major, whereas GLSL typically uses column-major,
            // we will almost always want to use true here.
            GL.UniformMatrix4(location, true, ref matrix);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);
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
