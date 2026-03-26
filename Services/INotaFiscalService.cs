using RetroagirNfEntrada.Models;

namespace RetroagirNfEntrada.Services
{
    public interface INotaFiscalService
    {
        Task<NotaFiscal?> BuscarPorNumeroEFilialAsync(int numeroNfTransferencia, string filial, string filialOrigem);
        Task<bool> AtualizarDatasAsync(int numeroNfTransferencia, string filial, string filialOrigem, DateTime emissao, DateTime dataEntradaConferida);
    }
}