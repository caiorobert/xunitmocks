using Alura.CoisasAFazer.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Alura.CoisasAFazer.Infrastructure
{
    public class DbTarefasContext : DbContext
    {
        public DbTarefasContext(DbContextOptions options) : base(options) { }

        public DbTarefasContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            optionsBuilder.UseSqlServer(@"Data Source=WKSJUN000348\SQLEXPRESS;Initial Catalog=DbTarefas;Trusted_Connection=true;");
        }

        public DbSet<Tarefa> Tarefas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
    }
}
