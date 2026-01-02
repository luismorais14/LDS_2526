import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ChatItem, Conversa, ConversaDetalhesDTO, EstadoPedidoFront, Mensagem, PedidoTransacao, PedidoTransacaoApi } from '../models/chatModels';


@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private baseUrl = 'https://flazbooksapi-dncpfkfmd6e8dwbj.germanywestcentral-01.azurewebsites.net/api';
  constructor(private http: HttpClient) { }

   obterMensagensDaConversa(
  conversaId: number
): Observable<{ conversa: Conversa; itens: ChatItem[] }> {
  return this.http
    .get<ConversaDetalhesDTO>(`${this.baseUrl}/Chat/${conversaId}`)
    .pipe(
      map((dto) => {
        console.log('DTO recebido:', dto);

        const mensagens: Mensagem[] = dto.mensagens?.mensagens ?? [];
        const pedidosApi: PedidoTransacaoApi[] = (dto.mensagens?.pedidos ?? []) as PedidoTransacaoApi[];
        
        const pedidos: PedidoTransacao[] = pedidosApi.map(p => ({
          id: p.id,
          estado: this.mapEstadoPedido(p.estadoPedido),
          preco: p.valorProposto,
          dataCriacao: p.dataCriacao,
          remetenteId: p.remetenteId,
          diasDeAluguel: p.diasDeAluguel
        }));

        console.log(pedidos);
        console.log(pedidosApi);

        const itens: ChatItem[] = [
          ...mensagens.map((m) => ({ tipo: 'mensagem', mensagem: m } as ChatItem)),
          ...pedidos.map((p) => ({ tipo: 'pedido', pedido: p } as ChatItem)),
        ];

        itens.sort((a, b) => {
          const dataA =
            a.tipo === 'mensagem'
              ? new Date(a.mensagem.dataEnvio).getTime()
              : new Date(a.pedido.dataCriacao).getTime();

          const dataB =
            b.tipo === 'mensagem'
              ? new Date(b.mensagem.dataEnvio).getTime()
              : new Date(b.pedido.dataCriacao).getTime();

          return dataA - dataB;
        });

        return {
          conversa: dto.conversa,
          itens,
        };
      })
    );
}


  enviarMensagem(conteudo: string, anuncioId : number, conversaId?: number): Observable<void> {
    var payload: { conteudo: string; anuncioId: number; conversaId?: number } = { conteudo, anuncioId};

    if (conversaId) {
      payload.conversaId = conversaId;
    }

    return this.http.post<void>(`${this.baseUrl}/Chat/EnviarMensagem`, payload);
  }

  enviarPedido(valorProposto: number, anuncioId: number, diasDeAluguel: number, conversaId?: number): Observable<void> {
    var payload: { valorProposto: number; anuncioId: number; diasDeAluguel: number; conversaId?: number } = { valorProposto, anuncioId, diasDeAluguel};

    if (conversaId) {
      payload.conversaId = conversaId;
    }

    return this.http.post<void>(`${this.baseUrl}/PedidoTransacao`, payload);
  }

  aceitarPedido(pedidoId: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/PedidoTransacao/${pedidoId}/aceitar`, {});
  }

  recusarPedido(pedidoId: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/PedidoTransacao/${pedidoId}/rejeitar`, {});
  }

  obterClienteIdAtual(): Observable<number> {
    return this.http.get<any>(`${this.baseUrl}/Cliente/me`).pipe(
      map(response => {
        console.log('Resposta do cliente atual:', response);

        if (typeof response === 'number') {
          return response;
        }

        return response.clienteId;
      })
    );
  }

  private mapEstadoPedido(estado: number | EstadoPedidoFront | string): EstadoPedidoFront {
    if (typeof estado === 'string') {
      return estado as EstadoPedidoFront;
    }

    switch (estado) {
      case 0: return 'Pendente';
      case 1: return 'Aceite';
      case 2: return 'Rejeitado';
      case 3: return 'Cancelado';
      default: return 'Pendente';
    }
  }
}
