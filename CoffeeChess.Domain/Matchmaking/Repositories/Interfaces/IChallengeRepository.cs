using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;

public interface IChallengeRepository : IBaseRepository<Challenge>
{
    IEnumerable<Challenge> GetAll();
}