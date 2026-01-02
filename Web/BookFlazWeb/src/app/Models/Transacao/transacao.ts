export class Transacao {
  id: number;
  dataTransacao: string;
  compradorId: number;
  vendedorId: number;
  pedidoId: number;
  valorFinal: number;
  pontosUsados: number;
  valorDesconto: number;
  estadoTransacao: string;

  constructor(
    id: number,
    dataTransacao: string,
    compradorId: number,
    vendedorId: number,
    pedidoId: number,
    valorFinal: number,
    pontosUsados: number,
    valorDesconto: number,
    estadoTransacao: string
  ) {
    this.id = id;
    this.dataTransacao = dataTransacao;
    this.compradorId = compradorId;
    this.vendedorId = vendedorId;
    this.pedidoId = pedidoId;
    this.valorFinal = valorFinal;
    this.pontosUsados = pontosUsados;
    this.valorDesconto = valorDesconto;
    this.estadoTransacao = estadoTransacao;
  }
}

export class TransacaoResumo {
  id: number;
  data: string;
  estado: string;
  pedidoId: number;
  anuncioId?: number;
  tituloAnuncio?: string;
  imagemAnuncio?: string;
  preco?: number;
  tipoAnuncio?: string;
  outroUtilizadorId: number;
  valorFinal: number;
  pontosUsados: number;
  valorDesconto: number;
  papel: string;

  constructor(
    id: number,
    data: string,
    estado: string,
    pedidoId: number,
    outroUtilizadorId: number,
    valorFinal: number,
    pontosUsados: number,
    valorDesconto: number,
    papel: string,
    anuncioId?: number,
    tituloAnuncio?: string,
    imagemAnuncio?: string,
    preco?: number,
    tipoAnuncio?: string
  ) {
    this.id = id;
    this.data = data;
    this.estado = estado;
    this.pedidoId = pedidoId;
    this.anuncioId = anuncioId;
    this.tituloAnuncio = tituloAnuncio;
    this.imagemAnuncio = imagemAnuncio;
    this.preco = preco;
    this.tipoAnuncio = tipoAnuncio;
    this.outroUtilizadorId = outroUtilizadorId;
    this.valorFinal = valorFinal;
    this.pontosUsados = pontosUsados;
    this.valorDesconto = valorDesconto;
    this.papel = papel;
  }
}

export class TransacaoFiltro {
  estado?: string;
  tipo?: string;
  de?: Date;
  ate?: Date;
  papel?: string;

  constructor(
    estado?: string,
    tipo?: string,
    de?: Date,
    ate?: Date,
    papel?: string
  ) {
    this.estado = estado;
    this.tipo = tipo;
    this.de = de;
    this.ate = ate;
    this.papel = papel;
  }
}
