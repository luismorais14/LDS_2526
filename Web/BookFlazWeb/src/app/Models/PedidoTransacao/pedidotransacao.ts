export interface PedidoTransacao {
  id: number;
  valorProposto: number;
  
  anuncioId: number;
  tituloAnuncio: string; 
  imagemAnuncio?: string;
  
  compradorId: number;
  vendedorId: number;
  
  tipoAnuncio: string; 
  estado: string;      
}