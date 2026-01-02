export type EstadoPedidoFront = 'Pendente' | 'Aceite' | 'Rejeitado' | 'Cancelado';

export interface Mensagem {
  id: number;
  conteudo: string;
  dataEnvio: string;
  clienteId: number;
  conversaId?: number;
}

export interface PedidoTransacao {
  id: number;
  estado: EstadoPedidoFront;
  preco: number;
  dataCriacao: string;
  remetenteId: number;
  diasDeAluguel: number;
}

export interface PedidoTransacaoApi {
  id: number;
  estadoPedido: number;  
  valorProposto: number;
  dataCriacao: string;
  remetenteId: number;
  diasDeAluguel: number;
}

export interface Conversa {
  id: number;
  dataCriacao: string;
  anuncioId: number;
  compradorId: number;
  vendedorId: number;
}

export interface ConversaDetalhesDTO {
  conversa: Conversa;
  mensagens: {
    mensagens: Mensagem[];
    pedidos: PedidoTransacaoApi[];
  };
}

export type ChatItem =
  | { tipo: 'mensagem'; mensagem: Mensagem }
  | { tipo: 'pedido'; pedido: PedidoTransacao };