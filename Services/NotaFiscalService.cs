using Microsoft.Data.SqlClient;
using RetroagirNfEntrada.Models;

namespace RetroagirNfEntrada.Services
{
    public class NotaFiscalService : INotaFiscalService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotaFiscalService> _logger;

        public NotaFiscalService(IConfiguration configuration, ILogger<NotaFiscalService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<NotaFiscal?> BuscarPorNumeroEFilialAsync(int numeroNfTransferencia, string filial, string filialOrigem)
        {
            try
            {
                _logger.LogInformation("Buscando NF {Numero} filial {Filial} filialOrigem {FilialOrigem}",
                    numeroNfTransferencia, filial, filialOrigem);

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
                    int ordNumero     = reader.GetOrdinal("numero_nf_transferencia");
                    int ordFilial     = reader.GetOrdinal("filial");
                    int ordOrigem     = reader.GetOrdinal("filial_origem");
                    int ordValor      = reader.GetOrdinal("valor_total");
                    int ordQtde       = reader.GetOrdinal("qtde_total");
                    int ordEmissao    = reader.GetOrdinal("emissao");
                    int ordEntrada    = reader.GetOrdinal("data_entrada_conferida");

                    return new NotaFiscal(
                        reader.GetInt32(ordNumero),
                        reader.IsDBNull(ordFilial)  ? string.Empty : reader.GetString(ordFilial),
                        reader.IsDBNull(ordOrigem)  ? string.Empty : reader.GetString(ordOrigem),
                        reader.IsDBNull(ordValor)   ? 0m           : reader.GetDecimal(ordValor),
                        reader.IsDBNull(ordQtde)    ? 0            : reader.GetInt32(ordQtde),
                        reader.IsDBNull(ordEmissao) ? DateTime.MinValue : reader.GetDateTime(ordEmissao),
                        reader.IsDBNull(ordEntrada) ? DateTime.MinValue : reader.GetDateTime(ordEntrada));
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar NF {Numero}", numeroNfTransferencia);
                throw new Exception($"Erro ao buscar nota fiscal: {ex.Message}", ex);
            }
        }

        public async Task<bool> AtualizarDatasAsync(
            int numeroNfTransferencia, string filial, string filialOrigem,
            DateTime emissao, DateTime dataEntradaConferida)
        {
            try
            {
                _logger.LogInformation("Retroagindo NF {Numero} filial {Filial}", numeroNfTransferencia, filial);

                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();

                var query = @"
                    UPDATE loja_entradas
                    SET emissao = @emissao,
                        data_entrada_conferida = @dataEntradaConferida
                    WHERE numero_nf_transferencia = @numeronftransferencia
                      AND filial = @filial
                      AND filial_origem = @filialorigem
                      AND entrada_conferida = 1
                      AND entrada_encerrada = 1";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@emissao", emissao);
                command.Parameters.AddWithValue("@dataEntradaConferida", dataEntradaConferida);
                command.Parameters.AddWithValue("@numeronftransferencia", numeroNfTransferencia);
                command.Parameters.AddWithValue("@filial", filial);
                command.Parameters.AddWithValue("@filialorigem", filialOrigem);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                _logger.LogInformation("NF {Numero} retroagida, {Rows} linha(s) afetada(s)", numeroNfTransferencia, rowsAffected);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao retroagir NF {Numero}", numeroNfTransferencia);
                throw new Exception($"Erro ao atualizar nota fiscal: {ex.Message}", ex);
            }
        }
    }
}
