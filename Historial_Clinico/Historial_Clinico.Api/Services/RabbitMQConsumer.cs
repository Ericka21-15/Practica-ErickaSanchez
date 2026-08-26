using Historial_Clinico.Api.Data;
using Historial_Clinico.Api.Events;
using Historial_Clinico.Api.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Historial_Clinico.Api.Services
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQConsumer(
            IConfiguration configuration,
            ILogger<RabbitMQConsumer> logger,
            IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]!),
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync(stoppingToken);
                    _channel = await _connection.CreateChannelAsync();
                    break;
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        ex,
                        "No se pudo conectar a RabbitMQ. Reintentando en 5 segundos..."
                    );
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            if (_connection == null || _channel == null)
                return;

            var queueName = _configuration["RabbitMQ:QueueName"]!;

            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensaje = Encoding.UTF8.GetString(body);

                var evento = JsonSerializer.Deserialize<PacienteCreadoEvento>(
                    mensaje,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (evento != null)
                {
                    _logger.LogInformation(
                        "Paciente creado recibido. IdPac: {IdPac}",
                        evento.id_pac
                    );

                    using var scope = _scopeFactory.CreateScope();

                    var dbContext = scope.ServiceProvider
                        .GetRequiredService<HistorialClinicoDBContext>();

                    var existe = await dbContext.HistorialesClinicos
                        .AnyAsync(h => h.id_pac == evento.id_pac);

                    if (!existe)
                    {
                        var historial = new tbl_historial_clinico
                        {
                            id_pac = evento.id_pac,
                            num_historia = evento.id_pac,
                            diagnostico_pac = "Sin diagnóstico",
                            tratamiento_pac = "Sin tratamiento",
                            fecha_his = DateTime.Now
                        };

                        dbContext.HistorialesClinicos.Add(historial);
                        await dbContext.SaveChangesAsync();

                        _logger.LogInformation(
                            "Historial clínico creado automáticamente para IdPac: {IdPac}",
                            evento.id_pac
                        );
                    }
                }

                await _channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false
                );
            };

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
