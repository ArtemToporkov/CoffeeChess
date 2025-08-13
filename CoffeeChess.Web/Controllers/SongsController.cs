using CoffeeChess.Application.Songs.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeChess.Web.Controllers;

public class SongsController(IMediator mediator) : Controller
{
    [HttpGet("/Songs/Audio/{songId}")]
    public async Task<IActionResult> GetSongAudio(string songId)
    {
        var audio = await mediator.Send(new GetSongAudioQuery(songId));
        return new FileStreamResult(audio, "audio/mp3");
    }
    
    [HttpGet("/Songs/Cover/{songId}")]
    public async Task<IActionResult> GetSongCover(string songId)
    {
        var cover = await mediator.Send(new GetSongCoverQuery(songId));
        return new FileStreamResult(cover, "image/jpeg");
    }

    [HttpGet("/Songs/All")]
    public async Task<IActionResult> GetAllSongs()
    {
        var command = new GetAllSongsQuery();
        var songs = await mediator.Send(command);
        return Json(songs);
    }
}