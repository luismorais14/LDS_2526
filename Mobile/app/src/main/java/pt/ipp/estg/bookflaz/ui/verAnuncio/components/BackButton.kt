package pt.ipp.estg.bookflaz.ui.verAnuncio.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp

/**
 * Botão de navegação utilizado para voltar ao ecrã anterior.
 *
 * Implementado como um botão circular com um ícone de seta para trás,
 * com fundo semitransparente, garantindo boa visibilidade em qualquer
 * background (ex.: imagens do produto).
 *
 * Este componente pode e deve ser reutilizável em múltiplas screens da aplicação.
 *
 * @param onClick Callback executado quando o utilizador toca no botão.
 * @param modifier Permite personalização do posicionamento e estilo externo.
 */
@Composable
fun BackButton(
    onClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    IconButton(
        onClick = onClick,
        modifier = modifier
            .size(40.dp)
            .background(
                color = Color.Black.copy(alpha = 0.45f),
                shape = CircleShape
            )
            .padding(4.dp)
    ) {
        Icon(
            imageVector = Icons.Filled.ArrowBack,
            contentDescription = "Back",
            tint = Color.White
        )
    }
}