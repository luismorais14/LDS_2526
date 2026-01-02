package pt.ipp.estg.bookflaz.data.model

/**
 * Representa a resposta "invólucro" (wrapper) da API para a listagem de anúncios.
 *
 * Esta classe é utilizada para estruturar a resposta JSON que contém uma lista de anúncios
 * e metadados sobre a operação (sucesso e contagem total).
 *
 * @property sucesso Indica se a requisição à API foi processada com êxito.
 * @property total O número total de anúncios retornados ou disponíveis (útil para paginação).
 * @property anuncios A lista contendo os objetos [Anuncio].
 */
data class TodosAnunciosResponse(
    val sucesso: Boolean,
    val total: Int,
    val anuncios: List<Anuncio>,
)

/**
 * Representa um anúncio individual de venda ou troca de um livro.
 *
 * Contém todos os detalhes visíveis nas listas de pesquisa ou no feed principal.
 *
 * @property id O identificador único do anúncio.
 * @property preco O valor monetário definido pelo vendedor.
 * @property estadoLivro Código numérico representando a condição física (ex: 0=Novo, 1=Usado).
 * @property tipoAnuncio Código numérico representando a modalidade (ex: Venda, Troca, Doação).
 * @property estadoAnuncio Código numérico representando o status do anúncio (ex: Ativo, Vendido, Pausado).
 * @property imagem O nome do ficheiro da imagem de capa (pode ser nulo).
 * @property dataCriacao A data em que o anúncio foi publicado (formato String, geralmente ISO 8601).
 * @property nomeVendedor O nome do utilizador que publicou o anúncio.
 * @property categoria A categoria literária do livro (ex: "Romance", "Técnico").
 * @property totalFavoritos Quantidade de utilizadores que marcaram este anúncio como favorito.
 * @property titulo O título do livro anunciado.
 * @property autor O nome do autor do livro.
 */
data class Anuncio(
    val id: Int,
    val preco: Double,
    val estadoLivro: Int,
    val tipoAnuncio: Int,
    val estadoAnuncio: Int,
    val imagem: String?,
    val dataCriacao: String,
    val nomeVendedor: String,
    val categoria: String,
    val totalFavoritos: Int,
    val titulo: String,
    val autor: String,
)