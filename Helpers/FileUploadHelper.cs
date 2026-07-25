namespace test.Helpers;

public static class FileUploadHelper
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedVideoExtensions = [".mp4", ".mov", ".avi", ".mkv"];

    public static async Task<string?> SaveFileAsync(IFormFile? file, string subfolder, IWebHostEnvironment env, string baseUrl = "")
    {
        if (file == null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uploadsRoot = Path.Combine(env.ContentRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        var relativePath = $"/uploads/{subfolder}/{fileName}";
        return string.IsNullOrWhiteSpace(baseUrl)
            ? relativePath
            : $"{baseUrl.TrimEnd('/')}{relativePath}";
    }

    public static async Task<List<string>> SaveFilesAsync(IList<IFormFile>? files, string subfolder, IWebHostEnvironment env, string baseUrl = "")
    {
        var urls = new List<string>();
        if (files == null) return urls;
        foreach (var file in files)
        {
            var url = await SaveFileAsync(file, subfolder, env, baseUrl);
            if (url != null) urls.Add(url);
        }
        return urls;
    }

    public static bool IsImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedImageExtensions.Contains(ext);
    }

    public static bool IsVideo(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedVideoExtensions.Contains(ext);
    }
}
