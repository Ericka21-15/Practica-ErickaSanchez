using Historial_Clinico.Api.Data;
using Historial_Clinico.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHostedService<RabbitMQConsumer>();

builder.Services.AddDbContext<HistorialClinicoDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("HistorialClinicoConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HistorialClinicoDBContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
