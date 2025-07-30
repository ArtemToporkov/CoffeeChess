using CoffeeChess.Application.EventHandlers;
using CoffeeChess.Application.Services;
using CoffeeChess.Application.Services.Implementations;
using CoffeeChess.Application.Services.Interfaces;
using CoffeeChess.Domain.Repositories.Interfaces;
using CoffeeChess.Domain.Services.Implementations;
using CoffeeChess.Domain.Services.Interfaces;
using CoffeeChess.Infrastructure.Identity;
using CoffeeChess.Infrastructure.Persistence;
using CoffeeChess.Infrastructure.Repositories.Implementations;
using CoffeeChess.Web.Hubs;
using CoffeeChess.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
    cfg.RegisterServicesFromAssembly(typeof(GameEventsHandler).Assembly));

builder.Services.AddScoped<IMatchmakingService, InMemoryMatchmakingService>();
builder.Services.AddScoped<IRatingService, EloRatingService>();
builder.Services.AddScoped<IGameEventNotifierService, SignalRGameEventNotifierService>();
builder.Services.AddScoped<IPlayerEventNotifierService, SignalRPlayerEventNotifierService>();

builder.Services.AddScoped<IChallengeRepository, InMemoryChallengeRepository>();
builder.Services.AddScoped<IGameRepository, InMemoryGameRepository>();
builder.Services.AddScoped<IPlayerRepository, SqlPlayerRepository>();

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