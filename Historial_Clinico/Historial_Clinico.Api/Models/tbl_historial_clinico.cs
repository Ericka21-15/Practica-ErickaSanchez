using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Historial_Clinico.Api.Models
{
    [Table("tbl_historial_clinico")]
    public class tbl_historial_clinico
    {
        [Key]
        [Column("id_histcl")]
        public int id_histcl { get; set; }

        [Column("id_pac")]
        public int id_pac { get; set; }

        [Column("num_historia")]
        public int num_historia { get; set; }

        [Required]
        [StringLength(150)]
        [Column("diagnostico_pac")]
        public string diagnostico_pac { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("tratamiento_pac")]
        public string tratamiento_pac { get; set; } = string.Empty;

        [Column("fecha_his")]
        public DateTime? fecha_his { get; set; }
    }
}
