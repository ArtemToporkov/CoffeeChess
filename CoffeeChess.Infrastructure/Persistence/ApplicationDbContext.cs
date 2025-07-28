using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Entities;
using CoffeeChess.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoffeeChess.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<UserModel>(options)
{
    public DbSet<Player> Players { get; set; }
    
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
    }
}