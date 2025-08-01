using CoffeeChess.Application.Chats.Services.Interfaces;
using CoffeeChess.Application.Games.EventHandlers;
using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Application.Matchmaking.Services.Implementations;
using CoffeeChess.Application.Matchmaking.Services.Interfaces;
using CoffeeChess.Application.Players.Services.Interfaces;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Games.Services.Implementations;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using CoffeeChess.Domain.Players.Services.Implementations;
using CoffeeChess.Domain.Players.Services.Interfaces;
using CoffeeChess.Infrastructure.Identity;
using CoffeeChess.Infrastructure.Persistence;
using CoffeeChess.Infrastructure.Repositories.Implementations;
using CoffeeChess.Web.HostedServices;
using CoffeeChess.Web.Hubs;
using CoffeeChess.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString!));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddIdentity<UserModel, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(DrawOfferDeclinedEventHandler).Assembly));

builder.Services.AddScoped<IMatchmakingService, InMemoryMatchmakingService>();
builder.Services.AddScoped<IRatingService, EloRatingService>();
builder.Services.AddScoped<IPgnBuilderService, StringBuilderPgnBuilderService>();
builder.Services.AddScoped<IGameEventNotifierService, SignalRGameEventNotifierService>();
builder.Services.AddScoped<IPlayerEventNotifierService, SignalRPlayerEventNotifierService>();
builder.Services.AddScoped<IChatEventNotifierService, SignalRChatEventNotifierService>();

builder.Services.AddSingleton<IChallengeRepository, InMemoryChallengeRepository>();
builder.Services.AddSingleton<IChatRepository, InMemoryChatRepository>();
builder.Services.AddSingleton<IGameRepository, RedisGameRepository>();
builder.Services.AddScoped<IPlayerRepository, SqlPlayerRepository>();

builder.Services.AddHostedService<GameTimeoutCheckerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapControllerRoute(
        name: "game",
        pattern: "{controller=Game}/{action=Play}/{id?}")
    .WithStaticAssets();

app.MapHub<GameHub>("/gameHub");

app.Run();