using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OK.Models
{
    public class PreguntaTest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("texto_pregunta")]
        public string TextoPregunta { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("tipo_pregunta")]
        public string TipoPregunta { get; set; } = string.Empty; // filtro, intensidad, frecuencia
        [Column("orden")]
        public int Orden { get; set; }
        [Column("id_dimension")]
        public int IdDimension { get; set; }

        [ForeignKey("IdDimension")]
        public DimensionTest? Dimension { get; set; }

        public ICollection<RespuestaTest>? Respuestas { get; set; }
    }
}
