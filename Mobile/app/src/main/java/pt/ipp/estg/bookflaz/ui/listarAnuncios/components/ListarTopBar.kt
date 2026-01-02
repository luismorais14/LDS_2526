package pt.ipp.estg.bookflaz.ui.listarAnuncios.components

import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Search
import androidx.compose.material.icons.filled.Tune
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

/**
 * Barra de pesquisa personalizada.
 *
 * @param texto Texto atual na barra de pesquisa.
 * @param onTextoChange Função a ser executada quando o texto na barra de pesquisa é alterado.
 * @param onLimpar Função a ser executada quando o botão de limpar é clicado.
 * @param onFilterClick Função a ser executada quando o botão de filtros é clicado.
 * @receiver Conteúdo da barra de pesquisa.
 */
@Composable
fun TopSearchBar(
    texto: String,
    onTextoChange: (String) -> Unit,
    onLimpar: () -> Unit,
    onFilterClick: () -> Unit
) {
    Surface(
        shadowElevation = 4.dp,
        color = Color.White
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp, 8.dp, 16.dp, 12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            OutlinedTextField(
                value = texto,
                onValueChange = onTextoChange,
                modifier = Modifier
                    .weight(1f)
                    .height(50.dp).testTag("input_search"),
                placeholder = { Text("O que procuras?", fontSize = 14.sp) },
                leadingIcon = { Icon(Icons.Default.Search, contentDescription = null, tint = Color.Gray) },
                trailingIcon = {
                    if (texto.isNotEmpty()) {
                        IconButton(onClick = onLimpar) {
                            Icon(Icons.Default.Close, contentDescription = "Limpar")
                        }
                    }
                },
                shape = RoundedCornerShape(25.dp),
                colors = OutlinedTextFieldDefaults.colors(
                    unfocusedContainerColor = Color(0xFFF0F2F5),
                    focusedContainerColor = Color.White,
                    unfocusedBorderColor = Color.Transparent,
                    focusedBorderColor = MaterialTheme.colorScheme.primary
                ),
                singleLine = true
            )

            Spacer(modifier = Modifier.width(8.dp))

            IconButton(onClick = {
                onFilterClick()
            },
                modifier = Modifier.testTag("btn_open_filters")
            ) {
                Icon(Icons.Default.Tune, contentDescription = "Filtros", tint = MaterialTheme.colorScheme.primary)
            }
        }
    }
}