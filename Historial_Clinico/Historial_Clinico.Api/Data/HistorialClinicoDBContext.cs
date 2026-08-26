using Historial_Clinico.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Historial_Clinico.Api.Data
{
    public class HistorialClinicoDBContext : DbContext
    {
        public HistorialClinicoDBContext(DbContextOptions<HistorialClinicoDBContext> options) : base(options)
        {
        }

        public DbSet<tbl_historial_clinico> HistorialesClinicos { get; set; }
    }
}
