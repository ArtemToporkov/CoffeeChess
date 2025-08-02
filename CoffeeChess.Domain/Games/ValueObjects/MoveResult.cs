using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Games.Enums;

namespace CoffeeChess.Domain.Games.ValueObjects;

public readonly struct MoveResult
{
    [MemberNotNullWhen(
        true, 
        nameof(San),
        nameof(FenAfterMove),
        nameof(MoveType),
        nameof(MoveResultType),
        nameof(MoveResultType))]
    public bool Valid { get; init; }
    
    public SanMove? San { get; init; }
    public Fen? FenAfterMove { get; init; }
    public MoveType? MoveType { get; init; }
    public MoveResultType? MoveResultType { get; init; }
}