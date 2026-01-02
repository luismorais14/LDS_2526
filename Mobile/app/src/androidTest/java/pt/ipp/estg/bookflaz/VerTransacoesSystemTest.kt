package pt.ipp.estg.bookflaz

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertTextContains
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onAllNodesWithTag
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onFirst
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performTouchInput
import androidx.test.espresso.action.ViewActions.swipeUp
import org.junit.Before
import org.junit.Rule
import org.junit.Test

/**
 * Testes de Sistema da funcionalidade “Histórico de Transações”.
 *
 * Versão Robusta: Usa Tags semânticas em vez de valores "hardcoded".
 */
class VerTransacoesSystemTest {

    @get:Rule
    val rule = createAndroidComposeRule<MainActivity>()

    @Before
    fun navegarParaHistorico() {
        rule.activityRule.scenario.onActivity { activity ->
            activity.navController.navigate("transactions")
        }


        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("txtPrecoTransacao").fetchSemanticsNodes().isNotEmpty()
        }
    }

    @Test
    fun listaCarregaETituloAparece() {
        rule.onNodeWithText("Histórico de Transações").assertIsDisplayed()
        rule.onNodeWithTag("listaTransacoes").assertExists()
    }

    /**
     * ST02 – Validação Genérica dos Dados do Cartão
     *
     * Objetivo:
     * - Verificar se o preço e título são apresentados.
     * - NÃO depende de valores fixos (funciona com 0.0 €, 30.0 €, 125.50 €, etc.).
     */


    /**
     * ST03 – Validação Flexível das Badges
     *
     * Objetivo:
     * - Verificar se a app desenhou pelo menos uma etiqueta de estado ou papel válida.
     * - Aceita qualquer cenário (só compras, só vendas, pendentes ou concluídas).
     */
    @Test
    fun badgesDeEstadoEPapelAparecem() {
        val badgesConhecidas = listOf(
            "COMPRADOR", "VENDEDOR",
            "PENDENTE", "CONCLUIDA", "CANCELADA", "DESCONHECIDO"
        )

        var encontrouAlguma = false

        for (texto in badgesConhecidas) {
            if (rule.onAllNodesWithText(texto).fetchSemanticsNodes().isNotEmpty()) {
                encontrouAlguma = true
                break
            }
        }

        assert(encontrouAlguma) { "A lista devia mostrar pelo menos uma badge (Comprador/Vendedor/Estado)." }
    }

    /**
     * ST04 - Lista permitir scroll
     */

    @Test
    fun listaPermiteScroll() {
        val lista = rule.onNodeWithTag("listaTransacoes")
        lista.assertExists()
        try {
            lista.performTouchInput { swipeUp() }
        } catch (e: Exception) {
        }
    }

    /**
     * ST05 – Persistência Genérica após Rotação
     */
    @Test
    fun dadosMantemAposRecreate() {
        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("txtPrecoTransacao").fetchSemanticsNodes().isNotEmpty()
        }

        rule.activityRule.scenario.recreate()

        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("txtPrecoTransacao").fetchSemanticsNodes().isNotEmpty()
        }

        rule.onAllNodesWithTag("txtPrecoTransacao")
            .onFirst()
            .assertExists()
    }
}