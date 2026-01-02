package pt.ipp.estg.bookflaz

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onAllNodesWithTag
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.performClick
import org.junit.Before
import org.junit.Rule
import org.junit.Test

/**
 * Testes de Sistema da funcionalidade “Favoritos”.
 *
 * Valida o fluxo completo:
 *  - marcar um anúncio como favorito
 *  - verificar que aparece na lista de favoritos
 *  - remover favorito a partir da lista
 */
class FavoritosSystemTest {

    @get:Rule
    val rule = createAndroidComposeRule<MainActivity>()


    /**
     * ST07 – Adicionar anúncio aos favoritos e ver na lista
     *
     * Objetivo:
     *  - Garantir que ao marcar um anúncio como favorito,
     *    este aparece na página de favoritos.
     */
    @Test
    fun adicionarFavorito_e_ApareceNaListaDeFavoritos() {
        rule.activityRule.scenario.onActivity { activity ->
            activity.navController.navigate("product/3")
        }

        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("txtTitulo").fetchSemanticsNodes().isNotEmpty()
        }

        // 2) Clicar no botão de favorito no ecrã de produto
        rule.onNodeWithTag("btnFavorite").assertExists().performClick()

        // 3) Dar tempo para a API tratar o pedido
        Thread.sleep(1500)

        rule.activityRule.scenario.onActivity { activity ->
            activity.navController.navigate("favoritos")
        }

        rule.onNodeWithTag("nav_favoritos").assertExists().performClick()

        // 5) Esperar até existir pelo menos um card de favorito
        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("favoritoCard").fetchSemanticsNodes().isNotEmpty()
        }

        // 6) Verificar que o card está visível
        rule.onAllNodesWithTag("favoritoCard")[0].assertIsDisplayed()
    }

    /**
     * ST08 – Remover favorito a partir da página de favoritos
     *
     * Objetivo:
     *  - Verificar que ao remover um favorito na lista, este desaparece da UI.
     */
    @Test
    fun removerFavorito_daLista_e_DeixaDeAparecer() {
        rule.activityRule.scenario.onActivity { activity ->
            activity.navController.navigate("favoritos")
        }

        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("favoritoCard").fetchSemanticsNodes().isNotEmpty()
        }

        val favoritosAntes =
            rule.onAllNodesWithTag("favoritoCard").fetchSemanticsNodes().size

        rule.onAllNodesWithTag("btnFavorite")[0].performClick()
        Thread.sleep(1500)

        rule.waitUntil(10_000) {
            val depois =
                rule.onAllNodesWithTag("favoritoCard").fetchSemanticsNodes().size
            depois <= favoritosAntes - 1
        }
    }
}
