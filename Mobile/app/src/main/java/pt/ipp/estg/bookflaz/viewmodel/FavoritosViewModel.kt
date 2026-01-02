package pt.ipp.estg.bookflaz.viewmodel;

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pt.ipp.estg.bookflaz.data.model.FavoritoResponse
import pt.ipp.estg.bookflaz.data.remote.api.AnuncioGetApi
import pt.ipp.estg.bookflaz.data.remote.retrofit.ApiService

/**
 * ViewModel responsável pela gestão de estado do ecrã de Favoritos.
 *
 * Este ViewModel segue o padrão MVVM, expondo o estado da UI através de [StateFlow]
 * e gerindo as operações assíncronas (chamadas de rede) utilizando Coroutines.
 */
class FavoritosViewModel : ViewModel() {
    private val api = ApiService.retrofit.create(AnuncioGetApi::class.java)

    private val _favoritos = MutableStateFlow<List<FavoritoResponse>>(emptyList())
    val favoritos: StateFlow<List<FavoritoResponse>> = _favoritos

    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading

    private val _erro = MutableStateFlow<String?>(null)
    val erro: StateFlow<String?> = _erro

    /**
     * Inicia o carregamento da lista de favoritos a partir da API.
     *
     * Atualiza os estados [isLoading], [erro] e [favoritos] conforme o progresso da operação.
     * Deve ser chamado quando o ecrã é iniciado ou para atualizar a lista (pull-to-refresh).
     */
    fun carregarFavoritos() {
        viewModelScope.launch {
            _isLoading.value = true
            _erro.value = null

            try {
                val lista = api.getFavoritos()
                _favoritos.value = lista
            } catch (e: Exception) {
                Log.e("Favoritos", "Erro ao carregar favoritos", e)
                _erro.value = "Não foi possível carregar os favoritos."
            } finally {
                _isLoading.value = false
            }
        }
    }

    /**
     * Remove um anúncio da lista de favoritos.
     *
     * Esta função comunica a alteração à API e, em caso de sucesso, atualiza a lista local
     * removendo o item imediatamente, sem necessitar de recarregar tudo da rede.
     *
     * @param id O identificador do anúncio a ser removido dos favoritos.
     */
    fun toggleFavorito(id: Int) {
        viewModelScope.launch {
            try {
                api.adicionarFavorito(id)

                _favoritos.value = _favoritos.value.filter { it.id != id }

            } catch (e: Exception) {
                _erro.value = e.message
            }
        }
    }
}