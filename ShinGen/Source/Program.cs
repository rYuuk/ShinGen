using System.Numerics;
using ShinGen;

using var engine = new Engine();

var room = new GameObject();
room.AddComponent(new ModelRendererComponent("Resources/EpicRoom.glb"));
room.Transform.Scale = Vector3.One * 0.2f;
room.Transform.Position = new Vector3(0.0f, 0, 0f);
room.Transform.Rotation = new Vector3(0, -45, 0);
engine.AddGameObject(room);

var avatar = new GameObject();
avatar.AddComponent(new ModelRendererComponent("Resources/Avatar/MultiMesh/Avatar.glb"));
avatar.Transform.Position = new Vector3(0.6f, 0.1f, 0.2f);
avatar.Transform.Scale = Vector3.One * 0.5f;
avatar.AddComponent(new RotateComponent());
engine.AddGameObject(avatar);


var canvas = engine.CreateUI<Canvas>();
canvas.RenderedModels.Add(avatar);
canvas.RenderedModels.Add(room);

var downloader = new ModelDownloader();
canvas.DownloadButtonClicked += downloader.DownloadAsync;

downloader.InProgress += progress => canvas.AddProgressLog("Downloading...", progress);
downloader.Completed += path =>
{
    canvas.AddTimedLog("Downloaded at - " + path);
    engine.DestroyGameObject(avatar);
    canvas.RenderedModels.Remove(avatar);

    avatar = new GameObject();
    avatar.AddComponent(new ModelRendererComponent(path));
    avatar.Transform.Position = new Vector3(0.6f, 0.1f, 0.2f);
    avatar.Transform.Scale = Vector3.One * 0.5f;
    avatar.AddComponent(new RotateComponent());
    engine.AddGameObject(avatar);

    canvas.RenderedModels.Add(avatar);
};

engine.Start();
