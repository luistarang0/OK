using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OK.Models
{
    [Table("Sesion_evaluaciones")]
    public class SesionEvaluaciones
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("fecha_realizacion")]
        public DateTime FechaRealizacion { get; set; } = DateTime.Now;

        [Column("id_nivel_satisfaccion")]
        public int? IdNivelSatisfaccion { get; set; }
        public NivelSatisfaccion? NivelSatisfaccion { get; set; }

        [Column("id_usuario")]
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        [Column("puntuacion")]
        public short? Puntuacion { get; set; }

        [Column("id_recomendacion")]
        public int? IdRecomendacion { get; set; }
        public Recomendacion? Recomendacion { get; set; }

        [Column("id_nivel_estres")]
        public int? IdNivelEstres { get; set; }
        public NivelEstres? NivelEstres { get; set; }

        [Column("estatus")]
        public byte Estatus { get; set; } = 1; // 1 = activa, 2 = finalizada

        // Relaciones inversas si deseas
        public ICollection<RespuestaTest>? RespuestasTest { get; set; }
        public ICollection<RespuestaEncuesta>? RespuestasEncuesta { get; set; }
    }
}
