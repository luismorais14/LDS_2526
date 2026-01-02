package pt.ipp.estg.bookflaz.ui.verFavoritos

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import pt.ipp.estg.bookflaz.ui.verFavoritos.components.FavoritoCard
import pt.ipp.estg.bookflaz.viewmodel.FavoritosViewModel

/**
 * Ecrã responsável por listar os anúncios marcados como favoritos pelo utilizador.
 *
 * Este componente implementa o padrão "UI State", gerindo quatro estados visuais distintos:
 * 1. **Loading:** Mostra um indicador de progresso enquanto os dados são carregados.
 * 2. **Erro:** Exibe uma mensagem caso a requisição à API falhe.
 * 3. **Vazio:** Informa o utilizador caso não existam favoritos guardados.
 * 4. **Conteúdo:** Apresenta a lista de cartões [FavoritoCard] com os dados.
 *
 * @param viewModel O [FavoritosViewModel] que fornece o fluxo de dados (StateFlow) e operações de lógica de negócio.
 * @param onAnuncioClick Callback de navegação invocado quando o utilizador clica num cartão para ver detalhes. Recebe o ID do anúncio.
 */
@Composable
fun FavoritosScreen(
    viewModel: FavoritosViewModel = viewModel(),
    onAnuncioClick: (Int) -> Unit = {}
) {
    val favoritos by viewModel.favoritos.collectAsState()
    val isLoading by viewModel.isLoading.collectAsState()
    val erro by viewModel.erro.collectAsState()

    LaunchedEffect(Unit) {
        viewModel.carregarFavoritos()
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(12.dp)
    ) {
        Text(
            text = "Os teus favoritos",
            style = MaterialTheme.typography.titleLarge
        )

        Spacer(modifier = Modifier.height(8.dp))

        Box(
            modifier = Modifier
                .fillMaxSize()
                .testTag("nav_favoritos")
        ) {
            when {
                isLoading -> {
                    CircularProgressIndicator(
                        modifier = Modifier.align(Alignment.Center)
                    )
                }

                erro != null -> {
                    Text(
                        text = "Não foi possível carregar os favoritos.",
                        style = MaterialTheme.typography.bodyMedium,
                        modifier = Modifier.align(Alignment.Center)
                    )
                }

                favoritos.isEmpty() -> {
                    Text(
                        text = "Ainda não tens anúncios favoritos.",
                        style = MaterialTheme.typography.bodyMedium,
                        modifier = Modifier.align(Alignment.Center)
                    )
                }

                else -> {
                    LazyColumn(
                        modifier = Modifier.fillMaxSize(),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        items(favoritos) { anuncio ->
                            FavoritoCard(
                                favorito = anuncio,
                                onClick = { onAnuncioClick(anuncio.id) },
                                onToggleFavorite = {
                                    viewModel.toggleFavorito(anuncio.id)
                                }
                            )
                        }
                    }
                }
            }
        }
    }
}