using CoffeeChess.Consumers.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<GameEndedConsumer>();

var host = builder.Build();
host.Run();