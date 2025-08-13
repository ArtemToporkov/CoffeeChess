using System.Linq.Expressions;
using System.Text.Json;
using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Songs.ReadModels;
using CoffeeChess.Domain.Games.ValueObjects;
using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Infrastructure.Identity;
using CoffeeChess.Infrastructure.Serialization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CoffeeChess.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<UserModel>(options)
{
    public DbSet<Player> Players { get; set; }
    public DbSet<CompletedGameReadModel> CompletedGames { get; set; }
    public DbSet<SongReadModel> Songs { get; set; }
    
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
            entity.Property(g => g.MovesHistory)
                .HasColumnType("jsonb")
                .HasConversion(
                    movesList => JsonSerializer.Serialize(movesList, new JsonSerializerOptions
                    {
                        Converters = { new SanConverter() }
                    }),
                    jsonMoves => JsonSerializer.Deserialize<List<MoveInfo>>(jsonMoves, new JsonSerializerOptions
                    {
                        Converters = { new SanConverter() }
                    }) ?? new List<MoveInfo>())
                .Metadata.SetValueComparer(new ValueComparer<List<MoveInfo>>(
                    (f, s) => 
                        ReferenceEquals(f, s) 
                        || (f == null && s == null) 
                        || (f != null && s != null && f.Count == s.Count && f.SequenceEqual(s)), 
                    list => list.Aggregate(0, HashCode.Combine), 
                    list => list));
        });
        builder.Entity<SongReadModel>(entity =>
        {
            entity.HasKey(e => e.SongId);
            entity.Property(e => e.Author).IsRequired();
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.AudioUrl).IsRequired();
            entity.Property(e => e.CoverUrl).IsRequired();
        });
    }
}