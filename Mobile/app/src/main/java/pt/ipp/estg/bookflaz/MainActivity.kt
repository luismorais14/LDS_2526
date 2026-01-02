package pt.ipp.estg.bookflaz

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import pt.ipp.estg.bookflaz.ui.MainScreen
import pt.ipp.estg.bookflaz.ui.navigation.AppNavGraph

/**
 * Activity principal da aplicação.
 *
 * É responsável por inicializar o ambiente Compose e configurar a
 * navegação global através do [AppNavGraph].
 *
 * Esta Activity representa o ponto de entrada da UI.
 */
class MainActivity : ComponentActivity() {

    // Propriedade acessível externamente para testes
    lateinit var navController: NavHostController
        private set

    /**
     * Criado quando a Activity é iniciada pela primeira vez.
     * Aqui definimos o conteúdo visual através de Compose,
     * incluindo o controlador de navegação para gerir os ecrãs.
     *
     * @param savedInstanceState Estado previamente guardado (caso exista).
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            val navHostController = rememberNavController()
            this.navController = navHostController

            MainScreen(navHostController)
        }

    }
}


