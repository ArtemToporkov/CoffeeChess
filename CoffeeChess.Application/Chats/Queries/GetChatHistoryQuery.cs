using CoffeeChess.Application.Chats.ReadModels;
using MediatR;

namespace CoffeeChess.Application.Chats.Queries;

public record GetChatHistoryQuery(string GameId) : IRequest<ChatHistoryReadModel>;