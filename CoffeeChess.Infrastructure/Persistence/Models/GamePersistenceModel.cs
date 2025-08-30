using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ChessDotNetCore;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.ValueObjects;
using CoffeeChess.Domain.Shared.Interfaces;
using CoffeeChess.Infrastructure.Mapping.Helpers;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StackExchange.Redis;

namespace CoffeeChess.Infrastructure.Persistence.Models;

public class GamePersistenceModel
{
     public const string GameIdName = "gameId";
     public const string WhitePlayerIdName = "whitePlayerId";
     public const string BlackPlayerIdName = "blackPlayerId";
     public const string IsOverName = "isOver";
     public const string InitialTimeForOnePlayerName = "initialTimeForOnePlayer";
     public const string IncrementName = "increment";
     public const string LastTimeUpdateName = "lastTimeUpdate";
     public const string WhiteTimeLeftName = "whiteTimeLeft";
     public const string BlackTimeLeftName = "blackTimeLeft";
     public const string CurrentPlayerColorName = "currentPlayerColor";
     public const string PlayerWithDrawOfferName = "playerWithDrawOffer";
     public const string CurrentFenName = "currentFen";
     public const int PlayerColorNullValue = -1;
     
     public HashEntry[] StaticMetadata { get; }
     public HashEntry[] MetadataThatCanUpdate { get; }
     public HashEntry[] PositionsForThreefold { get; }
     public RedisValue[] MovesHistory { get; }

     public GamePersistenceModel(HashEntry[] metadata, HashEntry[] positionsForThreefold, RedisValue[] movesHistory)
     {
          var metadataDictionary = metadata.ToDictionary(entry => entry.Name, entry => entry.Value);
          StaticMetadata =
          [
               new(GameIdName, metadataDictionary[GameIdName]),
               new(WhitePlayerIdName, metadataDictionary[WhitePlayerIdName]),
               new(BlackPlayerIdName, metadataDictionary[BlackPlayerIdName]),
               new(InitialTimeForOnePlayerName, (double)metadataDictionary[InitialTimeForOnePlayerName]),
               new(IncrementName, (double)metadataDictionary[IncrementName])
          ];
          MetadataThatCanUpdate =
          [
               new(IsOverName, metadataDictionary[IsOverName]),
               new(LastTimeUpdateName, metadataDictionary[LastTimeUpdateName]),
               new(WhiteTimeLeftName, metadataDictionary[WhiteTimeLeftName]),
               new(BlackTimeLeftName, metadataDictionary[BlackTimeLeftName]),
               new(CurrentPlayerColorName, metadataDictionary[CurrentPlayerColorName]),
               new(CurrentFenName, metadataDictionary[CurrentFenName]),
               new(PlayerWithDrawOfferName, metadataDictionary[PlayerWithDrawOfferName])
          ];
          PositionsForThreefold = positionsForThreefold;
          MovesHistory = movesHistory;
     }

     public static GamePersistenceModel FromGame(Game game, JsonSerializerOptions? serializerOptions = null)
          => new(
               GetMetadata(game), 
               GetPositionsForThreefold(game), 
               GetMovesHistory(game, serializerOptions: serializerOptions));

     public Game ToGame(JsonSerializerOptions? serializerOptions = null)
     {
          var game = (Game)RuntimeHelpers.GetUninitializedObject(typeof(Game));
          var metadataDictionary = StaticMetadata
               .Concat(MetadataThatCanUpdate)
               .ToDictionary(entry => entry.Name.ToString(), entry => entry.Value);
          var propertiesToSet = new (string GamePropertyName, object? PropertyValue)[]
          {
               (nameof(Game.GameId), (string)metadataDictionary[GameIdName]!),
               (nameof(Game.WhitePlayerId), (string)metadataDictionary[WhitePlayerIdName]!),
               (nameof(Game.BlackPlayerId), (string)metadataDictionary[BlackPlayerIdName]!),
               (nameof(Game.IsOver), (bool)metadataDictionary[IsOverName]!),
               (nameof(Game.InitialTimeForOnePlayer),
                    TimeSpan.FromMicroseconds((double)metadataDictionary[InitialTimeForOnePlayerName]!)),
               (nameof(Game.Increment),
                    TimeSpan.FromMicroseconds((double)metadataDictionary[IncrementName]!)),
               (nameof(Game.LastTimeUpdate),
                    DateTime.TryParseExact(
                         metadataDictionary[LastTimeUpdateName]!,
                         "O",
                         CultureInfo.InvariantCulture,
                         DateTimeStyles.RoundtripKind,
                         out var lastTimeUpdate)
                         ? lastTimeUpdate
                         : throw new FormatException($"Can't parse a {LastTimeUpdateName} " +
                                                     $"with value \"{(string)metadataDictionary[LastTimeUpdateName]!}\" " +
                                                     $"to DateTime.")),
               (nameof(Game.WhiteTimeLeft),
                    TimeSpan.FromMicroseconds((double)metadataDictionary[WhiteTimeLeftName]!)),
               (nameof(Game.BlackTimeLeft),
                    TimeSpan.FromMicroseconds((double)metadataDictionary[BlackTimeLeftName]!)),
               (nameof(Game.CurrentPlayerColor),
                    ConvertPlayerColorOrThrow((int)metadataDictionary[CurrentPlayerColorName]!)
                    ?? throw new ArgumentException($"A {nameof(Game.CurrentPlayerColor)} property can't be null.")),
               (nameof(Game.PlayerWithDrawOffer),
                    ConvertPlayerColorOrThrow((int)metadataDictionary[PlayerWithDrawOfferName]!)),
               (nameof(Game.CurrentFen), new Fen(metadataDictionary[CurrentFenName]!))
          };
          foreach (var (propertyName, propertyValue) in propertiesToSet)
               ReflectionMemberAccessHelper.SetPropertyValueOrThrow(game, propertyName, propertyValue);
          var movesHistory = MovesHistory
               .Select(move => JsonSerializer.Deserialize<MoveInfo>(move!, serializerOptions))
               .ToList();
          ReflectionMemberAccessHelper.SetFieldValueOrThrow(game, "_movesHistory", movesHistory);
          var positionsForThreefold = PositionsForThreefold
               .ToDictionary(entry => entry.Name.ToString(), entry => (int)entry.Value);
          ReflectionMemberAccessHelper.SetFieldValueOrThrow(
               game, "_positionsForThreefoldCount", positionsForThreefold);
          ReflectionMemberAccessHelper.SetFieldValueOrThrow(
               game, "_domainEvents", new List<IDomainEvent>());
          return game;
     }
     
     private static int ConvertNullablePlayerColor(PlayerColor? playerColor) 
          => playerColor.HasValue ? (int)playerColor.Value : PlayerColorNullValue;

     private static PlayerColor? ConvertPlayerColorOrThrow(int playerColor)
     {
          if (playerColor == PlayerColorNullValue)
               return null;
          if (!Enum.IsDefined(typeof(PlayerColor), playerColor))
               throw new InvalidEnumArgumentException(nameof(playerColor), playerColor, typeof(PlayerColor));
          return (PlayerColor)playerColor;
     }
     
     private static HashEntry[] GetMetadata(Game game)
     {
          var toUpdate = GetMetadataToUpdate(game).ToList();
          toUpdate.AddRange([
               new(GameIdName, game.GameId),
               new(WhitePlayerIdName, game.WhitePlayerId),
               new(BlackPlayerIdName, game.BlackPlayerId),
               new(InitialTimeForOnePlayerName, game.InitialTimeForOnePlayer.TotalMicroseconds),
               new(IncrementName, game.Increment.TotalMicroseconds),
          ]);
          return toUpdate.ToArray();
     }

     private static HashEntry[] GetMetadataToUpdate(Game game)
          =>
          [
               new(IsOverName, game.IsOver),
               new(LastTimeUpdateName, game.LastTimeUpdate.ToString("O")),
               new(WhiteTimeLeftName, game.WhiteTimeLeft.TotalMicroseconds),
               new(BlackTimeLeftName, game.BlackTimeLeft.TotalMicroseconds),
               new(CurrentPlayerColorName, (int)game.CurrentPlayerColor),
               new(CurrentFenName, (string)game.CurrentFen),
               new(PlayerWithDrawOfferName, ConvertNullablePlayerColor(game.PlayerWithDrawOffer))
          ];
     
     
     private static HashEntry[] GetPositionsForThreefold(Game game)
     {
          const string positionsFieldName = "_positionsForThreefoldCount";
          var positionsField = ReflectionMemberAccessHelper.GetPrivateFieldOrThrow<Game>(positionsFieldName);
          var positions = ReflectionMemberAccessHelper.GetFieldValueOrThrow<Game, Dictionary<string, int>>(
               positionsField, game);
          return positions
               .Select(kvp => new HashEntry(kvp.Key, kvp.Value))
               .ToArray();
     }
     
     private static RedisValue[] GetMovesHistory(Game game, JsonSerializerOptions? serializerOptions = null)
     {
          const string movesHistoryFieldName = "_movesHistory";
          var movesHistoryField = ReflectionMemberAccessHelper.GetPrivateFieldOrThrow<Game>(movesHistoryFieldName);
          var movesHistory = ReflectionMemberAccessHelper.GetFieldValueOrThrow<Game, List<MoveInfo>>(
               movesHistoryField, game);
          return movesHistory
               .Select(move => (RedisValue)JsonSerializer.Serialize(move, serializerOptions))
               .ToArray();
     }
}