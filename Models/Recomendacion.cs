using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OK.Models
{
    [Table("Recomendaciones")]
    public class Recomendacion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("recomendacion")]
        public string RecomendacionTexto { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty; // Físico, Cognitivo, Conductual

        // Relaciones
        public ICollection<SesionEvaluaciones>? Sesiones { get; set; }
    }
}
