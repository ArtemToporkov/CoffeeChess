using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Games.Enums;

namespace CoffeeChess.Domain.Games.ValueObjects;

public readonly struct MoveResult
{
    [MemberNotNullWhen(
        true, 
        nameof(San),
        nameof(FenAfterMove),
        nameof(IsCaptureOrPawnMove),
        nameof(MoveResultType),
        nameof(MoveResultType))]
    public bool Valid { get; init; }
    
    public San? San { get; init; }
    public Fen? FenAfterMove { get; init; }
    public bool? IsCaptureOrPawnMove { get; init; }
    public MoveResultType? MoveResultType { get; init; }
}