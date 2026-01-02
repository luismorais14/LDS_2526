package pt.ipp.estg.bookflaz.data.remote.retrofit

import okhttp3.OkHttpClient
import pt.ipp.estg.bookflaz.data.remote.AuthInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

/**
 * Serviço responsável por configurar o Retrofit para comunicação com a API.
 *
 * Esta classe centraliza:
 * - URL base da API
 * - Cliente HTTP com interceptor de autenticação
 * - Conversor JSON para objetos Kotlin via Gson
 *
 * O objeto é um Singleton, garantindo uma única instância do Retrofit
 * durante toda a execução da aplicação.
 */
object ApiService {

    /**
     * URL base utilizada em todas as chamadas à API.
     * Deve sempre terminar com '/' para Retrofit resolver corretamente os endpoints.
     */
    private const val BASE_URL = "https://flazbooksapi-dncpfkfmd6e8dwbj.germanywestcentral-01.azurewebsites.net/"

    /**
     * Cliente HTTP personalizado com suporte a:
     * - Interceptor de autenticação (envia token JWT no header Authorization)
     *
     * O OkHttpClient é a base de todas as chamadas realizadas pelo Retrofit.
     */
    private val client = OkHttpClient.Builder()
        .addInterceptor(AuthInterceptor())
        .build()

    /**
     * Instância do Retrofit configurada com:
     * - URL base da API
     * - Cliente HTTP com autenticação
     * - Converter JSON → Objetos Kotlin usando Gson
     */
    val retrofit: Retrofit = Retrofit.Builder()
        .baseUrl(BASE_URL)
        .client(client)
        .addConverterFactory(GsonConverterFactory.create())
        .build()
}