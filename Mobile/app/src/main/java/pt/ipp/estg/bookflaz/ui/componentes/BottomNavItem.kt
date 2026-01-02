package pt.ipp.estg.bookflaz.ui.componentes

import androidx.compose.ui.graphics.vector.ImageVector

/**
 * Modelo de dados que representa um item individual na barra de navegação inferior (Bottom Navigation).
 *
 * Esta classe serve para agrupar as informações visuais (ícone e texto) com a lógica de
 * navegação (rota) para cada botão do menu.
 *
 * @property label O texto descritivo que aparece por baixo do ícone (ex: "Início", "Perfil").
 * @property icon O ícone vetorial (ImageVector) do Material Design a ser exibido.
 * @property route A string única que identifica o destino de navegação associado a este item.
 */
data class BottomNavItem(
    val label: String,
    val icon: ImageVector,
    val route: String
)