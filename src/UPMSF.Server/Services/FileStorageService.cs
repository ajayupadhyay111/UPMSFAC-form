namespace UPMSF.Server.Services;

/// <summary>Stores uploaded files on the local filesystem (path saved in DB).
/// Root is configurable so the same code works on Azure App Service (use a mounted
/// share or %HOME%\data) or any host.</summary>
public class FileStorageService
{
    private readonly string _root;

    public FileStorageService(IConfiguration config, IWebHostEnvironment env)
    {
        _root = config["FileStorage:Root"]
                ?? Path.Combine(env.ContentRootPath, "App_Data", "uploads");
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(string applicationNumber, string docKey, string extension, Stream content, CancellationToken ct)
    {
        var dir = Path.Combine(_root, applicationNumber);
        Directory.CreateDirectory(dir);
        var fileName = $"{docKey}.{extension}";
        var full = Path.Combine(dir, fileName);
        await using var fs = new FileStream(full, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs, ct);
        return full;
    }

    public void Delete(string storedPath)
    {
        if (!string.IsNullOrEmpty(storedPath) && File.Exists(storedPath))
            File.Delete(storedPath);
    }
}
