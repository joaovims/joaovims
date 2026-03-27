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

        public async Task<NotaFiscal?> BuscarPorNumeroEFilialAsync(string numeroNfTransferencia, string filial, string filialOrigem)
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
                    return new NotaFiscal(
                        reader["numero_nf_transferencia"] == DBNull.Value ? string.Empty  : Convert.ToString(reader["numero_nf_transferencia"])!,
                        reader["filial"]                  == DBNull.Value ? string.Empty  : Convert.ToString(reader["filial"])!,
                        reader["filial_origem"]           == DBNull.Value ? string.Empty  : Convert.ToString(reader["filial_origem"])!,
                        reader["valor_total"]             == DBNull.Value ? 0m            : Convert.ToDecimal(reader["valor_total"]),
                        reader["qtde_total"]              == DBNull.Value ? 0             : Convert.ToInt32(reader["qtde_total"]),
                        reader["emissao"]                 == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["emissao"]),
                        reader["data_entrada_conferida"]  == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["data_entrada_conferida"]));
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
            string numeroNfTransferencia, string filial, string filialOrigem,
            DateTime emissao, DateTime dataEntradaConferida)
        {
            if (emissao.Date != dataEntradaConferida.Date)
                throw new Exception("A data de emissão e a data de entrada conferida devem ser iguais.");

            try
            {
                _logger.LogInformation("Retroagindo NF {Numero} filial {Filial}", numeroNfTransferencia, filial);

                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();

                // Busca as datas atuais no banco antes de validar
                var selectQuery = @"
                    SELECT emissao, data_entrada_conferida
                    FROM loja_entradas
                    WHERE numero_nf_transferencia = @numeronftransferencia
                      AND filial = @filial
                      AND filial_origem = @filialorigem
                      AND entrada_conferida = 1
                      AND entrada_encerrada = 1";

                using var selectCmd = new SqlCommand(selectQuery, connection);
                selectCmd.Parameters.AddWithValue("@numeronftransferencia", numeroNfTransferencia);
                selectCmd.Parameters.AddWithValue("@filial", filial);
                selectCmd.Parameters.AddWithValue("@filialorigem", filialOrigem);

                using var reader = await selectCmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    throw new Exception("Nota fiscal não encontrada ou não elegível para retroação.");

                var emissaoAtual    = reader["emissao"]                == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(reader["emissao"]);
                var entradaAtual    = reader["data_entrada_conferida"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(reader["data_entrada_conferida"]);
                reader.Close();

                if (emissao.Date > emissaoAtual.Date)
                    throw new Exception($"A nova data de emissão ({emissao:dd/MM/yyyy}) não pode ser maior que a data atual da nota ({emissaoAtual:dd/MM/yyyy}).");

                if (dataEntradaConferida.Date > entradaAtual.Date)
                    throw new Exception($"A nova data de entrada conferida ({dataEntradaConferida:dd/MM/yyyy}) não pode ser maior que a data atual da nota ({entradaAtual:dd/MM/yyyy}).");

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
