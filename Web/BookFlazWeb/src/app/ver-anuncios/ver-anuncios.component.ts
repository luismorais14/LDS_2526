import { Component, OnInit } from '@angular/core';
import { ApiServiceService } from '../services/api-service.service';
import { Anuncio } from '../Models/Anuncio/anuncio.model';

import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { Categoria } from '../Models/Categoria/categoria';
import { TipoAnuncio } from '../Models/Enums/tipoAnuncio';
import { FormsModule } from '@angular/forms';
import { Conversa, ConversaDetalhe } from '../Models/Conversa/conversa';
import { forkJoin } from 'rxjs';
import { ChatComponent } from "../chat/chat.component";
import { RouterModule } from '@angular/router';
import {MatSliderModule} from '@angular/material/slider';

@Component({
  selector: 'app-ver-anuncios',
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatChipsModule,
    MatTabsModule,
    ChatComponent,
    RouterModule,
    MatSliderModule
],
  templateUrl: './ver-anuncios.component.html',
  styleUrl: './ver-anuncios.component.css',
})
export class VerAnunciosComponent implements OnInit {
  todosAnuncios: Anuncio[] = [];
  listaAnuncios: Anuncio[] = [];
  listaCategorias: Categoria[] = [];
  TipoAnuncio = TipoAnuncio;
  tipoAnuncio?: TipoAnuncio;
  listaTiposAnuncio: string[] = [];
  anunciosVisiveis: Anuncio[] = [];
  tamanhoPagina = 12;

  filtroTexto: string = '';
  filtroCategoria: string | null = null;
  filtroTipoAnuncio: string | null = null;
  filtroOpen: Boolean = false;
  filtroPrecoMin: number = 0;
  filtroPrecoMax: number = 300;
  filtroPrecoMaxGlobal: number = 300;

  conversas: Conversa[] = [];
  conversasEnriquecidas: ConversaDetalhe[] = [];
  indiceConversaAtual: number = 0;
  isChatOpen: boolean = false;

  constructor(
    public apiService: ApiServiceService,
  ) { }

  /**
   * Inicializa o componente carregando anúncios, categorias, tipos de anúncio e conversas.
   */
  ngOnInit(): void {
    this.carregarAnuncios();
    this.carregarCategorias();
    this.carregarTiposAnuncio();
    this.carregarConversas();
  }

  /**
   * Carrega os anúncios da API e atualiza a lista de anúncios.
   */
  carregarAnuncios(): void {
    this.apiService.getAnuncios().subscribe({
      next: (data: { sucesso: boolean; total: number; anuncios: Anuncio[] }) => {
        if (data && Array.isArray(data.anuncios)) {
          this.listaAnuncios = data.anuncios;
          this.todosAnuncios = data.anuncios;

          if (this.todosAnuncios.length > 0) {
            this.filtroPrecoMaxGlobal = Math.max(...this.todosAnuncios.map(a => a.preco));

            this.filtroPrecoMin = 0;
            this.filtroPrecoMax = this.filtroPrecoMaxGlobal;
          }

          this.atualizarVisiveis(true);
        } else {
          this.todosAnuncios = [];
          this.listaAnuncios = [];
          this.anunciosVisiveis = [];
        }
      }, error : (error) => {
        console.error('Erro ao buscar anúncios:', error);
      }
      });
  }

  /**
   * Carrega as categorias da API e atualiza a lista de categorias.
   */
  carregarCategorias(): void {
    this.apiService.getCategorias().subscribe({
      next: (data: Categoria[]) => {
        this.listaCategorias = data;
      },
      error: (error) => {
        console.error('Erro ao buscar categorias:', error);
      }
    });
  }

  /**
   * Carrega os tipos de anúncio disponíveis.
   */
  carregarTiposAnuncio(): void {
    this.listaTiposAnuncio = Object.keys(TipoAnuncio).filter(key => isNaN(Number(key)));
  }

  /**
   * Atualiza a lista de anúncios visíveis com base na paginação.
   */
  carregarMais(): void {
    this.atualizarVisiveis(false);
  }

  /**
   * Atualiza a lista de anúncios visíveis com base na paginação.
   * @param reiniciar Indica se deve reiniciar a lista visível ou adicionar mais anúncios.
   */
  atualizarVisiveis(reiniciar: boolean = false): void {
    if (reiniciar) {
      this.anunciosVisiveis = this.listaAnuncios.slice(0, this.tamanhoPagina);
    } else {
      const atuais = this.anunciosVisiveis.length;
      const novos = this.listaAnuncios.slice(atuais, atuais + this.tamanhoPagina);
      this.anunciosVisiveis = [...this.anunciosVisiveis, ...novos];
    }
  }

  /**
   * Abre ou fecha o painel de filtros.
   */
  abrirFiltros(): void {
    if (this.filtroOpen) {
      this.filtroOpen = false;
    } else {
      this.filtroOpen = true;
    }
  }

  /**
   * Aplica os filtros selecionados à lista de anúncios.
   */
  aoPesquisar(): void {
    this.aplicarFiltros();
  }

  /**
   * Aplica os filtros selecionados à lista de anúncios.
   * @param evento Evento de mudança de filtro.
   */
  aoSelecionarCategoria(evento: any): void {
    if (evento.index === 0) {
      this.filtroCategoria = null;
    } else {
      this.filtroCategoria = this.listaCategorias[evento.index - 1].nome;
    }

    this.aplicarFiltros();
  }

  /**
   * Aplica os filtros selecionados à lista de anúncios.
   * @param tipo Tipo de anúncio selecionado.
   */
  aoSelecionarTipoAnuncio(tipo: string): void {
    if (this.filtroTipoAnuncio === tipo) {
      this.filtroTipoAnuncio = null;
    } else {
      this.filtroTipoAnuncio = tipo;
    }
    this.aplicarFiltros();
  }

  /**
   * Carrega as conversas da API e atualiza a lista de conversas.
   */
  carregarConversas(): void {
    this.apiService.getConversas().subscribe({
      next: (data: Conversa[]) => {
        this.conversas = data;
        this.processarConversas();
      },
      error: (error) => {
        console.error('Erro ao buscar conversas:', error);
      }
    });
  }

  /**
   * Obtém os detalhes do anúncio para cada conversa usando forkJoin.
   */
  processarConversas(): void {
      if (this.conversas.length === 0) {
          this.conversasEnriquecidas = [];
          return;
      }

      const anuncioIdsUnicos = [...new Set(this.conversas.map(c => c.AnuncioId))];

      const anuncioRequests = anuncioIdsUnicos.map(id =>
          this.apiService.getAnuncioPorId(id)
      );

      forkJoin(anuncioRequests).subscribe({
          next: (anuncios: Anuncio[]) => {
              const anuncioMap = new Map<number, Anuncio>();
              anuncioIdsUnicos.forEach((id, index) => {
                  anuncioMap.set(id, anuncios[index]);
              });

              this.conversasEnriquecidas = this.conversas.map((conversa) => {
                  const anuncio = anuncioMap.get(conversa.AnuncioId);
                  return {
                      ...conversa,
                      tituloAnuncio: anuncio?.titulo || 'Anúncio Indisponível',
                      precoAnuncio: anuncio?.preco || 0,
                  } as ConversaDetalhe;
              });

          },
          error: (err) => {
              console.error('Erro ao buscar detalhes dos anúncios:', err);
              this.conversasEnriquecidas = this.conversas.map(c => ({
                  ...c,
                  tituloAnuncio: 'Erro ao carregar anúncio',
                  precoAnuncio: 0
              } as ConversaDetalhe));
          }
      });
  }


  private aplicarFiltros(): void {
    const texto = this.filtroTexto.toLowerCase().trim();
    this.listaAnuncios = this.todosAnuncios.filter(anuncio => {
      const correspondeTexto = anuncio.titulo.toLowerCase().includes(texto);
      const correspondeCategoria = this.filtroCategoria === null || anuncio.categoria === this.filtroCategoria;
      const tipoAnuncioString = TipoAnuncio[anuncio.tipoAnuncio];
      const correspondeTipoAnuncio = this.filtroTipoAnuncio === null || tipoAnuncioString === this.filtroTipoAnuncio;
      const precoAnuncio = anuncio.preco || 0;
      const dentroFaixaPreco = precoAnuncio >= this.filtroPrecoMin && precoAnuncio <= this.filtroPrecoMax;

      return correspondeTexto && correspondeCategoria && correspondeTipoAnuncio && dentroFaixaPreco;
    });
    this.atualizarVisiveis(true);
  }

  /**
   * Abre o painel de chat para a conversa selecionada.
   * @param conversa Conversa selecionada.
   */
  abrirChat(conversa: ConversaDetalhe): void {

    const novoId = conversa.id;

    if (this.isChatOpen && this.indiceConversaAtual !== novoId) {
        this.isChatOpen = false;

        setTimeout(() => {
            this.indiceConversaAtual = novoId;
            this.isChatOpen = true;
        }, 0);

    } else if (!this.isChatOpen) {
        this.indiceConversaAtual = novoId;
        this.isChatOpen = true;
    }
}

  /**
   * Fecha o painel de chat.
   */
  fecharChat(): void {
    this.isChatOpen = false;
    this.indiceConversaAtual = -1;
  }

  /**
   * Formata o rótulo do slider adicionando o símbolo de euro.
   * @param value Valor do slider.
   * @returns Rótulo formatado.
   */
  formatLabel(value: number): string {
    return `${value}` + '€';
  }
}
