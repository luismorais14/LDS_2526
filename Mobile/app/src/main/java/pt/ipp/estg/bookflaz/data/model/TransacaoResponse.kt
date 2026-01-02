package pt.ipp.estg.bookflaz.data.model

import com.google.gson.annotations.SerializedName

data class TransacaoResponse(

    /**
     * O mapeamento JSON é forçado via @SerializedName para garantir
     * compatibilidade com o formato PascalCase enviado pelo backend (.NET).
     *
     * @property id Identificador único da transação.
     * @property data Data em que a transação ocorreu (formato string).
     * @property estado Estado atual da transação (ex: Pendente, Concluída, Cancelada).
     * @property pedidoId Identificador do pedido que originou a transação.
     * @property anuncioId Identificador do anúncio associado (pode ser nulo se o anúncio foi apagado).
     * @property tituloAnuncio Título do livro associado à transação.
     * @property imagemAnuncio Nome do ficheiro da imagem de capa do anúncio.
     * @property preco Preço original do anúncio.
     * @property tipoAnuncio Valor inteiro representando o tipo (Venda, Aluguer, etc.).
     * @property outroUtilizadorId ID do outro interveniente na transação (Comprador ou Vendedor).
     * @property papel String indicando o papel do utilizador atual ("COMPRADOR" ou "VENDEDOR").
     * @property valorFinal Valor monetário final da transação após descontos.
     * @property pontosUsados Quantidade de pontos de fidelidade utilizados.
     * @property valorDesconto Valor monetário descontado do total.
     */
    @SerializedName("id") val id: Long,
    @SerializedName("data") val data: String,
    @SerializedName("estado") val estado: String,
    @SerializedName("pedidoId") val pedidoId: Long,
    @SerializedName("anuncioId") val anuncioId: Long?,
    @SerializedName("tituloAnuncio") val tituloAnuncio: String?,
    @SerializedName("imagemAnuncio") val imagemAnuncio: String?,
    @SerializedName("preco") val preco: Double?,
    @SerializedName("tipoAnuncio") val tipoAnuncio: Int?,
    @SerializedName("outroUtilizadorId") val outroUtilizadorId: Long,
    @SerializedName("papel") val papel: String?,
    @SerializedName("valorFinal") val valorFinal: Double,
    @SerializedName("pontosUsados") val pontosUsados: Int,
    @SerializedName("valorDesconto") val valorDesconto: Double,
) {
    /**
     * Retorna apenas UMA string (URL), porque a transação só mostra a capa.
     */
    fun getImagemUrlCompleta(): String? {
        if (imagemAnuncio.isNullOrEmpty()) return null

        val fixed = imagemAnuncio.removePrefix("null").trim()

        if (fixed.isEmpty() || fixed.equals("null", ignoreCase = true)) return null

        return "https://flazbooksapi-dncpfkfmd6e8dwbj.germanywestcentral-01.azurewebsites.net/uploads/$fixed"
    }
}