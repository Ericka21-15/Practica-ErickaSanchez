using Microsoft.EntityFrameworkCore;
using Pacientes.Api.Data;
using Pacientes.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<RabbitMQPublisher>();

builder.Services.AddDbContext<PacientesDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PacientesConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PacientesDBContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
