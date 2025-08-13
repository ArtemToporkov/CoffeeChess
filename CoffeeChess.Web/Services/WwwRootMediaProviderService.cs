using CoffeeChess.Application.Songs.Sevices;

namespace CoffeeChess.Web.Services;

public class WwwRootMediaProviderService(IWebHostEnvironment webHostEnvironment) : IMediaProviderService
{
    private readonly string _webRootPath = webHostEnvironment.WebRootPath;

    public FileStream OpenSongAudioRead(string relativePath) => OpenRead(relativePath);

    public FileStream OpenSongCoverRead(string relativePath) => OpenRead(relativePath);

    private FileStream OpenRead(string relativePath)
    {
        var cleanRelative = relativePath.TrimStart('/', '\\');
        var fullPath = Path.Combine(_webRootPath, cleanRelative);
        var normalizedPath = Path.GetFullPath(fullPath);
        var normalizedWebRootPath = Path.GetFullPath(_webRootPath);

        if (!normalizedPath.StartsWith(normalizedWebRootPath, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Access to the path is denied.");

        if (!File.Exists(normalizedPath))
            throw new FileNotFoundException(
                $"The requested file {relativePath} was not found.", normalizedPath);
        
        return File.OpenRead(normalizedPath);
    }
}