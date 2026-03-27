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

    // Auto-replicar data de emissão para data de entrada conferida
    $('#novaEmissao').on('change', function() {
        $('#novaDataEntrada').val($(this).val());
    });

    function buscarNotaFiscal() {
        const numeroNota = $('#NumeroNfTransferencia').val().trim();
        const filial = $('#Filial').val().trim();
        const filialOrigem = $('#FilialOrigem').val().trim();

        if (!numeroNota || !filial || !filialOrigem) {
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
                numeroNfTransferencia: numeroNota,
                filial: filial,
                filialOrigem: filialOrigem
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
        $('#dadosNumeroNfTransferencia').text(notaFiscal.numeroNfTransferencia);
        $('#dadosFilial').text(notaFiscal.filial);
        $('#dadosFilialOrigem').text(notaFiscal.filialOrigem);
        $('#dadosEmissao').text(formatarData(notaFiscal.emissao));
        $('#dadosDataEntrada').text(formatarData(notaFiscal.dataEntradaConferida));
        $('#dadosValorTotal').text(formatarMoeda(notaFiscal.valorTotal));
        $('#dadosQuantidadeTotal').text(notaFiscal.qtdeTotal);

        // Preenche os campos hidden no modal
        $('#hdnNumeroNf').val(notaFiscal.numeroNfTransferencia);
        $('#hdnFilial').val(notaFiscal.filial);
        $('#hdnFilialOrigem').val(notaFiscal.filialOrigem);

        $('#resultadoBusca').removeClass('d-none');
    }

    function retroagirNota() {
        const emissao = $('#novaEmissao').val();
        const dataEntrada = $('#novaDataEntrada').val();
        const hoje = new Date().toISOString().split('T')[0];

        if (emissao > hoje) {
            mostrarErroModal('A data de emissão não pode ser superior à data atual.');
            return;
        }

        if (dataEntrada > hoje) {
            mostrarErroModal('A data de entrada conferida não pode ser superior à data atual.');
            return;
        }

        if (emissao !== dataEntrada) {
            mostrarErroModal('A data de emissão e a data de entrada conferida devem ser iguais.');
            return;
        }

        const formData = {
            numeroNfTransferencia: $('#hdnNumeroNf').val(),
            filial: $('#hdnFilial').val(),
            filialOrigem: $('#hdnFilialOrigem').val(),
            emissao: emissao,
            dataEntradaConferida: dataEntrada
        };

        $('#btnConfirmarRetroacao').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processando...');

        $.ajax({
            url: '/NotasFiscais/Retroagir',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                $('#btnConfirmarRetroacao').prop('disabled', false).html('<i class="fas fa-clock me-2"></i> Confirmar Retroação');

                if (response.success) {
                    $('#modalRetroacao').modal('hide');
                    mostrarSucesso(response.message);
                    limparResultado();
                } else {
                    mostrarErroModal(response.message);
                }
            },
            error: function(xhr, status, error) {
                $('#btnConfirmarRetroacao').prop('disabled', false).html('<i class="fas fa-clock me-2"></i> Confirmar Retroação');
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
