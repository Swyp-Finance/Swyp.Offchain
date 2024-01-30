using Microsoft.EntityFrameworkCore;
using Swyp.Data;
using Swyp.Sync.Reducers;
using Swyp.Sync.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<SwypDbContext>(options =>
{
    options
    .UseNpgsql(
        builder.Configuration
        .GetConnectionString("SwypContext"),
            x =>
            {
                x.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    builder.Configuration.GetConnectionString("SwypContextSchema")
                );
            }
        );
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Reducers
builder.Services.AddSingleton<IBlockReducer, BlockReducer>();
builder.Services.AddSingleton<ICoreReducer, TransactionOutputReducer>();
builder.Services.AddSingleton<IReducer, TbcByAddressReducer>();

builder.Services.AddHostedService<CardanoIndexWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
