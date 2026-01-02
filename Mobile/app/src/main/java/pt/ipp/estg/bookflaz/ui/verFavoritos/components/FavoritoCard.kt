package pt.ipp.estg.bookflaz.ui.verFavoritos.components

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage
import pt.ipp.estg.bookflaz.data.model.FavoritoResponse
import pt.ipp.estg.bookflaz.ui.verAnuncio.components.FavoriteCounterButton

/**
 * Cartão que representa um item individual na lista de favoritos.
 *
 * Apresenta um layout horizontal (Row) contendo a imagem do livro à esquerda,
 * informações textuais ao centro e o botão de ação (coração) à direita.
 *
 * @param favorito Objeto de dados [FavoritoResponse] contendo os detalhes do anúncio.
 * @param onClick Callback executado ao clicar no corpo do cartão (geralmente para navegar para detalhes).
 * @param onToggleFavorite Callback executado ao clicar no botão de coração (para remover dos favoritos).
 */
@Composable
fun FavoritoCard(
    favorito: FavoritoResponse,
    onClick: () -> Unit = {},
    onToggleFavorite: () -> Unit = {}
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onClick() }
            .testTag("favoritoCard"), // Tag para testes de UI
        shape = RoundedCornerShape(12.dp),
        elevation = CardDefaults.cardElevation(defaultElevation = 3.dp)
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {

            AsyncImage(
                model = favorito.getImagemUrl(),
                contentDescription = "Imagem do livro ${favorito.titulo}",
                contentScale = ContentScale.Crop,
                modifier = Modifier
                    .size(70.dp)
                    .clip(RoundedCornerShape(8.dp))
            )

            Spacer(modifier = Modifier.width(12.dp))

            Column(
                modifier = Modifier
                    .weight(1f)
                    .testTag("txtTitulo")
            ) {
                Text(
                    text = favorito.titulo,
                    style = MaterialTheme.typography.titleMedium,
                    maxLines = 2,
                    overflow = TextOverflow.Ellipsis
                )

                Spacer(modifier = Modifier.height(4.dp))

                Text(
                    text = String.format("%.2f €", favorito.preco),
                    style = MaterialTheme.typography.bodyMedium
                )
            }

            FavoriteCounterButton(
                count = 0,
                isFavorite = favorito.favorito,
                onToggleFavorite = onToggleFavorite,
                modifier = Modifier.testTag("btnFavorite")
            )
        }
    }
}