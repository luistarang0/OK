using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OK.Models
{
    public class RespuestaEncuesta
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("id_sesion")]

        public int IdSesion { get; set; }

        [ForeignKey("IdSesion")]
        public SesionEvaluaciones? Sesion { get; set; }
        [Column("id_pregunta_encuesta")]
        public int IdPreguntaEncuesta { get; set; }

        [ForeignKey("IdPreguntaEncuesta")]
        public PreguntaEncuesta? PreguntaEncuesta { get; set; }
        [Column("valor_respuesta")]
        public int? ValorRespuesta { get; set; }

        [Required]
        [StringLength(255)]
        [Column("valor_res_texto")]
        public string ValorResTexto { get; set; } = string.Empty;

        [Column("fecha_respuesta")]
        public DateTime FechaRespuesta { get; set; } = DateTime.Now;
    }
}
