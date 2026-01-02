package pt.ipp.estg.bookflaz.data.remote

import okhttp3.Interceptor
import okhttp3.Response

/**
 * Interceptor responsável por adicionar automaticamente o header Authorization
 * (JWT Bearer Token) em todas as chamadas enviadas pelo Retrofit.
 *
 * Esta abordagem garante que endpoints protegidos por autenticação
 * podem ser consumidos sem a necessidade de adicionar manualmente
 * o token em cada request.
 */
class AuthInterceptor : Interceptor {

    /**
     * Intercepta a request original e cria uma nova request
     * acrescentando o header "Authorization: Bearer <token>".
     *
     * @param chain Corrente de chamadas HTTP a ser processada.
     * @return Response resultante da execução da request modificada.
     */
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request().newBuilder()
            .addHeader(
                "Authorization",
                "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxNDkiLCJlbWFpbCI6InRlc3RlQGJvb2tmbGF6LmNvbSIsIklzQWRtaW4iOiJGYWxzZSIsIm5iZiI6MTc2NDY5MzcyNywiZXhwIjoxNzY1Mjk4NTI3LCJpYXQiOjE3NjQ2OTM3MjcsImlzcyI6IkJvb2tGbGF6SXNzdWVyIiwiYXVkIjoiQm9va0ZsYXpBdWRpZW5jZSJ9.ttW1BSZiT05CPepLkjwCzvyoytpphs-oRgJ7ovUF8d8"
            )
            .build()

        return chain.proceed(request)
    }
}