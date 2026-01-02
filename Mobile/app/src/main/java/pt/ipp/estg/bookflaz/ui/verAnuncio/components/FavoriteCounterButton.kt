package pt.ipp.estg.bookflaz.ui.verAnuncio.components

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.outlined.FavoriteBorder
import androidx.compose.material.icons.filled.Favorite
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp

/**
 * Botão que exibe o ícone de favorito e o número de utilizadores que
 * adicionaram o anúncio, a favorito. O ícone e o estilo visual mudam dinamicamente
 * conforme o estado atual de favorito.
 *
 * Este componente é interativo: ao ser clicado, executa o callback
 * [onToggleFavorite], permitindo atualizar o estado da UI e da API.
 *
 * É utilizado no ecrã de detalhes do produto.
 *
 * @param count Quantidade total de favoritos atribuídos ao anúncio.
 * @param isFavorite Indica se o utilizador atual marcou este anúncio como favorito.
 * @param modifier Modificador para personalização externa (posição/tamanho).
 * @param onToggleFavorite Ação executada ao clicar no botão (callback).
 */
@Composable
fun FavoriteCounterButton(
    count: Int,
    isFavorite: Boolean,
    modifier: Modifier = Modifier,
    onToggleFavorite: () -> Unit = {}
) {

    val background = if (isFavorite) Color.Red.copy(alpha = 0.85f) else Color.Black.copy(alpha = 0.75f)
    val icon = if (isFavorite) Icons.Filled.Favorite else Icons.Outlined.FavoriteBorder

    Row(
        modifier = modifier
            .clip(RoundedCornerShape(50))
            .background(background)
            .clickable { onToggleFavorite() }
            .padding(horizontal = 12.dp, vertical = 6.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {

        Icon(
            imageVector = icon,
            contentDescription = "Favorite",
            tint = Color.White
        )

        if (count != 0) {
            Spacer(modifier = Modifier.width(6.dp))

            Text(
                text = count.toString(),
                color = Color.White
            )
        }
    }
}