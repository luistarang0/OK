using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OK.Models
{
    [Table("Nivel_satisfaccion")]
    public class NivelSatisfaccion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("rango_min")]
        public int RangoMin { get; set; }

        [Required]
        [Column("rango_max")]
        public int RangoMax { get; set; }

        [Required]
        [StringLength(80)]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        // Relación con sesiones (evaluaciones)
        public ICollection<SesionEvaluaciones>? Sesiones { get; set; }
    }
}
