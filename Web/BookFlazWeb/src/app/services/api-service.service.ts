import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';
import { Anuncio } from '../Models/Anuncio/anuncio.model';
import { Categoria } from '../Models/Categoria/categoria';
import { Conversa } from '../Models/Conversa/conversa';

const endpoint = 'https://flazbooksapi-dncpfkfmd6e8dwbj.germanywestcentral-01.azurewebsites.net/api/';
const httpOptions = {
  headers : new HttpHeaders({
    'Content-Type': 'application/json'
  })
}

@Injectable({
  providedIn: 'root',
})
export class ApiServiceService {
  constructor(private http: HttpClient) {}

  /**
   * Obtém a lista de anúncios da API.
   * @returns Um Observable contendo um array de Anuncio.
   */
  getAnuncios() : Observable<{ sucesso: boolean; total: number; anuncios: Anuncio[] }> {
    return this.http.get<{ sucesso: boolean; total: number; anuncios: Anuncio[] }>(endpoint + 'anuncio', httpOptions);
  }

  /**
   * Obtém os detalhes de um anúncio específico da API.
   * @param anuncioId - O ID do anúncio a ser obtido.
   * @returns Um Observable contendo o Anuncio.
   */
  getAnuncioPorId(anuncioId: number): Observable<Anuncio> {
    return this.http.get<Anuncio>(endpoint + `Anuncio/${anuncioId}`, httpOptions);
  }

  /**
   * Obtém a lista de categorias disponíveis da API.
   * @returns Um Observable contendo um array de Categoria.
   */
  getCategorias(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(endpoint + 'Categoria/categorias/disponiveis', httpOptions);
  }

  /**
   * Obtém a lista de conversas da API.
   * @returns Um Observable contendo um array de conversas.
   */
  getConversas(): Observable<Conversa[]> {
    return this.http.get<any[]>(endpoint + 'Chat/conversas', httpOptions).pipe(
      map(data => data.map(jsonItem => {
        return {
                id: jsonItem.id,
                DataCriacao: new Date(jsonItem.dataCriacao),
                AnuncioId: jsonItem.anuncioId,              
                CompradorId: jsonItem.compradorId,          
                VendedorId: jsonItem.vendedorId            
            } as Conversa;
        }))
    );
  }

  /**
     * Envia para o backend os dados do anúncio que o utilizador preencheu
     * no formulário, incluindo imagens (através de FormData).
     *
     * @param formData — Objeto FormData contendo todas as propriedades do anúncio
     * @returns Observable com a resposta do backend
     */
    criarAnuncio(formData: FormData): Observable<any> {
        return this.http.post(endpoint + 'anuncio', formData);
    }

    /**
   * Realiza uma chamada HTTP para a API pública do Google Books
   * e devolve título e autores do resultado encontrado.
   *
   * Fluxo da função:
   *  1. Construção da URL utilizando o ISBN recebido
   *  2. Envio da requisição com HttpClient
   *  3. Transformação dos dados da resposta com map()
   *  4. Validação de existência de resultados
   *  5. Tratamento de erros com catchError
   *
   * @param isbn — Código ISBN para pesquisa do livro
   * @returns Observable com título e autores
   */
  pesquisaPorISBN(isbn: string): Observable<{ title: string, authors: string }> {

    // Endpoint do Google Books com filtro específico por ISBN
    const url = `https://www.googleapis.com/books/v1/volumes?q=isbn:${isbn}`;

    return this.http.get<any>(url) //Faz a chamada HTTP GET para a API Google Books, devolvendo um Observable
            .pipe ( // permite aplicar operadores para transformar/validar os dados da resposta

      // Transforma os dados recebidos em algo útil
      map(response => {

        // Se o array `items` não existe ou está vazio → ISBN não encontrado
        if (!response.items?.length) {
          throw 'ISBN não encontrado.';
        }

        // A API devolve um array de livros — é utilizado o primeiro item como resultado
        const info = response.items[0].volumeInfo;

        // Retorna somente os dados que interessam ao preenchimento automático
        return {
          title: info.title || '', // Caso venha undefined, devolvemos string vazia
          authors: info.authors?.join(', ') || '' // Lista convertida para string
        };
      }),

      /**
       * Tratamento de erros da requisição
       * Exemplo: falha de internet, API indisponível, etc.
       */
      catchError(() =>
        throwError(() => 'Erro ao aceder Google Books.'))
    );
  }
}
