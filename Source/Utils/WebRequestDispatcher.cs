namespace OpenGLEngine
{
    public static class WebRequestDispatcher
    {
        public static async Task DownloadRequest(string downloadUrl, string filePath, IProgress<float>? progress,
            CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);

            try
            {
                await using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                cancellationToken.Register(file.Close);
                await client.DownloadAsync(downloadUrl, file, progress, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
