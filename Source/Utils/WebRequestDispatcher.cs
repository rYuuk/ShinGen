namespace OpenGLEngine
{
    public static class WebRequestDispatcher
    {
        public static async Task DownloadRequest(string downloadUrl, string filePath, IProgress<float>? progress,
            CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);

            await using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await client.DownloadAsync(downloadUrl, file, progress, cancellationToken);
        }
    }
}
