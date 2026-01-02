import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable} from 'rxjs';
import { Cliente } from '../Models/Cliente/cliente';

@Injectable({
  providedIn: 'root'
})
export class ClienteService {
  private apiUrl = 'https://flazbooksapi-dncpfkfmd6e8dwbj.germanywestcentral-01.azurewebsites.net/api/';

  constructor(private http: HttpClient) { }

  /**
   * Tenta obter o cliente. Se o backend devolver um array, usa o primeiro elemento.
   * Se devolver um objecto, retorna-o tal qual.
   */
  getCliente(): Observable<Cliente> {
    return this.http.get<Cliente>(`${this.apiUrl}Cliente/me`);
     }
}
