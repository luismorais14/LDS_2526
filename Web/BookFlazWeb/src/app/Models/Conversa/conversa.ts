export class Conversa {
    id : number;
    DataCriacao : Date;
    AnuncioId: number;
    CompradorId: number;
    VendedorId: number;

    constructor(id: number, dataCriacao: Date, anuncioId: number, compradorId: number, vendedorId: number) {
        this.id = id;
        this.DataCriacao = dataCriacao;
        this.AnuncioId = anuncioId;
        this.CompradorId = compradorId;
        this.VendedorId = vendedorId;
    }
}

export interface ConversaDetalhe extends Conversa {
    tituloAnuncio: string;
    precoAnuncio: number;
}
