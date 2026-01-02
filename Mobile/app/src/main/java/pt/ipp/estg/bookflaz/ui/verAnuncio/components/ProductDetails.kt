package pt.ipp.estg.bookflaz.ui.verAnuncio.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Divider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.platform.testTag


/**
 * Secção que apresenta os detalhes de um anúncio em formato
 * de lista com título e atributos devidamente alinhados, separados por
 * divisores.
 *
 * Os dados são recebidos como uma lista de pares String/String, onde
 * cada item representa uma linha da tabela de informação.
 *
 * @param details Lista de pares (Label, Valor) a serem exibidos verticalmente.
 * @param modifier Permite personalização externa do layout via Compose.
 */
@Composable
fun ProductDetailsSection(
    details: List<Pair<String, String>>,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier
            .fillMaxWidth()
            .clip(RoundedCornerShape(20.dp))
            .background(MaterialTheme.colorScheme.surface)
            .padding(20.dp)
    ) {

        Text(
            text = "Detalhes do Produto",
            style = MaterialTheme.typography.titleLarge.copy(fontWeight = FontWeight.Bold),
            color = MaterialTheme.colorScheme.onSurface
        )

        Spacer(modifier = Modifier.height(16.dp))

        details.forEachIndexed { index, detail ->
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .then(
                        if (detail.first == "Título") Modifier.testTag("txtTitulo")
                        else Modifier
                    ),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Text(
                    text = detail.first,
                    style = MaterialTheme.typography.bodyMedium.copy(
                        fontWeight = FontWeight.Normal,
                        color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
                    )
                )

                Text(
                    text = detail.second,
                    style = MaterialTheme.typography.bodyMedium.copy(
                        fontWeight = FontWeight.SemiBold,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                )
            }

            if (index < details.size - 1) {
                Spacer(modifier = Modifier.height(12.dp))
                Divider(
                    modifier = Modifier.fillMaxWidth(),
                    color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.08f)
                )
                Spacer(modifier = Modifier.height(12.dp))
            }
        }
    }
}
