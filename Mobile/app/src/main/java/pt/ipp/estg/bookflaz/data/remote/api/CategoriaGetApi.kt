package pt.ipp.estg.bookflaz.data.remote.api

import pt.ipp.estg.bookflaz.data.model.CategoriaResponse
import retrofit2.http.GET

interface CategoriaGetApi {
    /**
     * Retorna uma lista de categorias
     *
     * @return Lista de categorias
     *
     * @see CategoriaResponse
     */
    @GET("api/Categoria/categorias/disponiveis")
    suspend fun getCategorias(): List<CategoriaResponse>
}