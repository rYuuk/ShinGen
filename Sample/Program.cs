using System.Numerics;
using ShinGen;

using var engine = new Engine();
var canvas = engine.CreateUICanvas<Canvas>();
canvas.AddLog("Load complete");

var room = new GameObject();
var roomRendererComponent = room.AddComponent<ModelRendererComponent>();
roomRendererComponent.SetPath("Resources/EpicRoom.glb");
room.Transform.Scale = Vector3.One * 0.2f;
room.Transform.Position = new Vector3(0.0f, 0, 0f);
room.Transform.Rotation = new Vector3(0, -45, 0);
engine.AddGameObject(room);

var avatar = CreateAvatar("Resources/Avatar/MultiMesh/Avatar.glb");
engine.AddGameObject(avatar);

canvas.LogModel(roomRendererComponent);
canvas.LogModel(avatar.GetComponent<ModelRendererComponent>()!);

var downloader = new ModelDownloader();
canvas.DownloadButtonClicked += path =>
{
    canvas.StartProgressLog();
    downloader.DownloadAsync(path);
};

downloader.InProgress += progress => canvas.AddProgressLog("Downloading...", progress);
downloader.Completed += path =>
{
    canvas.AddTimedLog("Downloaded at - " + path);
    canvas.StopProgress();

    engine.DestroyGameObject(avatar);
    canvas.RemoveModelLogging(avatar.GetComponent<ModelRendererComponent>()!);
    canvas.LogModel(roomRendererComponent);

    avatar = CreateAvatar(path);
    engine.AddGameObject(avatar);

    canvas.LogModel(avatar.GetComponent<ModelRendererComponent>()!);

};

GameObject CreateAvatar(string path)
{
    var newAvatar = new GameObject();
    var rendererComponent = newAvatar.AddComponent<ModelRendererComponent>();
    rendererComponent.SetPath(path);
    newAvatar.Transform.Position = new Vector3(0.6f, 0.1f, 0.2f);
    newAvatar.Transform.Scale = Vector3.One * 0.5f;
    newAvatar.AddComponent<RotateComponent>();
    return newAvatar;
}

engine.Start();
