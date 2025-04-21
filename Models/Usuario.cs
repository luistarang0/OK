using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OK.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(80)]
        [Column("ap_pat")]
        public string ApPat { get; set; }

        [Required]
        [MaxLength(80)]
        [Column("ap_mat")]
        public string ApMat { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(80)]
        public string Email { get; set; }

        [MaxLength(80)]
        public string? Carrera { get; set; }

        public byte? Semestre { get; set; }

        [Required]
        public byte Edad { get; set; }

        [Required]
        [RegularExpression("[MF]")]
        public string Sexo { get; set; }
        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public bool Estatus { get; set; } = true;
        
        [Required]
        [MaxLength(100)]
        public string Password { get; set; }
    }
}
