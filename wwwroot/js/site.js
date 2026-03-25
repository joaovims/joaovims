$(document).ready(function() {
    let notaFiscalAtual = null;

    // Submissão do formulário de busca
    $('#formBusca').on('submit', function(e) {
        e.preventDefault();
        buscarNotaFiscal();
    });

    // Submissão do formulário de retroação
    $('#formRetroacao').on('submit', function(e) {
        e.preventDefault();
        retroagirNota();
    });

    function buscarNotaFiscal() {
        const numeroNota = $('#NumeroNota').val().trim();
        const filial = $('#Filial').val();

        if (!numeroNota || !filial) {
            mostrarErro('Preencha todos os campos de busca.');
            return;
        }

        mostrarLoading();
        limparMensagens();

        $.ajax({
            url: '/NotasFiscais/Buscar',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                numeroNota: numeroNota,
                filial: parseInt(filial)
            }),
            success: function(response) {
                ocultarLoading();
                
                if (response.success) {
                    notaFiscalAtual = response.data;
                    exibirResultado(response.data);
                } else {
                    mostrarErro(response.message);
                }
            },
            error: function(xhr, status, error) {
                ocultarLoading();
                mostrarErro('Erro ao buscar nota fiscal: ' + error);
            }
        });
    }

    function exibirResultado(notaFiscal) {
        $('#dadosNumeroNota').text(notaFiscal.numeroNota);
        $('#dadosFilial').text(notaFiscal.filial);
        $('#dadosFornecedor').text(notaFiscal.fornecedor);
        $('#dadosEmissao').text(formatarData(notaFiscal.emissao));
        $('#dadosDataEntrada').text(formatarData(notaFiscal.dataEntradaConferida));
        $('#dadosValorTotal').text(formatarMoeda(notaFiscal.valorTotal));
        $('#dadosStatus').text(notaFiscal.status);
        
        // Preenche o ID no modal
        $('#notaFiscalId').val(notaFiscal.id);
        
        $('#resultadoBusca').removeClass('d-none');
    }

    function retroagirNota() {
        const formData = {
            notaFiscalId: parseInt($('#notaFiscalId').val()),
            emissao: $('#novaEmissao').val(),
            dataEntradaConferida: $('#novaDataEntrada').val()
        };

        $('#btnConfirmarRetroacao').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processando...');

        $.ajax({
            url: '/NotasFiscais/Retroagir',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                $('#btnConfirmarRetroacao').prop('disabled', false).html('<i class="fas fa-clock"></i> Confirmar Retroação');
                
                if (response.success) {
                    $('#modalRetroacao').modal('hide');
                    mostrarSucesso(response.message);
                    limparResultado();
                } else {
                    mostrarErroModal(response.message);
                }
            },
            error: function(xhr, status, error) {
                $('#btnConfirmarRetroacao').prop('disabled', false).html('<i class="fas fa-clock"></i> Confirmar Retroação');
                mostrarErroModal('Erro ao retroagir nota fiscal: ' + error);
            }
        });
    }

    // Funções auxiliares
    function mostrarLoading() {
        $('#loading').removeClass('d-none');
        $('#resultadoBusca').addClass('d-none');
    }

    function ocultarLoading() {
        $('#loading').addClass('d-none');
    }

    function mostrarErro(mensagem) {
        $('#mensagemErro').removeClass('d-none').text(mensagem);
        setTimeout(() => $('#mensagemErro').addClass('d-none'), 5000);
    }

    function mostrarSucesso(mensagem) {
        toastr.success(mensagem);
    }

    function mostrarErroModal(mensagem) {
        toastr.error(mensagem);
    }

    function limparMensagens() {
        $('#mensagemErro').addClass('d-none');
    }

    function limparResultado() {
        $('#resultadoBusca').addClass('d-none');
        $('#formBusca')[0].reset();
        notaFiscalAtual = null;
    }

    function formatarData(dataString) {
        const data = new Date(dataString);
        return data.toLocaleDateString('pt-BR');
    }

    function formatarMoeda(valor) {
        return new Intl.NumberFormat('pt-BR', { 
            style: 'currency', 
            currency: 'BRL' 
        }).format(valor);
    }

    // Limpar formulário do modal ao fechar
    $('#modalRetroacao').on('hidden.bs.modal', function() {
        $('#formRetroacao')[0].reset();
    });
});

window.limparResultado = function() {
    $('#resultadoBusca').addClass('d-none');
    $('#formBusca')[0].reset();
};