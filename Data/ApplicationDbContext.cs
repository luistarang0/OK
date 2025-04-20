using Microsoft.EntityFrameworkCore;
using OK.Models;
using System;

namespace OK.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        //public DbSet<DimensionTest> DimensionesTest { get; set; }
        //public DbSet<PreguntaTest> PreguntasTest { get; set; }
        //public DbSet<Recomendacion> Recomendaciones { get; set; }
        //public DbSet<NivelEstres> NivelesEstres { get; set; }
        //public DbSet<NivelSatisfaccion> NivelesSatisfaccion { get; set; }
        //public DbSet<PreguntaEncuesta> PreguntasEncuesta { get; set; }
        //public DbSet<SesionEvaluacion> SesionesEvaluacion { get; set; }
        //public DbSet<RespuestaTest> RespuestasTest { get; set; }
        //public DbSet<RespuestaEncuesta> RespuestasEncuesta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuraciones adicionales si son necesarias
        }
    }
}
