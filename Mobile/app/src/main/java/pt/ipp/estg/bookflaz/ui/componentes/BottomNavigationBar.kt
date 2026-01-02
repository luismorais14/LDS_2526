package pt.ipp.estg.bookflaz.ui.componentes

import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController

/**
 * Componente Composable que renderiza a barra de navegação inferior da aplicação.
 *
 * Utiliza o componente [NavigationBar] do Material3 e itera sobre uma lista de itens para criar
 * os botões de navegação. Gere também a lógica de navegação para evitar pilhas de
 * histórico duplicadas.
 *
 * @param navController O controlador de navegação utilizado para alternar entre ecrãs.
 * @param items Lista de objetos [BottomNavItem] que definem os ícones e rotas da barra.
 * @param currentRoute A string da rota (route) do ecrã atualmente visível (para destacar o ícone correto).
 * @param activeColor Cor do ícone e texto quando o item está selecionado (padrão: Preto).
 * @param inactiveColor Cor do ícone e texto quando o item não está selecionado (padrão: Cinzento).
 * @param backgroundColor Cor de fundo da barra de navegação (padrão: Branco).
 */
@Composable
fun BottomNavigationBar(
    navController: NavHostController,
    items: List<BottomNavItem>,
    currentRoute: String?,
    activeColor: Color = Color.Black,
    inactiveColor: Color = Color.Gray,
    backgroundColor: Color = Color.White
) {
    NavigationBar(
        containerColor = backgroundColor,
        tonalElevation = 0.dp
    ) {
        items.forEach { item ->
            val selected = currentRoute == item.route

            NavigationBarItem(
                selected = selected,
                onClick = {
                    if (!selected) {
                        navController.navigate(item.route) {
                            popUpTo(navController.graph.startDestinationId)
                            launchSingleTop = true
                        }
                    }
                },
                icon = {
                    Icon(
                        imageVector = item.icon,
                        contentDescription = item.label,
                        tint = if (selected) activeColor else inactiveColor
                    )
                },
                label = {
                    Text(
                        text = item.label,
                        color = if (selected) activeColor else inactiveColor,
                        fontWeight = if (selected) FontWeight.Bold else FontWeight.Normal
                    )
                },
                colors = NavigationBarItemDefaults.colors(
                    indicatorColor = Color.Transparent
                )
            )
        }
    }
}