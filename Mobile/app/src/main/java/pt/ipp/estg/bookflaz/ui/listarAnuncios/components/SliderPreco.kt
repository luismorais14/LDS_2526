package pt.ipp.estg.bookflaz.ui.listarAnuncios.components

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import kotlin.math.roundToInt

/**
 * Componente de interface que permite selecionar um intervalo de valores (preço) através
 * de uma barra deslizante (Slider) ou de campos de texto numéricos.
 *
 * Implementa sincronização bidirecional: ao mover o slider, os textos atualizam-se;
 * ao digitar valores, o slider move-se.
 *
 * @param currentRange O intervalo atual selecionado (ex: 10.0..50.0).
 * @param minAbsoluto O valor mínimo absoluto permitido para o filtro (início do slider).
 * @param maxAbsoluto O valor máximo absoluto permitido para o filtro (fim do slider).
 * @param onRangeChange Função de callback executada sempre que o intervalo é alterado (seja pelo slider ou texto).
 */
@Composable
fun SliderMinimal(
    currentRange: ClosedFloatingPointRange<Float>,
    minAbsoluto: Float,
    maxAbsoluto: Float,
    onRangeChange: (ClosedFloatingPointRange<Float>) -> Unit
) {
    var minText by remember { mutableStateOf(currentRange.start.roundToInt().toString()) }
    var maxText by remember { mutableStateOf(currentRange.endInclusive.roundToInt().toString()) }


    LaunchedEffect(currentRange) {
        if (minText.toFloatOrNull()?.roundToInt() != currentRange.start.roundToInt()) {
            minText = currentRange.start.roundToInt().toString()
        }
        if (maxText.toFloatOrNull()?.roundToInt() != currentRange.endInclusive.roundToInt()) {
            maxText = currentRange.endInclusive.roundToInt().toString()
        }
    }

    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(16.dp)
    ) {
        Text(
            text = "Intervalo de Preço",
            style = MaterialTheme.typography.titleMedium,
            fontWeight = FontWeight.Bold,
            modifier = Modifier.padding(bottom = 8.dp)
        )

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            OutlinedTextField(
                value = minText,
                onValueChange = { novoTexto ->
                    if (novoTexto.all { it.isDigit() }) {
                        minText = novoTexto

                        val novoMin = novoTexto.toFloatOrNull()
                        if (novoMin != null && novoMin >= minAbsoluto && novoMin <= currentRange.endInclusive) {
                            onRangeChange(novoMin..currentRange.endInclusive)
                        }
                    }
                },
                label = { Text("De (€)") },
                modifier = Modifier
                    .width(100.dp)
                    .testTag("input_price_min"),
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                singleLine = true
            )

            OutlinedTextField(
                value = maxText,
                onValueChange = { novoTexto ->
                    if (novoTexto.all { it.isDigit() }) {
                        maxText = novoTexto

                        val novoMax = novoTexto.toFloatOrNull()
                        if (novoMax != null && novoMax <= maxAbsoluto && novoMax >= currentRange.start) {
                            onRangeChange(currentRange.start..novoMax)
                        }
                    }
                },
                label = { Text("Até (€)") },
                modifier = Modifier
                    .width(100.dp)
                    .testTag("input_price_max"),
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                singleLine = true
            )
        }

        RangeSlider(
            value = currentRange,
            onValueChange = { range ->
                onRangeChange(range)
            },
            valueRange = minAbsoluto..maxAbsoluto,
            modifier = Modifier.padding(top = 16.dp)
        )

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Text(
                text = "${minAbsoluto.roundToInt()}€",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.outline
            )
            Text(
                text = "${maxAbsoluto.roundToInt()}€",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.outline
            )
        }
    }
}