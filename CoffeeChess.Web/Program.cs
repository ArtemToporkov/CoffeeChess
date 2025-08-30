using CoffeeChess.Application.Chats.Repositories.Interfaces;
using CoffeeChess.Application.Chats.Services.Interfaces;
using CoffeeChess.Application.Games.EventHandlers;
using CoffeeChess.Application.Games.Repositories.Interfaces;
using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Application.Matchmaking.Services.Interfaces;
using CoffeeChess.Application.Players.Services.Interfaces;
using CoffeeChess.Application.Songs.Repositories.Interfaces;
using CoffeeChess.Application.Songs.Sevices;
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
using CoffeeChess.Infrastructure.Repositories.Implementations.Chats;
using CoffeeChess.Infrastructure.Repositories.Implementations.Games;
using CoffeeChess.Infrastructure.Repositories.Implementations.Matchmaking;
using CoffeeChess.Infrastructure.Repositories.Implementations.Players;
using CoffeeChess.Infrastructure.Repositories.Implementations.Songs;
using CoffeeChess.Infrastructure.Serialization;
using CoffeeChess.Infrastructure.Services.Implementations;
using CoffeeChess.Web.BackgroundWorkers;
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

builder.Services.AddControllersWithViews()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new SanConverter());
        opts.JsonSerializerOptions.Converters.Add(new ChatMessageConverter());
    });
builder.Services.AddSignalR(cfg => cfg.EnableDetailedErrors = true);
builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(typeof(DrawOfferDeclinedEventHandler).Assembly));

builder.Services.AddScoped<IMatchmakingService, RedisMatchmakingService>();
builder.Services.AddScoped<IChessMovesValidatorService, ChessDotNetCoreMovesValidatorService>();
builder.Services.AddScoped<IRatingService, EloRatingService>();
builder.Services.AddScoped<IPgnBuilderService, StringBuilderPgnBuilderService>();
builder.Services.AddScoped<IGameEventNotifierService, SignalRGameEventNotifierService>();
builder.Services.AddScoped<IPlayerEventNotifierService, SignalRPlayerEventNotifierService>();
builder.Services.AddScoped<IChatEventNotifierService, SignalRChatEventNotifierService>();
builder.Services.AddScoped<IMediaProviderService, WwwRootMediaProviderService>();

builder.Services.AddSingleton<IChallengeRepository, RedisChallengeRepository>();
builder.Services.AddSingleton<IChatRepository, RedisChatRepository>();
builder.Services.AddSingleton<IGameRepository, RedisGameRepository>();
builder.Services.AddScoped<IPlayerRepository, SqlPlayerRepository>();
builder.Services.AddScoped<ICompletedGameRepository, SqlCompletedGameRepository>();
builder.Services.AddScoped<IChatHistoryRepository, SqlChatHistoryRepository>();
builder.Services.AddScoped<ISongRepository, SqlSongRepository>();

builder.Services.AddHostedService<GameTimeoutCheckerService>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
try
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}
catch (Exception exception)
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(exception, exception.Message);
}

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
app.MapControllerRoute(
        name: "game",
        pattern: "{controller=GamesHistory}/{action=Review}/{gameId}")
    .WithStaticAssets();

app.MapHub<GameHub>("/gameHub");

app.Run();