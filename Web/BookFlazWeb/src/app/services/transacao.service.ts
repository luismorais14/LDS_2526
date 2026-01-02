import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PedidoTransacao } from '../Models/PedidoTransacao/pedidotransacao';

@Injectable({
  providedIn: 'root'
})
export class TransacaoService {
  
  private apiUrl = 'https://flazbooksapi-dncpfkfmd6e8dwbj.germanywestcentral-01.azurewebsites.net/api/'; 

  constructor(private http: HttpClient) {}

 
  getPedidoById(id: number): Observable<PedidoTransacao> {
    return this.http.get<PedidoTransacao>(`${this.apiUrl}PedidoTransacao/${id}`);
  }

  /**
   * Cria a transação final (confirma o pagamento).
   */
  criarTransacao(dados: { pedidoId: number, pontosUsados: number, valorFinal: number }): Observable<any> {
    return this.http.post(`${this.apiUrl}Transacao`, dados);
  }
}