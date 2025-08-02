using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Games.Enums;

namespace CoffeeChess.Domain.Games.ValueObjects;

public struct MoveResult
{
    [MemberNotNullWhen(
        true, 
        nameof(San),
        nameof(FenAfterMove),
        nameof(MoveType),
        nameof(MoveResultType),
        nameof(MoveResultType))]
    public bool Valid { get; init; }
    
    public string? San { get; init; }
    public string? FenAfterMove { get; init; }
    public MoveType? MoveType { get; init; }
    public MoveResultType? MoveResultType { get; init; }
}