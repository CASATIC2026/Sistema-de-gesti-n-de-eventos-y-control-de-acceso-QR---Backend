using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Models;

namespace EventAccessControl.API.Data
{
    /// <summary>
    /// Contexto de la base de datos para la aplicación de control de acceso a eventos. 
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Constructor que recibe las opciones de configuración para el contexto de la base de datos. 
        /// </summary>
        /// <param name="options"></param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        /// <summary>
        /// DbSet que representa la colección de eventos en la base de datos. Permite realizar operaciones CRUD sobre los eventos, como crear nuevos eventos, consultar eventos 
        /// existentes, actualizar eventos y eliminar eventos.
        /// </summary>
        public DbSet<Event> Events { get; set; }

        /// <summary>
        /// DbSet que representa la colección de tickets en la base de datos. Permite realizar operaciones CRUD sobre los tickets, como registrar nuevos tickets para eventos, 
        /// consultar tickets existentes, actualizar tickets y eliminar tickets.
        /// </summary>
        public DbSet<Ticket> Tickets { get; set; }

        /// <summary>
        /// DbSet que representa la colección de registros de ingreso (check-in) en la base de datos. Permite realizar operaciones CRUD sobre los registros de ingreso, como crear 
        /// nuevos registros de ingreso, consultar registros de ingreso existentes, actualizar registros de ingreso y eliminar registros de ingreso.
        /// </summary>
        public DbSet<CheckInLog> CheckInLogs { get; set; }

        /// <summary>
        /// DbSet que representa la colección de usuarios en la base de datos. Permite realizar operaciones CRUD sobre los usuarios, como crear nuevos usuarios, consultar usuarios 
        /// existentes, actualizar usuarios y eliminar usuarios.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        ///  Configura el modelo de datos para la base de datos, incluyendo la configuración de claves primarias, relaciones 
        /// entre entidades, índices y tokens de concurrencia.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => new { t.EventId, t.UserEmail })
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.BirthDate)
                .HasColumnType("date");
        }
    }
}
