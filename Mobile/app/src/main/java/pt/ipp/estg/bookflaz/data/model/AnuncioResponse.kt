package pt.ipp.estg.bookflaz.data.model

/**
 * Representa um anúncio retornado pela API.
 *
 * Esta classe modela todos os dados relevantes de um anúncio
 * publicados por um utilizador, incluindo propriedades do livro,
 * estado e informações.
 *
 * O mapeamento é feito automaticamente pelo Retrofit.
 *
 * @property id Identificador único do anúncio.
 * @property preco Preço do livro no anúncio.
 * @property estadoLivro Valor inteiro associado ao enum EstadoLivro (0 = Novo, …).
 * @property tipoAnuncio Valor inteiro associado ao enum TipoAnuncio (0 = Venda, …).
 * @property estadoAnuncio Valor inteiro associado ao enum EstadoAnuncio (0 = Ativo, …).
 * @property imagens String que contêm os nomes dos ficheiros das imagens separados por ';'.
 * @property dataCriacao Data de criação no formato (YYYY-MM-DDTHH:MM:SS).
 * @property nomeVendedor Nome do utilizador que publicou o anúncio.
 * @property categoria Categoria do livro.
 * @property totalFavoritos Quantidade total de utilizadores que marcaram como favorito.
 * @property titulo Título do livro.
 * @property autor Nome do autor do livro.
 */
data class AnuncioResponse(
    val id: Int,
    val preco: Double,
    val estadoLivro: Int,
    val tipoAnuncio: Int,
    val estadoAnuncio: Int,
    val imagens: String?,
    val dataCriacao: String,
    val nomeVendedor: String,
    val categoria: String,
    val totalFavoritos: Int,
    val titulo: String,
    val autor: String,
) {

    /**
     * Converte a string de imagens retornada pelo backend
     * numa lista de URLs válidos para consumo pela UI.
     *
     * A API devolve os nomes das imagens separados por ';',
     * por isso é necessário:
     * 1 - Separar a string em elementos individuais
     * 2 - Limpar espaços e prefixos inválidos ("null")
     * 3 - Concatenar com a base URL do servidor
     *
     * @return Lista de URLs completas das imagens
     */
    fun getListaImagensComUrl(): List<String> =
        (imagens ?: "")
            .split(";")
            .map { it.trim() }
            .filter { it.isNotEmpty() && it.lowercase() != "null" }
            .map { img ->
                "https://flazbooksapi-dncpfkfmd6e8dwbj.germanywestcentral-01.azurewebsites.net/uploads/$img"
            }

}