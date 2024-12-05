using Microsoft.EntityFrameworkCore;
using WebCatalogo.Models;

namespace WebCatalogo.Context
{
    public class AppDbConstext : DbContext
    {

        //construtor 
        public AppDbConstext(DbContextOptions<AppDbConstext> options) : base(options) 
        { }


        public DbSet<Produto>? Produtos { get; set; }
        public DbSet<Categoria>? Categorias { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            //Chave primaria
            mb.Entity<Categoria>().HasKey(C => C.Id_categoria);

            //encadeamento das chamadas, coocando apenas um '.'
            mb.Entity<Categoria>().Property(c => c.Nome).HasMaxLength(100).IsRequired();
            mb.Entity<Categoria>().Property(c => c.Descricao).HasMaxLength(100).IsRequired();

            //Produto
            mb.Entity<Produto>().HasKey(c =>  c.Id_produto);
            mb.Entity<Produto>().Property(c => c.Nome).HasMaxLength(150);
            mb.Entity<Produto>().Property(c => c.Descricao).HasMaxLength(100);
            mb.Entity<Produto>().Property(c => c.Precco).HasPrecision(14, 2);

            //relacionameno
            mb.Entity<Produto>().HasOne<Categoria>(c => c.Categoria).WithMany(p => p.Produtos).HasForeignKey(c => c.Categoria);

        }

    }
}
