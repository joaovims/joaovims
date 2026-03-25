using RetroagirNfEntrada.Models;

namespace RetroagirNfEntrada.Services
{
    public interface INotaFiscalService
    {
        Task<NotaFiscal?> BuscarPorNumeroEFilialAsync(int numeroNfTransferencia, int filial, int filialOrigem);
        Task<bool> AtualizarDatasAsync(int notaFiscalId, DateTime emissao, DateTime dataEntradaConferida);
    }
}