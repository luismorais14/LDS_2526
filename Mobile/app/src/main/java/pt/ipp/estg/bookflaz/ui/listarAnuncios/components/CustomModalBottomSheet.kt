package pt.ipp.estg.bookflaz.ui.listarAnuncios.components

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.unit.dp
import pt.ipp.estg.bookflaz.viewmodel.AnuncioViewModel

/**
 * Composable que exibe um painel deslizante (Modal Bottom Sheet) contendo os controlos de filtragem.
 *
 * Este painel sobrepõe-se ao conteúdo principal e permite ao utilizador ajustar:
 * 1. O intervalo de preços (via [SliderMinimal]).
 * 2. O tipo de anúncio (via [AnuncioTypeCheckbox]).
 *
 * @param onDismiss Função de callback executada quando o painel deve ser fechado (ao clicar no botão "Aplicar" ou na área escura de fundo).
 * @param viewModel O [AnuncioViewModel] que gere o estado dos filtros. O componente lê e escreve diretamente neste ViewModel.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SlideUpScreen(
    onDismiss: () -> Unit,
    viewModel: AnuncioViewModel
) {
    val sheetState = rememberModalBottomSheetState(
        skipPartiallyExpanded = true
    )

    val priceRange = viewModel.priceRange
    val minAbsoluto = viewModel.minPrecoAbsoluto
    val maxAbsoluto = viewModel.maxPrecoAbsoluto
    val tiposSelecionados = viewModel.tiposSelecionados

    ModalBottomSheet(
        onDismissRequest = { onDismiss() },
        sheetState = sheetState
    ) {
        Column(modifier = Modifier.padding(bottom = 32.dp)) {

            SliderMinimal(
                currentRange = priceRange,
                minAbsoluto = minAbsoluto,
                maxAbsoluto = maxAbsoluto,
                onRangeChange = { novoRange ->
                    viewModel.onPriceRangeChange(novoRange)
                }
            )

            HorizontalDivider()

            AnuncioTypeCheckbox(
                selectedTypes = tiposSelecionados,
                onTypeChanged = { id -> viewModel.toggleTipoFiltro(id) }
            )

            Button(
                onClick = {
                    onDismiss()
                },
                modifier = Modifier
                    .align(Alignment.CenterHorizontally)
                    .testTag("btn_apply_filters")
            ) {
                Text("Aplicar")
            }
        }
    }
}