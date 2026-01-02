export class Anuncio {
    id: number;
    titulo: string;
    imagem: string; // URL da capa (singular)
    
    // --- O CAMPO QUE FALTA ---
    imagens?: string; // String com todas as imagens separadas por ';'
    // -------------------------

    preco: number;
    categoria: string;
    estadoLivro: number;
    tipoAnuncio: number;
    totalFavoritos: number;
    favorito: boolean;

    constructor(
        id: number, 
        titulo: string, 
        imagem: string, 
        preco: number, 
        categoria: string, 
        estadoLivro: number, 
        tipoAnuncio: number, 
        totalFavoritos: number, 
        favorito: boolean,
        imagens?: string // Adicionado ao construtor
    ) {
        this.id = id;
        this.titulo = titulo;
        this.imagem = imagem;
        this.preco = preco;
        this.categoria = categoria;
        this.estadoLivro = estadoLivro;
        this.tipoAnuncio = tipoAnuncio;
        this.totalFavoritos = totalFavoritos;
        this.favorito = favorito;
        this.imagens = imagens;
    }
}