using RetroagirNfEntrada.Models;

namespace RetroagirNfEntrada.Services
{
    public interface INotaFiscalService
    {
        Task<NotaFiscal?> BuscarPorNumeroEFilialAsync(string numeroNfTransferencia, string filial, string filialOrigem);
        Task<bool> AtualizarDatasAsync(string numeroNfTransferencia, string filial, string filialOrigem, DateTime emissao, DateTime dataEntradaConferida);
    }
}