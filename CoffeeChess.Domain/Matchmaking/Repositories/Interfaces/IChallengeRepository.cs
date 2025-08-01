using CoffeeChess.Domain.Players.Entities;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;

public interface IChallengeRepository : IBaseRepository<GameChallenge>
{
    IEnumerable<GameChallenge> GetAll();
}