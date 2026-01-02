package pt.ipp.estg.bookflaz.ui.verTransacoes

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import coil.compose.AsyncImage
import pt.ipp.estg.bookflaz.data.model.TransacaoResponse
import pt.ipp.estg.bookflaz.viewmodel.TransacaoViewModel

/**
 * Ecrã responsável por apresentar o histórico de transações do utilizador.
 *
 * A lógica de carregamento dos dados é gerida pelo [TransacaoViewModel],
 * sendo executada automaticamente quando o ecrã é iniciado.
 *
 * @param viewModel ViewModel responsável por comunicar com a API.
 */
@Composable
fun TransactionsScreen(
    viewModel: TransacaoViewModel = viewModel()
) {
    LaunchedEffect(Unit) {
        viewModel.carregarHistorico()
    }

    val transacoes = viewModel.listaTransacoes
    val isLoading = viewModel.isLoading

    Scaffold(
        topBar = {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp),
                contentAlignment = Alignment.Center
            ) {
                Text(
                    text = "Histórico de Transações",
                    style = MaterialTheme.typography.titleLarge.copy(fontWeight = FontWeight.Bold)
                )
            }
        }
    ) { padding ->
        Box(modifier = Modifier.padding(padding).fillMaxSize()) {

            if (isLoading) {
                CircularProgressIndicator(modifier = Modifier.align(Alignment.Center))
            } else if (transacoes.isEmpty()) {
                Text(
                    text = "Nenhuma transação encontrada.",
                    modifier = Modifier.align(Alignment.Center),
                    color = Color.Gray
                )
            } else {
                LazyColumn(
                    modifier = Modifier
                        .testTag("listaTransacoes")
                        .fillMaxSize(),
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    items(transacoes) { transacao ->
                        TransactionCard(transacao)
                    }
                }
            }
        }
    }
}

/**
 * Componente visual que representa um item individual na lista de transações.
 *
 * Exibe a imagem do livro, título, estado, papel (comprador/vendedor) e o preço final.
 * Inclui proteções contra dados nulos vindos da API (ex: "Sem Título").
 *
 * @param transacao Objeto com os dados da transação a exibir.
 */
@Composable
fun TransactionCard(transacao: TransacaoResponse) {
    val accentColor = Color(0xFF41A9B8)

    Card(
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp),
        modifier = Modifier.fillMaxWidth()
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {

            AsyncImage(
                model = transacao.getImagemUrlCompleta() ?: "https://via.placeholder.com/150",
                contentDescription = "Imagem Livro",
                contentScale = ContentScale.Crop,
                modifier = Modifier
                    .size(80.dp)
                    .clip(RoundedCornerShape(8.dp))
                    .background(Color.LightGray)
            )

            Spacer(modifier = Modifier.width(16.dp))


            Column(modifier = Modifier.weight(1f)) {

                Text(
                    text = transacao.tituloAnuncio ?: "Sem Título",
                    modifier = Modifier.testTag("txtTituloTransacao"),
                    style = MaterialTheme.typography.bodyLarge.copy(fontWeight = FontWeight.Bold),
                    maxLines = 1
                )

                Text(
                    text = formatarData(transacao.data ?: ""),
                    style = MaterialTheme.typography.bodySmall,
                    color = Color.Gray
                )

                Spacer(modifier = Modifier.height(4.dp))

                Row(verticalAlignment = Alignment.CenterVertically) {
                    Badge(
                        text = transacao.estado ?: "Pendente",
                        color = Color.DarkGray
                    )

                    Spacer(modifier = Modifier.width(8.dp))

                    val papelTexto = transacao.papel ?: "Desconhecido"
                    Badge(
                        text = papelTexto,
                        // Azul para Compras, Vermelho para Vendas
                        color = if (papelTexto.equals("COMPRADOR", ignoreCase = true)) accentColor else Color(0xFFE57373)
                    )
                }
            }

            Text(
                text = "${transacao.valorFinal} €",
                modifier = Modifier.testTag("txtPrecoTransacao"),
                style = MaterialTheme.typography.titleMedium.copy(fontWeight = FontWeight.Bold, color = accentColor)
            )
        }
    }
}

/**
 * Pequena etiqueta visual para destacar informação (Estado ou Papel).
 *
 * @param text Texto da etiqueta.
 * @param color Cor do texto e do fundo (com transparência).
 */
@Composable
fun Badge(text: String, color: Color) {
    Box(
        modifier = Modifier
            .background(color.copy(alpha = 0.1f), RoundedCornerShape(4.dp))
            .padding(horizontal = 6.dp, vertical = 2.dp)
    ) {
        Text(
            text = text.uppercase(),
            style = MaterialTheme.typography.labelSmall.copy(fontSize = 10.sp),
            color = color
        )
    }
}

/**
 * Formata a data removendo a hora, se existir.
 * Exemplo: "2024-12-02T15:00" passa a "2024-12-02".
 */
private fun formatarData(data: String): String {
    return try {
        data.split("T")[0]
    } catch (e: Exception) {
        data
    }
}