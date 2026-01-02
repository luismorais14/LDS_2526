package pt.ipp.estg.bookflaz.viewmodel

import android.util.Log
import androidx.compose.runtime.*
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.launch
import pt.ipp.estg.bookflaz.data.model.TransacaoResponse
import pt.ipp.estg.bookflaz.data.remote.api.TransacoesGetApi
import pt.ipp.estg.bookflaz.data.remote.retrofit.ApiService

/**
 * ViewModel responsável por gerir e expor o histórico de transações
 * para a UI.
 *
 * Esta classe atua como intermediária entre a camada de dados (API) e a
 * interface gráfica (Screen), gerindo o estado do carregamento e a lista
 * de dados apresentada.
 *
 * Funcionalidades:
 * - Carrega o histórico completo de transações do utilizador
 * - Gere o estado de carregamento (loading) para feedback visual
 * - Permite filtragem opcional por papel (comprador/vendedor)
 *
 * A implementação utiliza coroutines via [viewModelScope] para executar
 * os pedidos de rede de forma assíncrona e segura.
 */
class TransacaoViewModel : ViewModel() {

    /** Cliente de API responsável pelas operações remotas de transações */
    private val api = ApiService.retrofit.create(TransacoesGetApi::class.java)

    /** Estado reativo contendo a lista de transações carregadas da API */
    var listaTransacoes by mutableStateOf<List<TransacaoResponse>>(emptyList())
        private set

    /**
     * Indica se o processo de carregamento de dados está em curso.
     * Utilizado pela UI para exibir indicadores de progresso (ex: CircularProgressIndicator).
     */
    var isLoading by mutableStateOf(false)
        private set

    /**
     * Solicita à API o histórico de transações do utilizador autenticado.
     *
     * @param papel Filtro opcional ("comprador" ou "vendedor"). Se nulo, traz tudo.
     */
    fun carregarHistorico(papel: String? = null) {
        viewModelScope.launch {
            isLoading = true
            try {
                listaTransacoes = api.getHistorico(papel = papel)
            } catch (e: Exception) {
                Log.e("API", "Erro ao carregar transações: ${e.message}")
            } finally {
                isLoading = false
            }
        }
    }
}