package pt.ipp.estg.bookflaz.ui.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import pt.ipp.estg.bookflaz.ui.listarAnuncios.ListarAnunciosScreen
import pt.ipp.estg.bookflaz.ui.verAnuncio.ProductScreen
import pt.ipp.estg.bookflaz.ui.verFavoritos.FavoritosScreen
import pt.ipp.estg.bookflaz.ui.verTransacoes.TransactionsScreen

/**
 * Define a estrutura de navegação principal (NavGraph) da aplicação utilizando Jetpack Navigation Compose.
 *
 * Esta função configura o [NavHost], que atua como um contentor onde os ecrãs são trocados.
 * Mapeia Strings (rotas) para os respetivos Composable screens.
 *
 * @param navController O controlador central que gere a pilha de navegação (Back Stack) e a troca de ecrãs.
 */
@Composable
fun AppNavGraph(navController: NavHostController) {
    NavHost(
        navController = navController,
        startDestination = "home"
    ) {
        /**
         * Rota: "product/{id}"
         * Objetivo: Mostrar os detalhes de um livro específico.
         * Argumentos: Recebe um 'id' dinâmico na URL.
         */
        composable("product/{id}") { backStackEntry ->
            val id = backStackEntry.arguments?.getString("id")?.toInt() ?: 3

            ProductScreen(
                anuncioId = id,
                onBack = { navController.navigateUp() }
            )
        }

        /**
         * Rota: "favoritos"
         * Objetivo: Listar os livros marcados como favoritos pelo utilizador.
         */
        composable("favoritos") {
            FavoritosScreen(
                onAnuncioClick = { anuncioId ->
                    navController.navigate("product/$anuncioId")
                }
            )
        }

        /**
         * Rota: "home"
         * Objetivo: Ecrã principal (Feed), mostra a lista de todos os anúncios.
         */
        composable("home") {
            ListarAnunciosScreen(
                onBack = { navController.navigateUp() },
                navController = navController
            )
        }

        /**
         * Rota: "transactions"
         * Objetivo: Mostrar o histórico de compras e vendas do utilizador.
         */
        composable("transactions") {
            TransactionsScreen()
        }
    }
}