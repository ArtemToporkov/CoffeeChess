namespace CoffeeChess.Application.Songs.Sevices;

public interface IMediaProviderService
{
    public FileStream OpenSongAudioRead(string relativePath);
    
    public FileStream OpenSongCoverRead(string relativePath);
}