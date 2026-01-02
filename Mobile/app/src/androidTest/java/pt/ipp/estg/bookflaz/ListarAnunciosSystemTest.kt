package pt.ipp.estg.bookflaz

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertIsNotDisplayed
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onAllNodesWithTag
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onFirst
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performImeAction
import androidx.compose.ui.test.performTextClearance
import androidx.compose.ui.test.performTextInput
import org.junit.Before
import org.junit.Rule
import org.junit.Test

/**
 * Testes de Sistema da funcionalidade “Listar Anúncios”.
 *
 */
class ListarAnunciosSystemTest {
    @get:Rule
    val rule = createAndroidComposeRule<MainActivity>()

    @Before
    fun navegarParaListarAnuncios() {
        rule.activityRule.scenario.onActivity { activity ->
            activity.navController.navigate("home")
        }
    }

    /**
     * ST01 – Filtragem por intervalo de preço válido
     *
     */
    @Test
    fun filtarAnunciosPorPrecoValido() {
        rule.onNodeWithTag("btn_open_filters").performClick()
        rule.onNodeWithTag("input_price_min").performTextClearance()
        rule.onNodeWithTag("input_price_min").performTextInput("30")
        rule.onNodeWithTag("input_price_max").performTextClearance()
        rule.onNodeWithTag("input_price_max").performTextInput("50")
        rule.onNodeWithTag("btn_apply_filters").performClick()
        rule.onAllNodesWithText("Harry Potter e la pietra filosofale. Ediz. illustrata").onFirst().assertIsDisplayed()
        rule.onNodeWithText("A hipótese do amor").assertIsNotDisplayed()
    }

    /**
     * ST02 – Filtragem por categoria válida
     *
     */
    @Test
    fun filtrarAnunciosPorCategoriaValido() {
        rule.onNodeWithTag("btn_open_filters").performClick()
        rule.onNodeWithTag("Venda_checkbox").performClick()
        rule.onNodeWithTag("Doação_checkbox").performClick()
        rule.onNodeWithTag("btn_apply_filters").performClick()
        rule.onAllNodesWithText("Aluguer").onFirst().assertIsDisplayed()
        rule.onNodeWithText("Doação").assertIsNotDisplayed()
    }

    /**
     * ST03 – Filtragem por intervalo de preço inválido
     *
     */
    @Test
    fun filtrarAnuncioFiltroInvalido() {
        rule.onNodeWithTag("btn_open_filters").performClick()
        rule.onNodeWithTag("input_price_min").performTextClearance()
        rule.onNodeWithTag("input_price_min").performTextInput("90")
        rule.onNodeWithTag("input_price_max").performTextClearance()
        rule.onNodeWithTag("input_price_max").performTextInput("100")
        rule.onNodeWithTag("btn_apply_filters").performClick()
        rule.onNodeWithTag("anuncio_card").assertDoesNotExist()
    }

    /**
     * ST04 – Filtragem por texto válido
     *
     */
    @Test
    fun filtrarAnunciosPorTextoValido() {
        rule.onNodeWithTag("input_search").performTextInput("Harry Potter")
        rule.onAllNodesWithText("Harry Potter e la pietra filosofale. Ediz. illustrata").onFirst().assertIsDisplayed()
        rule.onNodeWithText("A hipótese do amor").assertIsNotDisplayed()
    }

    /**
     * ST05 – Abrir página do anúncio
     *
     */
    @Test
    fun abrirPaginaAnuncio() {
        rule.onAllNodesWithTag("anuncio_card").onFirst().performClick()
        rule.onNodeWithTag("txtTitulo").assertIsDisplayed()
    }
}
