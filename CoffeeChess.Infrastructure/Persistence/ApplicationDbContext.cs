using System.Linq.Expressions;
using System.Text.Json;
using CoffeeChess.Application.Chats.ReadModels;
using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Songs.ReadModels;
using CoffeeChess.Domain.Chats.ValueObjects;
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
    public DbSet<ChatHistoryReadModel> ChatsHistory { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        ConfigurePlayer(builder);
        ConfigureCompletedGameReadModel(builder);
        ConfigureSongReadModel(builder);
        ConfigureChatHistoryReadModel(builder);
    }

    private static void ConfigurePlayer(ModelBuilder builder)
    {
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
    }

    private static void ConfigureCompletedGameReadModel(ModelBuilder builder)
    {
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
            var movesHistorySerializerOptions = new JsonSerializerOptions { Converters = { new SanConverter() } };
            entity.Property(g => g.MovesHistory)
                .HasColumnType("jsonb")
                .HasConversion(
                    movesList => JsonSerializer.Serialize(movesList, movesHistorySerializerOptions),
                    jsonMoves => JsonSerializer.Deserialize<List<MoveInfo>>(
                        jsonMoves, movesHistorySerializerOptions) ?? new List<MoveInfo>())
                .Metadata.SetValueComparer(new ValueComparer<List<MoveInfo>>(
                    (f, s) => 
                        ReferenceEquals(f, s) 
                        || (f == null && s == null) 
                        || (f != null && s != null && f.Count == s.Count && f.SequenceEqual(s)), 
                    list => list.Aggregate(0, HashCode.Combine), 
                    list => list));
        });
    }

    private static void ConfigureSongReadModel(ModelBuilder builder)
    {
        builder.Entity<SongReadModel>(entity =>
        {
            entity.HasKey(e => e.SongId);
            entity.Property(e => e.Author).IsRequired();
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.AudioUrl).IsRequired();
            entity.Property(e => e.CoverUrl).IsRequired();
        });
    }

    private static void ConfigureChatHistoryReadModel(ModelBuilder builder)
    {
        var chatMessageSerializerOptions = new JsonSerializerOptions { Converters = { new ChatMessageConverter() } };
        builder.Entity<ChatHistoryReadModel>(entity =>
        {
            entity.HasKey(e => e.GameId);
            entity.Property(e => e.Messages).IsRequired();
            entity.Property(e => e.Messages)
                .HasColumnType("jsonb")
                .HasConversion(
                    messages => JsonSerializer.Serialize(messages, chatMessageSerializerOptions),
                    jsonMessages => JsonSerializer.Deserialize<IReadOnlyList<ChatMessage>>(
                        jsonMessages, chatMessageSerializerOptions) ?? new List<ChatMessage>())
                .Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<ChatMessage>>(
                    (f, s) => 
                        ReferenceEquals(f, s) 
                        || (f == null && s == null) 
                        || (f != null && s != null && f.Count == s.Count && f.SequenceEqual(s)), 
                    list => list.Aggregate(0, HashCode.Combine), 
                    list => list));
        });
    }
}