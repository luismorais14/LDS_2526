package pt.ipp.estg.bookflaz.viewmodel

import android.util.Log
import androidx.compose.runtime.*
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.launch
import pt.ipp.estg.bookflaz.data.model.Anuncio
import pt.ipp.estg.bookflaz.data.model.AnuncioResponse
import pt.ipp.estg.bookflaz.data.remote.api.AnuncioGetApi
import pt.ipp.estg.bookflaz.data.remote.retrofit.ApiService

/**
 * ViewModel responsável pela gestão da lógica de negócio e estados dos Anúncios.
 *
 * Esta classe gere:
 * 1. A comunicação com a API para obter listas e detalhes.
 * 2. A filtragem local de anúncios (Pesquisa, Categoria, Tipo, Preço).
 * 3. A paginação dos dados para a UI (Infinite Scroll).
 * 4. A gestão de favoritos e estados de carregamento.
 */
class AnuncioViewModel : ViewModel() {

    private val api = ApiService.retrofit.create(AnuncioGetApi::class.java)

    /** Lista completa original carregada da API (sem filtros). */
    private var listaCompleta = listOf<Anuncio>()

    /** Lista intermédia após a aplicação dos filtros (pesquisa, preço, etc). */
    private var listaFiltrada = listOf<Anuncio>()

    private val _anunciosVisiveis = mutableStateListOf<Anuncio>()
    /** * Lista exposta à UI que contém apenas os itens a serem renderizados atualmente.
     * É alimentada progressivamente pela função [carregarProximoLote].
     */
    val anunciosVisiveis: List<Anuncio> = _anunciosVisiveis

    /** Texto atual da barra de pesquisa. */
    var textoPesquisa by mutableStateOf("")
        private set

    /** Categoria selecionada para filtro (ex: "Policial", "Romance", "Todas"). */
    var categoriaSelecionada by mutableStateOf("Todas")
        private set

    /** * Conjunto de IDs dos tipos de anúncio selecionados.
     * Geralmente mapeado para Enums (ex: 0=Venda, 1=Troca, etc).
     */
    var tiposSelecionados by mutableStateOf(setOf(0, 1, 2))
        private set

    private val ITENS_POR_PAGINA = 10
    private var paginaAtual = 0

    /** Indica se o carregamento inicial ou total está a decorrer. */
    var aCarregar by mutableStateOf(false)

    /** Indica se está a carregar mais itens (paginação/scroll infinito). */
    var aCarregarMais by mutableStateOf(false)

    /** * Guarda os detalhes de um anúncio específico selecionado.
     * Pode ser null enquanto carrega ou se nenhum foi selecionado.
     */
    var anuncio by mutableStateOf<AnuncioResponse?>(null)

    /** Indica se o anúncio atualmente visualizado está nos favoritos do utilizador. */
    var isFavorite by mutableStateOf(false)

    /** O intervalo de preços selecionado atualmente pelo utilizador (Slider). */
    var priceRange by mutableStateOf(0f..1000f)
        private set

    /** O menor preço encontrado na lista completa (para definir os limites do Slider). */
    var minPrecoAbsoluto by mutableStateOf(0f)
        private set

    /** O maior preço encontrado na lista completa. */
    var maxPrecoAbsoluto by mutableStateOf(1000f)
        private set


    /** Atualiza o texto de pesquisa e reaplica os filtros automaticamente. */
    fun onTextoPesquisaChange(novoTexto: String) {
        textoPesquisa = novoTexto
        aplicarFiltros()
    }

    /** Atualiza a categoria selecionada e reaplica os filtros. */
    fun onCategoriaChange(novaCategoria: String) {
        categoriaSelecionada = novaCategoria
        aplicarFiltros()
    }

    /** Limpa o campo de pesquisa e restaura a lista filtrada. */
    fun onLimparPesquisa() {
        textoPesquisa = ""
        aplicarFiltros()
    }

    /** * Adiciona ou remove um tipo de anúncio do conjunto de filtros ativos.
     * @param tipoId O ID numérico do tipo de anúncio.
     */
    fun toggleTipoFiltro(tipoId: Int) {
        val novoSet = tiposSelecionados.toMutableSet()
        if (novoSet.contains(tipoId)) novoSet.remove(tipoId) else novoSet.add(tipoId)
        tiposSelecionados = novoSet
        aplicarFiltros()
    }

    /** Atualiza o intervalo de preço selecionado e filtra a lista. */
    fun onPriceRangeChange(newRange: ClosedFloatingPointRange<Float>) {
        priceRange = newRange
        aplicarFiltros()
    }

    /**
     * Carrega a lista inicial de anúncios da API.
     * Calcula também os preços mínimos e máximos para configurar os filtros.
     */
    fun carregarAnunciosIniciais() {
        if (listaCompleta.isNotEmpty()) return

        aCarregar = true
        viewModelScope.launch {
            try {
                val response = api.getAnuncios()
                if (response.sucesso) {
                    listaCompleta = response.anuncios

                    if (listaCompleta.isNotEmpty()) {
                        val min = listaCompleta.minOf { it.preco.toFloat() }
                        val max = listaCompleta.maxOf { it.preco.toFloat() }
                        minPrecoAbsoluto = min
                        maxPrecoAbsoluto = max
                        priceRange = min..max
                    }

                    aplicarFiltros()
                }
            } catch (e: Exception) {
                Log.e("API", "Erro ao carregar lista: ${e.message}")
            } finally {
                aCarregar = false
            }
        }
    }

    /**
     * Filtra a [listaCompleta] com base em todos os critérios selecionados
     * (texto, categoria, tipo, preço) e reinicia a paginação.
     */
    private fun aplicarFiltros() {
        listaFiltrada = listaCompleta.filter { anuncio ->
            val matchNome = anuncio.titulo.contains(textoPesquisa, ignoreCase = true)

            val matchCat = if (categoriaSelecionada == "Todas") true
            else anuncio.categoria.equals(categoriaSelecionada, ignoreCase = true)

            val matchTipo = anuncio.tipoAnuncio in tiposSelecionados

            val preco = anuncio.preco.toFloat()
            val matchPreco = preco >= priceRange.start && preco <= priceRange.endInclusive

            matchNome && matchCat && matchTipo && matchPreco
        }

        _anunciosVisiveis.clear()
        paginaAtual = 0
        carregarProximoLote()
    }

    /**
     * Sistema de Paginação Local.
     * Adiciona o próximo lote de [ITENS_POR_PAGINA] da [listaFiltrada] para a [_anunciosVisiveis].
     * Deve ser chamado quando o utilizador faz scroll até ao fundo da lista.
     */
    fun carregarProximoLote() {
        if (aCarregarMais || _anunciosVisiveis.size >= listaFiltrada.size) return

        aCarregarMais = true
        viewModelScope.launch {
            val inicio = paginaAtual * ITENS_POR_PAGINA
            val fim = (inicio + ITENS_POR_PAGINA).coerceAtMost(listaFiltrada.size)

            if (inicio < listaFiltrada.size) {
                _anunciosVisiveis.addAll(listaFiltrada.subList(inicio, fim))
                paginaAtual++
            }
            aCarregarMais = false
        }
    }

    /**
     * Carrega os detalhes completos de um anúncio específico e verifica se é favorito.
     * @param id O ID do anúncio a buscar.
     */
    fun carregarAnuncioDetalhe(id: Int) {
        viewModelScope.launch {
            try {
                anuncio = api.getAnuncio(id)
                val favoritosResponse = api.getFavoritos()
                isFavorite = favoritosResponse.any { it.id == id }
            } catch (e: Exception) {
                Log.e("API", "Erro ao carregar detalhe: ${e.message}")
            }
        }
    }

    /**
     * Alterna o estado de favorito de um anúncio na API e atualiza a UI localmente.
     * Atualiza também a contagem de favoritos no objeto [anuncio] atual.
     */
    fun toggleFavorito(id: Int) {
        viewModelScope.launch {
            try {
                api.adicionarFavorito(id)
                isFavorite = !isFavorite

                anuncio = anuncio?.copy(
                    totalFavoritos = if (isFavorite) (anuncio?.totalFavoritos ?: 0) + 1
                    else (anuncio?.totalFavoritos ?: 0).coerceAtLeast(1) - 1
                )
            } catch (e: Exception) {
                Log.e("API", "Erro favorito: ${e.message}")
            }
        }
    }
}