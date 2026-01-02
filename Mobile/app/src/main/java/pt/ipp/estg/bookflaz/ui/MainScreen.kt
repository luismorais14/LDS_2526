package pt.ipp.estg.bookflaz.ui

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AttachMoney
import androidx.compose.material.icons.filled.Favorite
import androidx.compose.material.icons.filled.Home
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.navigation.NavHostController
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import pt.ipp.estg.bookflaz.ui.componentes.BottomNavItem
import pt.ipp.estg.bookflaz.ui.componentes.BottomNavigationBar
import pt.ipp.estg.bookflaz.ui.navigation.AppNavGraph

/**
 * O Ecrã Principal (Root Composable) que define a estrutura base da aplicação.
 *
 * Este componente utiliza um [Scaffold] para gerir o layout global, integrando:
 * 1. O grafo de navegação [AppNavGraph] que renderiza o conteúdo dos ecrãs.
 * 2. A barra de navegação inferior [BottomNavigationBar].
 *
 * Implementa lógica para ocultar automaticamente a barra inferior quando o utilizador
 * navega para ecrãs de detalhe (que não estão listados no menu principal).
 *
 * @param navController O controlador de navegação. É instanciado por defeito, mas pode ser injetado para testes.
 */
@Composable
fun MainScreen(
    navController: NavHostController = rememberNavController()
) {
    val navigationItems = listOf(
        BottomNavItem("Home", Icons.Default.Home, "home"),
        BottomNavItem("Favoritos", Icons.Default.Favorite, "favoritos"),
        BottomNavItem("Transações", Icons.Default.AttachMoney, "transactions")
    )

    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route

    Scaffold(
        bottomBar = {
            if (currentRoute in navigationItems.map { it.route }) {
                BottomNavigationBar(
                    navController = navController,
                    items = navigationItems,
                    currentRoute = currentRoute
                )
            }
        }
    ) { innerPadding ->
        Box(modifier = Modifier.padding(innerPadding)) {
            AppNavGraph(navController = navController)
        }
    }
}