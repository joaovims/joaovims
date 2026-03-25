using RetroagirNfEntrada.Services;
using Microsoft.EntityFrameworkCore; // necessário para UseSqlServer
using RetroagirNfEntrada.Models;
using RetroagirNfEntrada.Context;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SeuDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=NotasFiscais}/{action=Index}/{id?}");

app.Run();


namespace RetroagirNfEntrada.Context
{
    public class SeuDbContext : DbContext
    {
        public SeuDbContext(DbContextOptions<SeuDbContext> options) : base(options) { }
        public DbSet<NotaFiscal> NotasFiscais { get; set; }
    }
}

// Crie o arquivo: Services/NotaFiscalService.cs

// Services/NotaFiscalService.cs


namespace RetroagirNfEntrada.Services
{
    public class NotaFiscalService : INotaFiscalService
    {
        private readonly IConfiguration _configuration;

        public NotaFiscalService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

public async Task<NotaFiscal?> BuscarPorNumeroEFilialAsync(int numeroNfTransferencia, int filial, int filialOrigem)
{
    try
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        var query = @"
            SELECT 
                numero_nf_transferencia,
                filial,
                filial_origem,
                emissao,
                data_entrada_conferida,
                qtde_total,
                valor_total
            FROM loja_entradas 
            WHERE numero_nf_transferencia = @numeroNfTransferencia 
              AND filial = @filial 
              AND filial_origem = @filialOrigem";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@numeroNfTransferencia", numeroNfTransferencia);
        command.Parameters.AddWithValue("@filial", filial);
        command.Parameters.AddWithValue("@filialOrigem", filialOrigem);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new NotaFiscal(
                reader.GetInt32(reader.GetOrdinal("numero_nf_transferencia")),
                reader.GetString(reader.GetOrdinal("filial")),
                reader.GetString(reader.GetOrdinal("filial_origem")),
                reader.GetDecimal(reader.GetOrdinal("valor_total")),
                reader.GetInt32(reader.GetOrdinal("qtde_total")),
                reader.GetDateTime(reader.GetOrdinal("emissao")),
                reader.GetDateTime(reader.GetOrdinal("data_entrada_conferida")));

        }

        return null;
    }
    catch (Exception ex)
    {
        throw new Exception($"Erro ao buscar nota fiscal: {ex.Message}", ex);
    }
}

        public async Task<bool> AtualizarDatasAsync(int notaFiscalId, DateTime emissao, DateTime dataEntradaConferida)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();

                var query = @"
                    UPDATE loja_entradas 
                    SET emissao = @emissao, 
                        data_entrada_conferida = @dataEntradaConferida 
                    WHERE numero_nf_transferencia = @numeronftransfernecia
                            and filial = @filial
                                and filial_origem = @filialorigem
                                    and entrada_conferida = 1
                                        and entrada_encerrada = 1";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@emissao", emissao);
                command.Parameters.AddWithValue("@dataEntradaConferida", dataEntradaConferida);
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log do erro aqui se necessário
                throw new Exception($"Erro ao atualizar nota fiscal: {ex.Message}", ex);
            }
        }
    }
}
