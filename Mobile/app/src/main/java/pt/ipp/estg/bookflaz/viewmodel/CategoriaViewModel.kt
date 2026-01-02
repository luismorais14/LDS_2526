package pt.ipp.estg.bookflaz.viewmodel

import android.util.Log
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.launch
import pt.ipp.estg.bookflaz.data.model.CategoriaResponse
import pt.ipp.estg.bookflaz.data.remote.api.AnuncioGetApi
import pt.ipp.estg.bookflaz.data.remote.api.CategoriaGetApi
import pt.ipp.estg.bookflaz.data.remote.retrofit.ApiService

/**
 * ViewModel para a lista de categorias
 *
 * @property api CategoriaGetApi
 * @property categorias List<CategoriaResponse>
 */
class CategoriaViewModel : ViewModel() {
    private val api = ApiService.retrofit.create(CategoriaGetApi::class.java)
    var categorias by mutableStateOf<List<CategoriaResponse>>(listOf())
        private set

    /**
     * Carrega todas as categorias disponíveis no sistema
     */
    fun carregarCategorias() {
        viewModelScope.launch {
            try {
                categorias = api.getCategorias()
            } catch (e: Exception) {
                Log.e("API", "Erro ao carregar categorias: ${e.message}")
            }
        }
    }

}