using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OK.Models
{
    public class NivelEstres
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("rango")]
        public int Rango { get; set; } // Límite superior del rango, ej. 48 para "Leve"

        [Required]
        [StringLength(80)]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        // Relaciones con SesionEvaluaciones (si las manejas con EF)
        public ICollection<SesionEvaluaciones>? Sesiones { get; set; }
    }
}
