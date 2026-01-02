package pt.ipp.estg.bookflaz.ui.verAnuncio.components

import androidx.compose.foundation.ExperimentalFoundationApi
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.pager.HorizontalPager
import androidx.compose.foundation.pager.rememberPagerState
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage

/**
 * Componente de imagens que permite ao utilizador navegar
 * horizontalmente pelas imagens do produto através de um sistema de
 * "swipe" (pager).
 *
 * Cada imagem é carregada dinamicamente a partir de um URL externo.
 *
 * @param images Lista de URLs das imagens a exibir.
 * @param modifier Permite personalização do layout externo do componente.
 */
@OptIn(ExperimentalFoundationApi::class)
@Composable
fun ImagePager(
    images: List<String>,
    modifier: Modifier = Modifier
) {
    val pagerState = rememberPagerState(pageCount = { images.size })

    Column(
        modifier = modifier
            .fillMaxWidth()
            .testTag("imagePager"),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {

        HorizontalPager(
            state = pagerState,
            modifier = Modifier
                .fillMaxWidth()
                .height(500.dp)
        ) { page ->
            AsyncImage(
                model = images[page],
                contentDescription = "Product Image",
                contentScale = ContentScale.Crop,
                modifier = Modifier
                    .fillMaxSize()
                    .testTag("imageItem_$page")
            )
        }

        Spacer(modifier = Modifier.height(8.dp))
    }
}