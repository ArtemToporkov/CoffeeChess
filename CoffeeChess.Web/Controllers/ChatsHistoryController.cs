using System.Security.Authentication;
using System.Security.Claims;
using CoffeeChess.Application.Chats.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeChess.Web.Controllers;

public class ChatsHistoryController(IMediator mediator) : Controller
{
    [HttpGet("/Chats/{gameId}")]
    public async Task<IActionResult> GetChatHistory(string gameId)
    {
        try
        {
            var chatHistory = await mediator.Send(new GetChatHistoryQuery(gameId));
            return Json(chatHistory);
        }
        catch (Exception ex)
        {
            return NotFound($"Something went wrong while receiving chat history: {ex.Message}");
        }
    }
}