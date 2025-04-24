using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OK.Models
{
    [Table("Respuesta_test")]
    public class RespuestaTest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("id_sesion")]
        public int IdSesion { get; set; }

        [ForeignKey("IdSesion")]
        public SesionEvaluaciones? Sesion { get; set; }

        [Required]
        [Column("id_pregunta_test")]
        public int IdPreguntaTest { get; set; }

        [ForeignKey("IdPreguntaTest")]
        public PreguntaTest? PreguntaTest { get; set; }

        [Required]
        [Column("valor_respuesta")]
        public byte ValorRespuesta { get; set; }

        [Required]
        [StringLength(80)]
        [Column("valor_res_texto")]
        public string ValorResTexto { get; set; } = string.Empty;

        [Column("fecha_respuesta")]
        public DateTime FechaRespuesta { get; set; } = DateTime.Now;
    }
}
