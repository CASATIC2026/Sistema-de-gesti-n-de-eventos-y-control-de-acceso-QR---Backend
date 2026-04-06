using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Models;

namespace EventAccessControl.API.Data
{
    /// <summary>
    /// Contexto de la base de datos para la aplicación de control de acceso a eventos. Define las entidades Event, Ticket y CheckInLog, y configura el control de concurrencia 
    /// optimista para la entidad Ticket utilizando el campo Xmin. Este contexto se utiliza para interactuar con la base de datos y realizar operaciones CRUD sobre las entidades 
    /// definidas.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Constructor que recibe las opciones de configuración para el contexto de la base de datos. Estas opciones se utilizan para configurar la conexión a la base de datos y 
        /// otros aspectos relacionados con el contexto. 
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

        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Configura el modelo de la base de datos, incluyendo la configuración del control de concurrencia optimista para la entidad Ticket utilizando el campo Xmin. 
        /// Esto garantiza que en escenarios de alta concurrencia, como el registro simultáneo de tickets para el mismo evento y correo electrónico, solo una operación pueda 
        /// completar exitosamente, mientras que las demás recibirán un error de concurrencia, evitando así registros duplicados y garantizando la integridad de los datos.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>()
                .UseXminAsConcurrencyToken();
        }
    }
}
