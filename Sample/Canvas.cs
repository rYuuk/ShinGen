using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Windowing;

namespace ShinGen
{
    public class Canvas : ICanvas
    {
        private struct WindowData
        {
            public string Name;
            public Vector2 Pos;
            public Vector2 Size;
        }

        private readonly Stopwatch stopwatch;

        public Action<string> DownloadButtonClicked = null!;

        private string url = "https://api.readyplayer.me/v1/avatars/63c5900d295455f2dd017fd2.glb";
        private string downloadButton = "Download";
        private string log = string.Empty;

        private float ElapsedSeconds => stopwatch.ElapsedMilliseconds / 1000f;

        public IWindow Window { get; set; } = null!;
        private string lastLog = null!;

        private readonly List<ModelRendererComponent> renderedModels;

        public Canvas()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            renderedModels = new List<ModelRendererComponent>();
        }

        public void Render(double deltaTime)
        {
            RenderAvatarLoaderWindow();
            RenderDebugWindow(deltaTime);
            RenderLogWindow();
        }

        public void AddLog(string logString)
        {
            log += logString + "\n";
        }

        public void StartProgressLog()
        {
            lastLog = log;
        }

        public void AddProgressLog(string logString, float progress)
        {
            log = lastLog + $"{logString} {progress * 100:F2}%%\n";
        }

        public void StopProgress()
        {
            downloadButton = "Download";
        }

        public void RestartStopwatch()
        {
            stopwatch.Restart();
        }

        public void AddTimedLog(string logString)
        {
            log += $"{logString} : [{ElapsedSeconds:F2}s]\n";
        }

        public void LogModel(ModelRendererComponent rendererComponent)
        {
            renderedModels.Add(rendererComponent);
        }

        public void RemoveModelLogging(ModelRendererComponent rendererComponent)
        {
            renderedModels.Remove(rendererComponent);
        }


        private void CreateWindow(WindowData windowData, Action onWindow)
        {
            ImGui.Begin(windowData.Name, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            ImGui.SetWindowPos(windowData.Pos);
            ImGui.SetWindowSize(windowData.Size);
            ImGui.SetWindowFontScale(1.6f);
            onWindow.Invoke();
            ImGui.End();
        }

        private void RenderAvatarLoaderWindow()
        {
            var downloadWindowData = new WindowData
            {
                Name = "Avatar Loader",
                Pos = new Vector2(20, 20),
                Size = new Vector2(900, 80)
            };

            CreateWindow(downloadWindowData, () =>
            {
                ImGui.PushItemWidth(760f);
                ImGui.InputText(string.Empty, ref url, 100);
                ImGui.SameLine();
                if (ImGui.Button(downloadButton))
                {
                    downloadButton = downloadButton == "Download" ? "Cancel" : "Download";
                    DownloadButtonClicked(url);
                }
            });
        }

        private void RenderDebugWindow(double deltaTime)
        {
            var debugWindowData = new WindowData
            {
                Name = "Debug",
                Pos = new Vector2(Window.Size.X - 520, 20),
                Size = new Vector2(500, 300)
            };

            CreateWindow(debugWindowData, () =>
            {
                ImGui.Text("Loaded model data.");
                ImGui.SameLine(debugWindowData.Size.X - 120);
                ImGui.Text($"FPS: {1 / deltaTime:F0}");
                ImGui.Separator();

                for (var j = 0; j < renderedModels.Count; j++)
                {
                    var renderer = renderedModels[j];
                    ImGui.Text($"Model: {j}\nMeshCount: {renderer.Model.Meshes.Count}");

                    foreach (var mesh in renderer.Model.Meshes)
                    {
                        ImGui.Separator();
                        ImGui.Text($"Name: {mesh.Name}\n " +
                                   $"Vertices: {mesh.Vertices.Length}\n " +
                                   $"Indices: {mesh.Indices.Length}\n " +
                                   $"Normals: {mesh.Normals.Length}\n " +
                                   $"TexCoords: {mesh.TexCoords.Length}\n " +
                                   $"Textures: {mesh.Textures.Length}");
                    }
                    ImGui.Separator();
                }
            });
        }

        private void RenderLogWindow()
        {
            var logWindowData = new WindowData
            {
                Name = "Log",
                Pos = new Vector2(20, Window.Size.Y - 320),
                Size = new Vector2(900, 300)
            };

            CreateWindow(logWindowData, () =>
            {
                ImGui.Text(log);
            });
        }


    }
}
