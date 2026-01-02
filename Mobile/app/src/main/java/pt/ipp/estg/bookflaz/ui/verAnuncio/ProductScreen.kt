package pt.ipp.estg.bookflaz.ui.verAnuncio

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import pt.ipp.estg.bookflaz.ui.verAnuncio.components.BackButton
import pt.ipp.estg.bookflaz.ui.verAnuncio.components.BottomActionButtons
import pt.ipp.estg.bookflaz.ui.verAnuncio.components.FavoriteCounterButton
import pt.ipp.estg.bookflaz.ui.verAnuncio.components.ImagePager
import pt.ipp.estg.bookflaz.ui.verAnuncio.components.ProductDetailsSection
import pt.ipp.estg.bookflaz.utils.mapEstadoAnuncio
import pt.ipp.estg.bookflaz.utils.mapEstadoLivro
import pt.ipp.estg.bookflaz.utils.mapTipoAnuncio
import pt.ipp.estg.bookflaz.viewmodel.AnuncioViewModel

/**
 * Ecrã responsável por apresentar os detalhes de um anúncio,
 * permitindo ao utilizador visualizar imagens, informações detalhadas
 * e interagir com o anúncio.
 *
 * A lógica de carregamento do anúncio vive no [AnuncioViewModel],
 * sendo chamada automaticamente sempre que o ID fornecido se altera.
 *
 * @param anuncioId Identificador do anúncio a ser carregado e exibido.
 * @param viewModel ViewModel responsável por gerir os dados do anúncio e estado de favorito.
 */
@Composable
fun ProductScreen(
    anuncioId: Int = 3,
    onBack: () -> Unit = {},
    viewModel: AnuncioViewModel = viewModel()
) {
    val anuncio = viewModel.anuncio

    // Carrega o anúncio quando o ID muda
    LaunchedEffect(anuncioId) {
        viewModel.carregarAnuncioDetalhe(anuncioId);
    }

    val isFavorite = viewModel.isFavorite

    Scaffold(
        contentWindowInsets = WindowInsets(0),
        bottomBar = {
            BottomActionButtons(
                onProposal = { },
                onBuy = { }
            )
        }
    ) { padding ->

        // Estado de carregamento inicial
        if (anuncio == null) {
            Box(
                modifier = Modifier
                    .padding(padding)
                    .fillMaxSize(),
                contentAlignment = Alignment.Center
            ) {
                Text(
                    "A carregar…",
                    modifier = Modifier.testTag("loadingState")
                )
            }
            return@Scaffold
        }

        // Conteúdo do anúncio
        Column(
            modifier = Modifier
                .padding(padding)
                .fillMaxSize()
                .verticalScroll(rememberScrollState()),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {

            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(500.dp)
            ) {
                ImagePager(
                    images = anuncio.getListaImagensComUrl(),
                    modifier = Modifier.fillMaxSize()
                )

                BackButton(
                    onClick = onBack,
                    modifier = Modifier
                        .align(Alignment.TopStart)
                        .padding(16.dp)
                )

                FavoriteCounterButton(
                    count = anuncio.totalFavoritos,
                    isFavorite = isFavorite,
                    onToggleFavorite = { viewModel.toggleFavorito(anuncio.id) },
                    modifier = Modifier
                        .testTag("btnFavorite")
                        .align(Alignment.BottomEnd)
                        .padding(16.dp)
                )
            }

            Spacer(modifier = Modifier.height(10.dp))

            // Secção de propriedades detalhadas
            ProductDetailsSection(
                details = listOf(
                    "Título" to anuncio.titulo,
                    "Autor" to anuncio.autor,
                    "Estado do Livro" to mapEstadoLivro(anuncio.estadoLivro),
                    "Estado do Anúncio" to mapEstadoAnuncio(anuncio.estadoAnuncio),
                    "Tipo de Anúncio" to mapTipoAnuncio(anuncio.tipoAnuncio),
                    "Vendedor" to anuncio.nomeVendedor,
                    "Categoria" to anuncio.categoria,
                    "Preço" to "${anuncio.preco} €",
                    "Data Publicação" to formatarData(anuncio.dataCriacao)
                )
            )
        }
    }
}

/**
 * Formata a data recebida da API, removendo a parte das horas.
 *
 * @param data String com timestamp vindo da API.
 * @return Data formatada apenas com a porção AAAA-MM-DD.
 */
private fun formatarData(data: String): String {
    return data.split("T")[0]
}