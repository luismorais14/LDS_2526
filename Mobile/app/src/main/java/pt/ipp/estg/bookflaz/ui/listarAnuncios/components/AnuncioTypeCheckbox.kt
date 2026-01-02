package pt.ipp.estg.bookflaz.ui.listarAnuncios.components

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.material3.Checkbox
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import pt.ipp.estg.bookflaz.utils.mapTipoAnuncio

/**
 * Componente de filtro que exibe uma lista de caixas de seleção (Checkboxes) para os tipos de anúncio.
 *
 * Permite ao utilizador filtrar os resultados por múltiplos tipos (ex: Venda e Troca simultaneamente).
 * Implementa o padrão de "State Hoisting", onde o estado é gerido pelo componente pai.
 *
 * @param selectedTypes Um [Set] de inteiros contendo os IDs dos tipos atualmente selecionados.
 * @param onTypeChanged Função de callback (lambda) invocada quando o utilizador clica numa opção. Recebe o ID do tipo alterado.
 */
@Composable
fun AnuncioTypeCheckbox(
    selectedTypes: Set<Int>,
    onTypeChanged: (Int) -> Unit
) {
    val possibleIds = listOf(0, 1, 2)

    Column(
        modifier = Modifier.padding(16.dp)
    ) {
        Text(
            "Tipo de Anúncio:",
            style = MaterialTheme.typography.titleMedium,
            fontWeight = FontWeight.Bold,
            modifier = Modifier.padding(bottom = 8.dp)
        )

        possibleIds.forEach { typeId ->
            val isChecked = selectedTypes.contains(typeId)
            val label = mapTipoAnuncio(typeId)

            Row(
                verticalAlignment = Alignment.CenterVertically,
                modifier = Modifier
                    .fillMaxWidth()
                    .clickable {
                        onTypeChanged(typeId)
                    }
            ) {
                Checkbox(
                    checked = isChecked,
                    onCheckedChange = {
                        onTypeChanged(typeId)
                    },
                    modifier = Modifier.testTag("${label}_checkbox")
                )
                Text(
                    text = label,
                    modifier = Modifier.testTag("${label}_text")
                )
            }
        }
    }
}