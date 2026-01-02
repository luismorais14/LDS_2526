package pt.ipp.estg.bookflaz.ui.listarAnuncios.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import coil.compose.AsyncImage
import pt.ipp.estg.bookflaz.data.model.Anuncio
import pt.ipp.estg.bookflaz.utils.mapTipoAnuncio

/**
 * Componente visual que representa um cartão individual de um anúncio na lista de produtos.
 *
 * O cartão está dividido verticalmente em duas secções:
 * 1. Superior (60% da altura): Imagem do livro e etiqueta de preço.
 * 2. Inferior (40% da altura): Categoria, título e tipo de transação.
 *
 * @param anuncio O objeto de dados [Anuncio] contendo as informações a exibir.
 * @param navController Controlador de navegação utilizado para redirecionar o utilizador para os detalhes do produto ao clicar no cartão.
 */
@Composable
fun AnuncioCardItem(anuncio: Anuncio, navController: NavController) {
    Card(
        colors = CardDefaults.cardColors(containerColor = Color.White),
        elevation = CardDefaults.cardElevation(defaultElevation = 0.dp),
        border = BorderStroke(1.dp, Color(0xFFE0E0E0)),
        shape = RoundedCornerShape(12.dp),
        modifier = Modifier
            .fillMaxWidth()
            .height(280.dp)
            .clickable {
                navController.navigate("product/${anuncio.id}")
            }
            .testTag("anuncio_card")
    ) {
        Column {
            Box(
                modifier = Modifier
                    .weight(0.6f)
                    .fillMaxWidth()
                    .background(Color(0xFFEEEEEE))
            ) {
                AsyncImage(
                    model = anuncio.imagem ?: "https://placehold.co/400x600/png",
                    contentDescription = "Capa do livro ${anuncio.titulo}",
                    contentScale = ContentScale.Crop,
                    modifier = Modifier.fillMaxSize()
                )

                Surface(
                    color = MaterialTheme.colorScheme.primary,
                    shape = RoundedCornerShape(topStart = 0.dp, bottomEnd = 12.dp),
                    modifier = Modifier.align(Alignment.TopStart)
                ) {
                    Text(
                        text = "${anuncio.preco} €",
                        color = Color.White,
                        style = MaterialTheme.typography.labelMedium,
                        fontWeight = FontWeight.Bold,
                        modifier = Modifier.padding(horizontal = 10.dp, vertical = 6.dp)
                    )
                }
            }

            Column(
                modifier = Modifier
                    .weight(0.4f)
                    .padding(12.dp)
                    .fillMaxWidth()
            ) {
                Text(
                    text = anuncio.categoria.uppercase(),
                    style = MaterialTheme.typography.labelSmall,
                    color = Color.Gray,
                    maxLines = 1,
                    fontSize = 10.sp
                )

                Spacer(modifier = Modifier.height(4.dp))

                Text(
                    text = anuncio.titulo,
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold,
                    color = Color(0xFF1F1F1F),
                    maxLines = 2,
                    overflow = TextOverflow.Ellipsis,
                    lineHeight = 18.sp
                )

                Spacer(modifier = Modifier.weight(1f))

                Text(
                    text = mapTipoAnuncio(anuncio.tipoAnuncio),
                    style = MaterialTheme.typography.labelSmall,
                    color = MaterialTheme.colorScheme.primary,
                    fontWeight = FontWeight.Bold
                )
            }
        }
    }
}