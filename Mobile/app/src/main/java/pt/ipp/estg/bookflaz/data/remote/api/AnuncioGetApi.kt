package pt.ipp.estg.bookflaz.data.remote.api

import pt.ipp.estg.bookflaz.data.model.Anuncio
import pt.ipp.estg.bookflaz.data.model.AnuncioResponse
import pt.ipp.estg.bookflaz.data.model.FavoritoResponse
import pt.ipp.estg.bookflaz.data.model.TodosAnunciosResponse
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.Query

/**
 * Interface responsável por definir os endpoints relacionados a anúncios e favoritos.
 *
 * Esta interface é interpretada pelo Retrofit, que gera automaticamente as funções
 * assíncronas de acesso à API.
 *
 * Todas as chamadas são executadas numa coroutine (funções `suspend`) para evitar
 * bloqueio da UI.
 */
interface AnuncioGetApi {

    /**
     * Obtém os detalhes completos de um anúncio específico.
     *
     * Endpoint: GET /api/Anuncio/{id}
     *
     * @param id Identificador único do anúncio a carregar.
     * @return [AnuncioResponse] contendo os dados do anúncio.
     */
    @GET("api/Anuncio/{id}")
    suspend fun getAnuncio(@Path("id") id: Int): AnuncioResponse

    /**
     * Adiciona ou remove um anúncio da lista de favoritos do utilizador autenticado.
     *
     * Esta ação funciona como *toggle*: se já for favorito, será removido; caso contrário, será adicionado.
     *
     * Endpoint: POST /api/Favorito?idAnuncio={id}
     *
     * @param id Identificador único do anúncio.
     */
    @POST("api/Favorito")
    suspend fun adicionarFavorito(@Query("idAnuncio") id: Int)

    /**
     * Obtém a lista de todos os anúncios que o utilizador autenticado marcou como favoritos.
     *
     * Requer envio de token JWT via Authorization Header (feito automaticamente pelo AuthInterceptor).
     *
     * Endpoint: GET /api/Favorito
     *
     * @return Lista de [AnuncioResponse] representando os anúncios favoritos do utilizador.
     */
    @GET("api/Favorito")
    suspend fun getFavoritos(): List<FavoritoResponse>

    /**
     * Obtém a lista completa de todos os anúncios criados na plataforma
     *
     * Endpoint: GET /api/anuncio
     *
     * @return Lista de [TodosAnunciosResponse] representando os anúncios criados na plataforma
     */
    @GET("api/anuncio")
    suspend fun getAnuncios(): TodosAnunciosResponse
}