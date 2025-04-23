using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OK.Models
{
    public class DimensionTest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        [Column ("nombre_dimen")]
        public string NombreDimen { get; set; } = string.Empty;

        [StringLength(80)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [StringLength(80)]
        [Column("instrucciones")]
        public string Instrucciones { get; set; } = string.Empty;
        [Column("orden")]
        public int Orden { get; set; }

        public ICollection<PreguntaTest>? Preguntas { get; set; }
    }
}
