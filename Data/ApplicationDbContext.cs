using Microsoft.EntityFrameworkCore;
using OK.Models;
using System;

namespace OK.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<DimensionTest> DimensionesTest { get; set; }
        public DbSet<PreguntaTest> PreguntasTest { get; set; }
        public DbSet<Recomendacion> Recomendaciones { get; set; }
        public DbSet<NivelEstres> NivelesEstres { get; set; }
        public DbSet<NivelSatisfaccion> NivelesSatisfaccion { get; set; }
        public DbSet<PreguntaEncuesta> PreguntasEncuesta { get; set; }
        public DbSet<SesionEvaluaciones> Sesiones { get; set; }
        public DbSet<RespuestaTest> RespuestasTest { get; set; }
        public DbSet<RespuestaEncuesta> RespuestasEncuesta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SesionEvaluaciones>()
                .HasOne(s => s.NivelEstres)
                .WithMany(n => n.Sesiones)
                .HasForeignKey(s => s.IdNivelEstres)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SesionEvaluaciones>()
                .HasOne(s => s.Recomendacion)
                .WithMany(r => r.Sesiones)
                .HasForeignKey(s => s.IdRecomendacion)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SesionEvaluaciones>()
                .HasOne(s => s.Usuario)
                .WithMany(u => u.Sesiones)
                .HasForeignKey(s => s.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SesionEvaluaciones>()
                .HasOne(s => s.NivelSatisfaccion)
                .WithMany(n => n.Sesiones)
                .HasForeignKey(s => s.IdNivelSatisfaccion)
                .OnDelete(DeleteBehavior.SetNull);

            // PreguntasTest: relacion con Dimensiones
            modelBuilder.Entity<PreguntaTest>()
                .HasOne(p => p.Dimension)
                .WithMany(d => d.Preguntas)
                .HasForeignKey(p => p.IdDimension)
                .OnDelete(DeleteBehavior.Cascade);

            // RespuestasTest: relacion con Sesion y PreguntaTest
            modelBuilder.Entity<RespuestaTest>()
                .HasOne(r => r.Sesion)
                .WithMany(s => s.RespuestasTest)
                .HasForeignKey(r => r.IdSesion)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RespuestaTest>()
                .HasOne(r => r.PreguntaTest)
                .WithMany(p => p.Respuestas)
                .HasForeignKey(r => r.IdPreguntaTest)
                .OnDelete(DeleteBehavior.Cascade);

            // RespuestaEncuesta
            modelBuilder.Entity<RespuestaEncuesta>()
                .HasOne(r => r.Sesion)
                .WithMany(s => s.RespuestasEncuesta)
                .HasForeignKey(r => r.IdSesion)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RespuestaEncuesta>()
                .HasOne(r => r.PreguntaEncuesta)
                .WithMany(p => p.Respuestas)
                .HasForeignKey(r => r.IdPreguntaEncuesta)
                .OnDelete(DeleteBehavior.Cascade);

            // RespuestasEncuesta: relacion con Sesion y PreguntaEncuesta
            modelBuilder.Entity<SesionEvaluaciones>()
                .HasOne(s => s.NivelSatisfaccion)
                .WithMany(n => n.Sesiones)
                .HasForeignKey(s => s.IdNivelSatisfaccion)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
