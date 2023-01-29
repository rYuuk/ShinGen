namespace ShinGen
{
    public class ModelDownloader
    {
        private const string DOWNLOADED_AVATAR_FILE_PATH = "Resources/DownloadedAvatar.glb";

        private enum AvatarDownloadStatus
        {
            None,
            InProgress,
            Downloaded
        }

        public event Action<float>? InProgress;
        public event Action<string>? Completed;

        private AvatarDownloadStatus avatarDownloadStatus;
        private CancellationTokenSource ctx = null!;

        public async void DownloadAsync(string path)
        {
            if (avatarDownloadStatus == AvatarDownloadStatus.InProgress)
            {
                avatarDownloadStatus = AvatarDownloadStatus.None;
                ctx.Cancel();
                return;
            }
            ctx = new CancellationTokenSource();
            avatarDownloadStatus = AvatarDownloadStatus.InProgress;

            var progress = new Progress<float>(progress =>
            {
                InProgress?.Invoke(progress);
            });
            await WebRequestDispatcher.DownloadRequest(path, DOWNLOADED_AVATAR_FILE_PATH, progress, ctx.Token);
            if (ctx.IsCancellationRequested)
            {
                return;
            }
            Completed?.Invoke(DOWNLOADED_AVATAR_FILE_PATH);
            avatarDownloadStatus = AvatarDownloadStatus.Downloaded;
        }
    }
}
