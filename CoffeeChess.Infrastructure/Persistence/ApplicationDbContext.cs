using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoffeeChess.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<UserModel>(options)
{
    public DbSet<Player> Players { get; set; }
    public DbSet<CompletedGameReadModel> CompletedGames { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.Rating)
                .HasDefaultValue(1500);
            
            entity.Property(p => p.Name)
                .IsRequired();
            
            entity.HasOne<UserModel>()
                .WithOne()
                .HasForeignKey<Player>(p => p.Id);
        });
        builder.Entity<CompletedGameReadModel>(entity =>
        {
            entity.HasKey(g => g.GameId);
            entity.Property(g => g.WhitePlayerId).IsRequired();
            entity.Property(g => g.WhitePlayerName).IsRequired();
            entity.Property(g => g.BlackPlayerId).IsRequired();
            entity.Property(g => g.BlackPlayerName).IsRequired();
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(g => g.WhitePlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(g => g.BlackPlayerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}