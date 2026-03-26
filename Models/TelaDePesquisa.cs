using System.ComponentModel.DataAnnotations;

namespace RetroagirNfEntrada.Models
{
    public class NotaFiscal
    {
        public NotaFiscal() { }

        public NotaFiscal(int numeroNfTransferencia, string filial, string filialOrigem, decimal valorTotal, int qtdeTotal, DateTime emissao, DateTime dataEntradaConferencia)
        {
            NumeroNfTransferencia = numeroNfTransferencia;
            Filial = filial;
            FilialOrigem = filialOrigem;
            ValorTotal = valorTotal;
            QtdeTotal = qtdeTotal;
            DataEntradaConferida = dataEntradaConferencia;
            Emissao = emissao;
        }

        public int Id { get; set; }

        [Display(Name = "Número da Nota")]
        public int NumeroNfTransferencia { get; set; }

        [Display(Name = "Filial")]
        public string Filial { get; set; } = string.Empty;

        [Display(Name = "Filial Origem")]
        public string FilialOrigem { get; set; } = string.Empty;

        [Display(Name = "Emissão")]
        [DataType(DataType.Date)]
        public DateTime Emissao { get; set; }

        [Display(Name = "Data Entrada Conferida")]
        [DataType(DataType.Date)]
        public DateTime DataEntradaConferida { get; set; }

        [Display(Name = "Valor Total")]
        [DataType(DataType.Currency)]
        public decimal ValorTotal { get; set; }

        [Display(Name = "Quantidade Total")]
        public int QtdeTotal { get; set; }
    }

    public class BuscaNotaFiscalViewModel
    {
        public BuscaNotaFiscalViewModel() { }

        public BuscaNotaFiscalViewModel(int numeroNfTransferencia)
        {
            NumeroNfTransferencia = numeroNfTransferencia;
        }

        [Required(ErrorMessage = "O número da nota fiscal é obrigatório")]
        [Display(Name = "Número da Nota Fiscal")]
        public int NumeroNfTransferencia { get; set; }

        [Required(ErrorMessage = "A filial é obrigatória")]
        [Display(Name = "Filial")]
        public string Filial { get; set; } = string.Empty;

        [Required(ErrorMessage = "A filial de origem é obrigatória")]
        [Display(Name = "Filial de Origem")]
        public string FilialOrigem { get; set; } = string.Empty;
    }

    public class RetroacaoNotaViewModel
    {
        [Required(ErrorMessage = "O número da nota fiscal é obrigatório")]
        public int NumeroNfTransferencia { get; set; }

        [Required(ErrorMessage = "A filial é obrigatória")]
        public string Filial { get; set; } = string.Empty;

        [Required(ErrorMessage = "A filial de origem é obrigatória")]
        public string FilialOrigem { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de emissão é obrigatória")]
        [Display(Name = "Data de Emissão")]
        [DataType(DataType.Date)]
        public DateTime Emissao { get; set; }

        [Required(ErrorMessage = "A data de entrada conferida é obrigatória")]
        [Display(Name = "Data Entrada Conferida")]
        [DataType(DataType.Date)]
        public DateTime DataEntradaConferida { get; set; }
    }
}
