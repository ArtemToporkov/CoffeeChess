using MediatR;

namespace CoffeeChess.Application.Chats.Commands;

public record SendChatMessageCommand(string GameId, string Username, string Message) : IRequest;