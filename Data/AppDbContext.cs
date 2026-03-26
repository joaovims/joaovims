using Microsoft.EntityFrameworkCore;
using RetroagirNfEntrada.Models;

namespace RetroagirNfEntrada.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<NotaFiscal> NotasFiscais { get; set; }
    }
}
