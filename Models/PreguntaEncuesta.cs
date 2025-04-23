using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OK.Models
{
    public class PreguntaEncuesta
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("texto_pregunta")]
        public string TextoPregunta { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("tipo_pregunta")]
        public string? TipoPregunta { get; set; }

        [Column("orden")]
        public int Orden { get; set; }

        public ICollection<RespuestaEncuesta>? Respuestas { get; set; }
    }
}
