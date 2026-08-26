using Microsoft.EntityFrameworkCore;
using Pacientes.Api.Models;

namespace Pacientes.Api.Data
{
    public class PacientesDBContext : DbContext
    {
        public PacientesDBContext(DbContextOptions<PacientesDBContext> options) : base(options)
        {
        }

        public DbSet<tbl_pacientes> Pacientes { get; set; }
    }
}
