using Microsoft.AspNetCore.Mvc;
using RetroagirNfEntrada.Models;
using RetroagirNfEntrada.Services;

namespace RetroagirNfEntrada.Controllers
{
    public class NotasFiscaisController : Controller
    {
        private readonly INotaFiscalService _notaFiscalService;
        private readonly IWebHostEnvironment _env;

        public NotasFiscaisController(INotaFiscalService notaFiscalService, IWebHostEnvironment env)
        {
            _notaFiscalService = notaFiscalService;
            _env = env;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new BuscaNotaFiscalViewModel();
            return View(model);
        }

        [HttpPost]
        [Route("NotasFiscais/Buscar")]
        public async Task<IActionResult> BuscarNotaFiscal([FromBody] BuscaNotaFiscalViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { 
                        success = false, 
                        message = "Dados de busca inválidos",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var notaFiscal = await _notaFiscalService.BuscarPorNumeroEFilialAsync(model.NumeroNfTransferencia, model.Filial, model.FilialOrigem);
                
                if (notaFiscal == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "Nota fiscal não encontrada" 
                    });
                }

                return Json(new { 
                    success = true, 
                    data = notaFiscal 
                });
            }
            catch (Exception ex)
            {
                return Json(new {
                    success = false,
                    message = _env.IsDevelopment() ? ex.Message : "Erro interno do servidor. Contate o suporte."
                });
            }
        }

        [HttpPost]
        [Route("NotasFiscais/Retroagir")]
        public async Task<IActionResult> RetroagirNota([FromBody] RetroacaoNotaViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { 
                        success = false, 
                        message = "Dados inválidos",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var sucesso = await _notaFiscalService.AtualizarDatasAsync(
                    model.NumeroNfTransferencia,
                    model.Filial,
                    model.FilialOrigem,
                    model.Emissao,
                    model.DataEntradaConferida
                );

                if (sucesso)
                {
                    return Json(new { 
                        success = true, 
                        message = "Nota fiscal retroagida com sucesso!" 
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "Erro ao retroagir a nota fiscal" 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new {
                    success = false,
                    message = _env.IsDevelopment() ? ex.Message : "Erro interno do servidor. Contate o suporte."
                });
            }
        }
    }
}