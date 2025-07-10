using System.Diagnostics.CodeAnalysis;

namespace CoffeeChess.Core.Enums;

public class DrawOfferResult
{
    [MemberNotNullWhen(false, nameof(Message))]
    public bool Success { get; private init; }
    public string? Message { get; private init; }

    public static DrawOfferResult Ok() => new() { Success = true };

    public static DrawOfferResult Fail(string? message) => new() { Success = false, Message = message };
}