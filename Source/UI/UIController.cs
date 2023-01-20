using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace OpenGLEngine
{
    public class UIController
    {
        private struct WindowData
        {
            public string Name;
            public Vector2 Pos;
            public Vector2 Size;
        }

        public Action<string> DownloadButtonClicked = null!;

        private readonly ImGuiController imgui;
        private readonly IView window;

        private string url = "https://api.readyplayer.me/v1/avatars/63c5900d295455f2dd017fd2.glb";
        private string log = string.Empty;


        public UIController(GL gl, IView window, IInputContext inputContext)
        {
            this.window = window;
            imgui = new ImGuiController(gl, window, inputContext);
        }

        public void Begin(double deltaTime)
        {
            imgui.Update((float) deltaTime);
        }

        public void End()
        {
            imgui.Render();
        }

        private void CreateWindow(WindowData windowData, Action onWindow)
        {
            ImGui.Begin(windowData.Name, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            ImGui.SetWindowPos(windowData.Pos);
            ImGui.SetWindowSize(windowData.Size);
            ImGui.SetWindowFontScale(1.6f);
            onWindow?.Invoke();
            ImGui.End();
        }

        public void RenderAvatarLoaderWindow()
        {
            var downloadWindowData = new WindowData
            {
                Name = "Avatar Loader",
                Pos = new Vector2(20, 20),
                Size = new Vector2(900, 80),
            };

            CreateWindow(downloadWindowData, () =>
            {
                ImGui.PushItemWidth(760f);
                ImGui.InputText(string.Empty, ref url, 100);
                ImGui.SameLine();
                if (ImGui.Button("Load"))
                {
                    DownloadButtonClicked?.Invoke(url);
                }
            });
            ImGui.End();
        }

        public void RenderDebugWindow(double deltaTime, params Model[] models)
        {
            var debugWindowData = new WindowData
            {
                Name = "Debug",
                Pos = new Vector2(window.Size.X - 520, 20),
                Size = new Vector2(500, 300),
            };

            CreateWindow(debugWindowData, () =>
            {
                foreach (var model in models)
                {
                    ImGui.Text($"MeshCount: {model.Meshes.Count}");
                    ImGui.SameLine(debugWindowData.Size.X - 120);
                    ImGui.Text($"FPS: {1 / deltaTime:F0}");

                    for (var i = 0; i < model.Meshes.Count; i++)
                    {
                        ImGui.Separator();
                        var mesh = model.Meshes[i];
                        ImGui.Text($"Mesh{i}\n" +
                                   $" Name: {mesh.Name}\n" +
                                   $" Vertices: {mesh.Vertices.Length}\n" +
                                   $" Indices: {mesh.Indices.Length}\n" +
                                   $" Normals: {mesh.Normals.Length}\n" +
                                   $" TexCoords: {mesh.TexCoords.Length}\n" +
                                   $" Textures: {mesh.Textures.Length}");
                    }
                }
            });
        }

        public void RenderLogWindow()
        {
            var logWindowData = new WindowData
            {
                Name = "Log",
                Pos = new Vector2(20, window.Size.Y - 320),
                Size = new Vector2(800, 300),
            };

            CreateWindow(logWindowData, () =>
            {
                ImGui.Text(log);
            });
        }

        public void AddLog(string logString)
        {
            log += logString + "\n";
        }

        public void ProgressLog(string logString, float progress)
        {
            log = $"{logString}: {progress * 100:F2}%" + "\n";
        }
    }
}
