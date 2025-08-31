using CoffeeChess.Domain.Games.Exceptions;
using CoffeeChess.Domain.Matchmaking.Enums;
using CoffeeChess.Domain.Matchmaking.Events;
using CoffeeChess.Domain.Matchmaking.Exceptions;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Matchmaking.Entities;

public class Challenge(string playerId, int playerRating, ChallengeSettings challengeSettings) 
    : AggregateRoot<IDomainEvent>
{
    public string PlayerId { get; } = playerId;
    public int PlayerRating { get; } = playerRating;
    public ChallengeSettings ChallengeSettings { get; } = challengeSettings;
    public bool IsAccepted { get; private set; }

    public void Accept(Challenge toAccept)
    {
        if (IsAccepted)
            throw new InvalidMatchmakingOperationException(
                $"Challenge owned by player with ID \"{PlayerId}\" is already accepted.");
        if (!IsMatchingWith(toAccept))
            throw new InvalidMatchmakingOperationException(
                $"Tried to accept not matching challenge. Player ID: \"{PlayerId}\".");
        IsAccepted = true;
        AddDomainEvent(new ChallengeAccepted(
            PlayerId, ChallengeSettings, toAccept.PlayerId));
    }

    private bool IsMatchingWith(Challenge other)
    {
        var sameTimeControl 
            = other.ChallengeSettings.TimeControl.Minutes == ChallengeSettings.TimeControl.Minutes
              && other.ChallengeSettings.TimeControl.Increment == ChallengeSettings.TimeControl.Increment;
        var ratingMatching 
            = other.PlayerRating >= ChallengeSettings.EloRatingPreference.Min 
              && other.PlayerRating <= ChallengeSettings.EloRatingPreference.Max 
              && PlayerRating >= other.ChallengeSettings.EloRatingPreference.Min
              && PlayerRating <= other.ChallengeSettings.EloRatingPreference.Max;
        var colorPreferenceMatching 
            = other.ChallengeSettings.ColorPreference == ColorPreference.Any 
              || ChallengeSettings.ColorPreference == ColorPreference.Any
              || other.ChallengeSettings.ColorPreference == ColorPreference.White
                && ChallengeSettings.ColorPreference == ColorPreference.Black
              || other.ChallengeSettings.ColorPreference == ColorPreference.Black
                && ChallengeSettings.ColorPreference == ColorPreference.White;
        return sameTimeControl && ratingMatching && colorPreferenceMatching;
    }
}