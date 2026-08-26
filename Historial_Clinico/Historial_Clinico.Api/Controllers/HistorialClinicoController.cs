using Historial_Clinico.Api.Data;
using Historial_Clinico.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Historial_Clinico.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistorialClinicoController : ControllerBase
    {
        private readonly HistorialClinicoDBContext _dbContext;

        public HistorialClinicoController(HistorialClinicoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<tbl_historial_clinico>>> GetHistorialesClinicos()
        {
            var historiales = await _dbContext.HistorialesClinicos
                .AsNoTracking()
                .ToListAsync();
            return Ok(historiales);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<tbl_historial_clinico>> GetHistorialClinico(int id)
        {
            var historial = await _dbContext.HistorialesClinicos
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.id_histcl == id);
            if (historial == null) return NotFound();
            return Ok(historial);
        }

        [HttpPost]
        public async Task<ActionResult<tbl_historial_clinico>> CrearHistorialClinico(tbl_historial_clinico historial)
        {
            _dbContext.HistorialesClinicos.Add(historial);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHistorialClinico),
                new { id = historial.id_histcl },
                historial);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarHistorialClinico(int id, tbl_historial_clinico historial)
        {
            if (id != historial.id_histcl)
                return BadRequest();

            _dbContext.Entry(historial).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarHistorialClinico(int id)
        {
            var historial = await _dbContext.HistorialesClinicos.FindAsync(id);
            if (historial == null) return NotFound();

            _dbContext.HistorialesClinicos.Remove(historial);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
