using MediatR;

namespace CoffeeChess.Application.Commands;

public record SendChatMessageCommand(string GameId, string Username, string Message) : IRequest;