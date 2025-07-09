using CoffeeChess.Service.Implementations;
using CoffeeChess.Service.Interfaces;
using CoffeeChess.Web.BackgroundWorkers;
using CoffeeChess.Web.Data;
using CoffeeChess.Web.Hubs;
using CoffeeChess.Web.Models;
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

builder.Services.AddHostedService<GameTimeoutBackgroundWorker>();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IGameManagerService, BaseGameManagerService>();
builder.Services.AddSingleton<IRatingService, EloRatingService>();
builder.Services.AddScoped<IGameFinisherService, SignalRGameFinisherService>();

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