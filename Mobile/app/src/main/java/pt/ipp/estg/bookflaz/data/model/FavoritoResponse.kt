package pt.ipp.estg.bookflaz.data.model

/**
 * Representa a resposta da API contendo os detalhes de um livro ou anúncio marcado como favorito.
 *
 * Esta classe é utilizada para mapear os dados JSON recebidos do backend para um objeto Kotlin.
 *
 * @property id O identificador único do anúncio/livro.
 * @property titulo O título do livro.
 * @property imagem O nome do ficheiro da imagem (pode ser nulo ou uma string "null").
 * @property preco O preço do livro.
 * @property estadoLivro Código numérico que representa o estado de conservação (ex: Novo, Usado).
 * @property tipoAnuncio Código numérico que representa o tipo de transação (ex: Venda, Troca).
 * @property totalFavoritos O número total de utilizadores que adicionaram este item aos favoritos.
 * @property favorito Indica se o utilizador atual marcou este item como favorito (booleano).
 */
data class FavoritoResponse(
    val id: Int,
    val titulo: String,
    val imagem: String?,
    val preco: Double,
    val estadoLivro: Int,
    val tipoAnuncio: Int,
    val totalFavoritos: Int,
    val favorito: Boolean
) {
    /**
     * Constrói e retorna o URL completo para a imagem do livro.
     *
     * Esta função verifica se o campo [imagem] é válido (não nulo, não vazio e diferente da string "null").
     * Se for válido, concatena o nome da imagem com o URL base da API armazenado na Azure.
     *
     * @return Uma [String] com o URL completo da imagem ou `null` se não houver imagem válida.
     */
    fun getImagemUrl(): String? =
        imagem
            ?.takeIf { it.isNotBlank() && it.lowercase() != "null" }
            ?.let { nome ->
                "https://flazbooksapi-dncpfkfmd6e8dwbj.germanywestcentral-01.azurewebsites.net/uploads/$nome"
            }
}