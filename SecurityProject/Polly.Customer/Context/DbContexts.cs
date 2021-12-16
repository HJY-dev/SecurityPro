using Microsoft.EntityFrameworkCore;
using Polly.Customer.Entity;
using System;

namespace Polly.Customer.Context
{
    public class DbContexts:DbContext
    {
        public DbContexts(DbContextOptions<DbContexts> options)
           : base(options)
        {
        }

        public DbSet<Customers> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            //只输出SQL
            optionsBuilder.LogTo(data => { Console.WriteLine(data); }, new string[] { "Microsoft.EntityFrameworkCore.Database.Command" }, Microsoft.Extensions.Logging.LogLevel.Information, Microsoft.EntityFrameworkCore.Diagnostics.DbContextLoggerOptions.Category);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customers>(entity => {
                entity.ToTable("Customers");

            });
        }

    }
}
