using CoffeeChess.Application.Games.Commands;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.ValueObjects;
using MediatR;

namespace CoffeeChess.Application.Games.CommandHandlers;

public class MakeMoveCommandHandler(IGameRepository gameRepository, IChessMovesValidator chessMovesValidator) : IRequestHandler<MakeMoveCommand>
{
    public async Task Handle(MakeMoveCommand request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(request.GameId, cancellationToken) 
                   ?? throw new InvalidOperationException(
                       $"[{nameof(MakeMoveCommandHandler)}.{nameof(Handle)}]: game not found.");

        if (game.IsOver)
            throw new InvalidOperationException(
                $"[{nameof(MakeMoveCommandHandler)}.{nameof(Handle)}]: game is over.");
        
        game.ApplyMove(chessMovesValidator, 
            request.PlayerId, 
            new ChessSquare(request.From), 
            new ChessSquare(request.To), 
            ConvertCharToPromotion(request.Promotion));
        await gameRepository.SaveChangesAsync(game, cancellationToken);
    }

    private Promotion? ConvertCharToPromotion(string? promotion)
        => promotion?[0] switch
        {
            'b' => Promotion.Bishop,
            'n' => Promotion.Knight,
            'r' => Promotion.Rook,
            'q' => Promotion.Queen,
            null => null,
            _ => throw new ArgumentException(
                $"Argument {nameof(promotion)} is not a valid promotion: should be ether b, n, r or q.")
        };
}