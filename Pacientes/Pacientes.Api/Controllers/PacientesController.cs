using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pacientes.Api.Data;
using Pacientes.Api.Models;
using Pacientes.Api.Services;

namespace Pacientes.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private readonly PacientesDBContext _dbContext;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public PacientesController(PacientesDBContext dbContext, RabbitMQPublisher rabbitMQPublisher)
        {
            _dbContext = dbContext;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<tbl_pacientes>>> GetPacientes()
        {
            var pacientes = await _dbContext.Pacientes
                .AsNoTracking()
                .ToListAsync();
            return Ok(pacientes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<tbl_pacientes>> GetPaciente(int id)
        {
            var paciente = await _dbContext.Pacientes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.id_pac == id);
            if (paciente == null) return NotFound();
            return Ok(paciente);
        }

        [HttpPost]
        public async Task<ActionResult<tbl_pacientes>> CrearPaciente(tbl_pacientes paciente)
        {
            _dbContext.Pacientes.Add(paciente);
            await _dbContext.SaveChangesAsync();

            await _rabbitMQPublisher.PublicarPacienteCreadoAsync(paciente);

            return CreatedAtAction(nameof(GetPaciente),
                new { id = paciente.id_pac },
                paciente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPaciente(int id, tbl_pacientes paciente)
        {
            if (id != paciente.id_pac)
                return BadRequest();

            _dbContext.Entry(paciente).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPaciente(int id)
        {
            var paciente = await _dbContext.Pacientes.FindAsync(id);
            if (paciente == null) return NotFound();

            _dbContext.Pacientes.Remove(paciente);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
