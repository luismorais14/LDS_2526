package pt.ipp.estg.bookflaz.data.model

/**
 * Classe de resposta para a requisição de categorias.
 *
 * @property id O identificador único da categoria.
 * @property nome O nome da categoria.
 * @property ativo Indica se a categoria está ativa ou não.
 *
 * @constructor Cria uma instância da classe CategoriaResponse com os valores fornecidos.
 */
data class CategoriaResponse(
    val id: Int,
    val nome: String,
    val ativo: Boolean
)