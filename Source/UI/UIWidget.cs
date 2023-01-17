using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace OpenGLEngine.UI
{
    public class UIWidget
    {
        public Action<string> DownloadButtonClicked = null!;

        private readonly ImGuiController imgui;
        private string url = "https://api.readyplayer.me/v1/avatars/63c5900d295455f2dd017fd2.glb";
        private string log = string.Empty;

        public UIWidget(GL gl, IView window, IInputContext inputContext)
        {
            imgui = new ImGuiController(gl, window, inputContext);
        }

        public void RenderDownloadUI()
        {
            ImGui.Text("Enter a URL for a GLB to render");
            ImGui.NewLine();
            ImGui.InputText(string.Empty, ref url, 100);
            ImGui.SameLine();
            if (ImGui.Button("Download"))
            {
                DownloadButtonClicked?.Invoke(url);
            }
        }

        public void RenderDebugUI(double deltaTime, Model avatar)
        {
            ImGui.NewLine();
            ImGui.Separator();
            if (ImGui.CollapsingHeader("Debug"))
            {
                ImGui.Text($"MeshCount: {avatar.Meshes.Count}");
                ImGui.SameLine(900 - 100);
                ImGui.Text($"FPS: {1 / deltaTime:F0}");

                for (var i = 0; i < avatar.Meshes.Count; i++)
                {
                    ImGui.Separator();
                    var mesh = avatar.Meshes[i];
                    ImGui.Text($"Mesh{i}\n" +
                               $" Name: {mesh.Name}\n" +
                               $" Vertices: {mesh.Vertices.Length}\n" +
                               $" Indices: {mesh.Indices.Length}\n" +
                               $" Normals: {mesh.Normals.Length}\n" +
                               $" TexCoords: {mesh.TexCoords.Length}\n" +
                               $" Textures: {mesh.Textures.Length}");
                }
                ImGui.Separator();
            }


        }

        public void AddLog(string logString)
        {
            log += logString + "\n";
        }

        public void RenderLogUI()
        {
            if (ImGui.CollapsingHeader("Logs"))
            {
                ImGui.Text(log);
            }
        }

        public void Begin(double deltaTime)
        {
            imgui.Update((float) deltaTime);
            ImGui.Begin("Avatar Loader");
            ImGui.SetWindowSize(new Vector2(900, 500));
            ImGui.PushItemWidth(760f);
            ImGui.SetWindowFontScale(1.5f);
        }

        public void End()
        {
            ImGui.End();
            imgui.Render();
        }
    }
}
