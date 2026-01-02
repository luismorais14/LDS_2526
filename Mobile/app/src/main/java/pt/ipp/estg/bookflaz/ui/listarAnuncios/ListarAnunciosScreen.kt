package pt.ipp.estg.bookflaz.ui.listarAnuncios

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.grid.*
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import pt.ipp.estg.bookflaz.ui.listarAnuncios.components.AnuncioCardItem
import pt.ipp.estg.bookflaz.ui.listarAnuncios.components.SlideUpScreen
import pt.ipp.estg.bookflaz.ui.listarAnuncios.components.TopSearchBar
import pt.ipp.estg.bookflaz.viewmodel.AnuncioViewModel
import pt.ipp.estg.bookflaz.viewmodel.CategoriaViewModel

/**
 * Ecrã principal de listagem de anúncios (Feed de Produtos).
 *
 * Este ecrã agrega várias funcionalidades complexas:
 * 1. Pesquisa por texto (via TopBar).
 * 2. Filtragem horizontal por categorias (Chips).
 * 3. Filtragem avançada por preço/tipo (BottomSheet).
 * 4. Listagem em grelha com "Infinite Scroll" (carregamento automático ao chegar ao fundo).
 *
 * @param onBack Função de callback para navegação de retorno (atualmente não utilizada no Scaffold).
 * @param viewModel O [AnuncioViewModel] que contém a lista de anúncios, estados de loading e lógica de paginação.
 * @param categoriaViewModel O [CategoriaViewModel] responsável por fornecer a lista de categorias para os filtros.
 * @param navController O controlador de navegação para redirecionar para os detalhes do anúncio.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ListarAnunciosScreen(
    onBack: () -> Unit = {},
    viewModel: AnuncioViewModel = viewModel(),
    categoriaViewModel: CategoriaViewModel = viewModel(),
    navController: NavController
) {
    val scrollState = rememberLazyGridState()

    var showBottomSheet by remember { mutableStateOf(false) }

    val categoriasState = categoriaViewModel.categorias
    val categorias = remember(categoriasState) { listOf("Todas") + categoriasState.map { it.nome } }

    LaunchedEffect(Unit) {
        viewModel.carregarAnunciosIniciais()
        categoriaViewModel.carregarCategorias()
    }

    val deveCarregarMais by remember {
        derivedStateOf {
            val layoutInfo = scrollState.layoutInfo
            val totalItems = layoutInfo.totalItemsCount
            val lastVisibleItem = layoutInfo.visibleItemsInfo.lastOrNull()?.index ?: 0

            totalItems > 0 && lastVisibleItem >= (totalItems - 4)
        }
    }

    LaunchedEffect(deveCarregarMais) {
        if (deveCarregarMais) viewModel.carregarProximoLote()
    }

    Scaffold(
        contentWindowInsets = WindowInsets(0),
        topBar = {
            TopSearchBar(
                texto = viewModel.textoPesquisa,
                onTextoChange = { viewModel.onTextoPesquisaChange(it) },
                onLimpar = { viewModel.onLimparPesquisa() },
                onFilterClick = { showBottomSheet = true },
            )
        }
    ) { padding ->

        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .background(Color(0xFFF4F6F8))
        ) {
            Column(modifier = Modifier.background(Color.White)) {
                LazyRow(
                    contentPadding = PaddingValues(horizontal = 16.dp, vertical = 12.dp),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    items(categorias) { cat ->
                        FilterChip(
                            selected = viewModel.categoriaSelecionada == cat,
                            onClick = { viewModel.onCategoriaChange(cat) },
                            label = { Text(cat) },
                            colors = FilterChipDefaults.filterChipColors(
                                selectedContainerColor = MaterialTheme.colorScheme.primaryContainer,
                                selectedLabelColor = MaterialTheme.colorScheme.onPrimaryContainer
                            )
                        )
                    }
                }
            }

            Spacer(modifier = Modifier.height(8.dp))

            if (viewModel.aCarregar) {
                Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                    CircularProgressIndicator()
                }
            } else {
                LazyVerticalGrid(
                    state = scrollState,
                    columns = GridCells.Fixed(2),
                    contentPadding = PaddingValues(start = 12.dp, end = 12.dp, top = 8.dp, bottom = 80.dp),
                    horizontalArrangement = Arrangement.spacedBy(12.dp),
                    verticalArrangement = Arrangement.spacedBy(12.dp),
                    modifier = Modifier.fillMaxSize()
                ) {
                    items(viewModel.anunciosVisiveis) { anuncio ->
                        AnuncioCardItem(anuncio = anuncio, navController = navController)
                    }

                    if (viewModel.aCarregarMais) {
                        item(span = { GridItemSpan(2) }) {
                            Box(modifier = Modifier.padding(16.dp).fillMaxWidth(), contentAlignment = Alignment.Center) {
                                CircularProgressIndicator(modifier = Modifier.size(24.dp), strokeWidth = 2.dp)
                            }
                        }
                    }
                }
            }
        }
    }

    if (showBottomSheet) {
        SlideUpScreen(
            onDismiss = { showBottomSheet = false },
            viewModel = viewModel
        )
    }
}