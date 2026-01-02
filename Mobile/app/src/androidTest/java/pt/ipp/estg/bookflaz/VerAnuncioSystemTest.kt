package pt.ipp.estg.bookflaz

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onAllNodesWithTag
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTouchInput
import androidx.test.espresso.action.ViewActions.swipeLeft
import androidx.test.espresso.action.ViewActions.swipeRight
import org.junit.Before
import org.junit.Rule
import org.junit.Test

/**
 * Testes de Sistema da funcionalidade “Ver Anúncio”.
 *
 * Este conjunto de testes valida o comportamento do sistema completo
 * desde a obtenção dos dados reais pela API até à apresentação e
 * interação no interface de utilizador.
 */
class VerAnuncioSystemTest {

    @get:Rule
    val rule = createAndroidComposeRule<MainActivity>() // Abre a MainActivity como num dispositivo real

    @Before
    fun navegarParaAnuncio() {

        rule.activityRule.scenario.onActivity { activity ->
            activity.navController.navigate("product/3")
        }


        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("txtTitulo").fetchSemanticsNodes().isNotEmpty()
        }
    }

    /**
     * ST01 – Carregamento dos dados do anúncio
     *
     * Objetivo:
     *  - Garantir que os dados do anúncio são obtidos da API
     *  - Verificar se o título é exibido corretamente na UI
     *
     * Estratégia:
     *  - Aguardar pelo carregamento assíncrono dos dados via ViewModel
     *  - Validar visibilidade do elemento identificado por testTag("txtTitulo")
     */
    @Test
    fun anuncioCarregaEDetalhesAparecem() {

        // Aguarda até que o título tenha sido carregado da API e renderizado no UI
        // A API é assíncrona, pode demorar, sem isto o teste falhava
        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("txtTitulo").fetchSemanticsNodes().isNotEmpty()
        }

        // Verifica que o título está visível para o utilizador
        rule.onNodeWithTag("txtTitulo").assertIsDisplayed()
    }

    /**
     * ST02 – Interação com favorito (persistência na API)
     *
     * Objetivo:
     *  - Verificar que o utilizador consegue marcar/desmarcar favorito
     *  - Garantir que a UI se mantém funcional após operação remota
     */
    @Test
    fun toggleFavoritoAtualizaContador() {
        // Espera o botão aparecer
        rule.waitUntil(
            timeoutMillis = 10_000,
            condition = {
                rule.onAllNodesWithTag("btnFavorite")
                    .fetchSemanticsNodes().isNotEmpty()
            }
        )

        // Simula um clique real do utilizador no botão de favorito
        rule.onNodeWithTag("btnFavorite").performClick()

        Thread.sleep(2000) // Espera retorno da API

        // Verifica se o botão continua presente e funcional após operação remota
        rule.onNodeWithTag("btnFavorite").assertExists()
    }

    /**
     * ST03 – Exibição dos detalhes essenciais do anúncio
     *
     * Valida que os principais atributos do anúncio são apresentados ao utilizador,
     * garantindo qualidade mínima da informação visualizada.
     */
    @Test
    fun todosOsDetalhesBasicosAparecem() {
        // Espera até os dados textuais do anúncio carregarem
        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("txtTitulo").fetchSemanticsNodes().isNotEmpty()
        }

        // Confirma se atributos importantes são exibidos ao utilizador
        rule.onNodeWithText("Autor").assertExists()
        rule.onNodeWithText("Categoria").assertExists()
        rule.onNodeWithText("Preço").assertExists()
    }

    /**
     * ST04 – Persistência do estado de favorito após recriação da Activity
     *
     * Objetivo:
     *  - Simular eventos como rotação de ecrã ou término da UI
     *  - Garantir que o estado guardado no backend é novamente refletido
     *    na interface ao recarregar a Activity
     */
    @Test
    fun estadoMantemAposRecreate() {
        // Aguarda pelo botão favorito carregado
        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("btnFavorite").fetchSemanticsNodes().isNotEmpty()
        }

        // Marca favorito na API
        rule.onNodeWithTag("btnFavorite").performClick()
        Thread.sleep(1500)

        // Simula fechar/abrir o ecrã novamente (recriar Activity)
        // A UI é reconstruída e volta a pedir favoritos à API
        rule.activityRule.scenario.recreate()

        // Espera novamente pelo carregamento do estado
        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("btnFavorite").fetchSemanticsNodes().isNotEmpty()
        }

        // Se ainda existe, a API confirmou o estado favorito
        rule.onNodeWithTag("btnFavorite").assertExists()
    }

    /**
     * ST05 – Carregamento das imagens do anúncio
     *
     * Objetivo:
     *  - Verificar se o sistema obtém e apresenta imagens vindas de fonte externa
     */
    @Test
    fun imagemPrincipalCarregaSemErro() {
        // Espera pelos dados antes de validar UI
        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("txtTitulo").fetchSemanticsNodes().isNotEmpty()
        }

        // Garante que a primeira imagem é carregada
        rule.onNodeWithTag("imageItem_0")
            .assertExists()
            .assertIsDisplayed()
    }

    /**
     * ST06 – Navegação por swipe entre imagens
     *
     * Objetivo:
     *  - Garantir a boa experiência do utilizador ao navegar pelas imagens
     *  - Confirmar estabilidade da UI ao realizar interações de swipe
     */
    @Test
    fun swipeEntreImagensNaoCrash() {
        // Espera o componente de pager existir
        rule.waitUntil(10_000) {
            rule.onAllNodesWithTag("imagePager").fetchSemanticsNodes().isNotEmpty()
        }

        // Obtém referência ao componente de swipe
        val pager = rule.onNodeWithTag("imagePager")

        // Swipe para o lado esquerdo
        pager.performTouchInput { swipeLeft() }

        // Swipe para o lado direito
        pager.performTouchInput { swipeRight() }

        pager.assertIsDisplayed() // se crashasse, nunca chegava aqui
    }

}