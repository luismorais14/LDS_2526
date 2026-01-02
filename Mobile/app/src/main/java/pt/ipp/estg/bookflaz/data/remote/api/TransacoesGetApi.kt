package pt.ipp.estg.bookflaz.data.remote.api

import pt.ipp.estg.bookflaz.data.model.TransacaoResponse
import retrofit2.http.GET
import retrofit2.http.Query


/**
 * Interface responsável por definir os endpoints relacionados com Transações.
 *
 */
interface TransacoesGetApi {

    /**
     * Endpoint: GET /api/Transacao/registo
     * Aceita o filtro opcional 'papel' ("comprador" ou "vendedor").
     */
    @GET("api/Transacao/registo")
    suspend fun getHistorico(
        @Query("papel") papel: String? = null,
    ): List<TransacaoResponse>
}