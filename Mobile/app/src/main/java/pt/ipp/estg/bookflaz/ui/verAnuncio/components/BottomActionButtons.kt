package pt.ipp.estg.bookflaz.ui.verAnuncio.components

import androidx.compose.foundation.border
import androidx.compose.foundation.layout.*
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.material3.ButtonDefaults
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.foundation.shape.RoundedCornerShape

/**
 * Conjunto de botões de ação apresentados na parte inferior da
 * página de detalhes de um anúncio.
 *
 * Este componente fornece duas ações possíveis:
 * - Enviar mensagem ao vendedor (contato e negociação)
 * - Enviar proposta/compra (ação principal do anúncio)
 *
 * Ambos os botões são exibidos lado a lado, com ênfase visual na
 * ação principal para guiar o utilizador.
 *
 * @param onProposal Callback executado ao clicar no botão de envio de mensagem.
 * @param onBuy Callback executado ao clicar no botão de proposta/compra.
 */
@Composable
fun BottomActionButtons(
    onProposal: () -> Unit,
    onBuy: () -> Unit,
) {
    val accentBlue = Color(0xFF41A9B8)

    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(16.dp),
        horizontalArrangement = Arrangement.spacedBy(12.dp)
    ) {

        Box(
            modifier = Modifier
                .weight(1f)
                .border(
                    width = 2.dp,
                    color = accentBlue,
                    shape = RoundedCornerShape(10.dp)
                )
        ) {
            Button(
                onClick = onProposal,
                colors = ButtonDefaults.buttonColors(
                    containerColor = Color.Transparent,
                    contentColor = accentBlue
                ),
                shape = RoundedCornerShape(10.dp),
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Enviar mensagem")
            }
        }

        Button(
            onClick = onBuy,
            modifier = Modifier.weight(1f),
            colors = ButtonDefaults.buttonColors(
                containerColor = accentBlue,
                contentColor = Color.White
            ),
            shape = RoundedCornerShape(10.dp),
        ) {
            Text("Proposta")
        }
    }
}
