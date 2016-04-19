using Microsoft.Data.Entity;

namespace CustomExceptionHandler.Models
{
    /// <summary>
    /// The DbContext used to log the exceptions in a SQL Server database.
    /// </summary>
    public class ExceptionLogDbContext : DbContext
    {
        private string conn;

        /// <summary>
        /// Sets the connection string passed in by the parent application and creates the database if it doesn't exist.
        /// </summary>
        /// <param name="conn">A SQL Server connection to the database that has the ExecptionLog table.</param>
        public ExceptionLogDbContext(string conn)
        {
            this.conn = conn;

            //*POI
            //Creates the database and ExecptionLog table.  This only works if the database in the connection string does not exist.
            //
            //As of EF version 7.0.0-rc1-final, You cannot programmatically create migrations.  And since the connection string
            //is passed in during runtime, you cannot use the dnx ef command "migrations add" to create a migration.  If using
            //an existing database, the ExecptionLog table must be created by running the ExceptionLog.sql file in he root of this
            //project against the database.
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this.conn);
        }

        public DbSet<ExceptionLog> ExceptionLog { get; set; }
    }
}
