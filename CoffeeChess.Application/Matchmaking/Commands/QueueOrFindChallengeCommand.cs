using CoffeeChess.Domain.Matchmaking.Enums;
using MediatR;

namespace CoffeeChess.Application.Matchmaking.Commands;

public record QueueOrFindChallengeCommand(
    string PlayerId, 
    int Minutes, int Increment, 
    ColorPreference ColorPreference, 
    int MinRating, int MaxRating) : IRequest;