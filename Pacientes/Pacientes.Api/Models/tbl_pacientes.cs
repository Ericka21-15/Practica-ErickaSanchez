using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pacientes.Api.Models
{
    [Table("tbl_pacientes")]
    public class tbl_pacientes
    {
        [Key]
        [Column("id_pac")]
        public int id_pac { get; set; }

        [Required]
        [StringLength(13)]
        [Column("cedula_pac")]
        public string cedula_pac { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Column("nombre_pac")]
        public string nombre_pac { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Column("apellido_pac")]
        public string apellido_pac { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("direccion_pac")]
        public string direccion_pac { get; set; } = string.Empty;
    }
}
