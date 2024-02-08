using Cardano.Sync;
using Cardano.Sync.Reducers;
using Swyp.Sync.Data;
using Swyp.Sync.Reducers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCardanoIndexer<SwypDbContext>(builder.Configuration, 60);

builder.Services.AddSingleton<IReducer, TbcByAddressReducer>();
builder.Services.AddSingleton<IReducer, TeddyAddressReducer>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
