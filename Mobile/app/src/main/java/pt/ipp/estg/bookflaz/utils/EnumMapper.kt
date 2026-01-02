package pt.ipp.estg.bookflaz.utils

/**
 * Converte o valor numérico do enum `EstadoLivro`
 * numa string legível para apresentação na interface.
 *
 * Mapeamento atual:
 * - 0 → Novo
 * - 1 → Como novo
 * - 2 → Usado
 * - 3 → Danificado
 *
 * Caso o valor não seja reconhecido, será devolvido "Desconhecido".
 *
 * @param code Valor inteiro do enum vindo da API.
 * @return Texto equivalente ao estado do livro.
 */
fun mapEstadoLivro(code: Int) = when (code) {
    0 -> "Novo"
    1 -> "Como novo"
    2 -> "Usado"
    3 -> "Danificado"
    else -> "Desconhecido"
}

/**
 * Converte o valor numérico do enum `EstadoAnuncio`
 * numa string legível para apresentação na interface.
 *
 * Mapeamento atual:
 * - 0 → Ativo
 * - 1 → Indisponível
 * - 2 → Vendido
 *
 * Caso o valor não seja reconhecido, será devolvido "Desconhecido".
 *
 * @param code Valor inteiro do enum vindo da API.
 * @return Texto equivalente ao estado do anúncio.
 */
fun mapEstadoAnuncio(code: Int) = when (code) {
    0 -> "Ativo"
    1 -> "Indisponível"
    2 -> "Vendido"
    else -> "Desconhecido"
}

/**
 * Converte o valor numérico do enum `TipoAnuncio`
 * numa string legível para apresentação na interface.
 *
 * Caso o valor não seja reconhecido, será devolvido "Desconhecido".
 *
 * @param code Valor inteiro do enum vindo da API.
 * @return Texto equivalente ao tipo do anúncio.
 */
fun mapTipoAnuncio(code: Int) = when (code) {
    0 -> "Venda"
    1 -> "Aluguer"
    2 -> "Doação"
    else -> "Desconhecido"
}
