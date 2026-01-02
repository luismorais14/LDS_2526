/**
 * Serviço responsável por exibir notificações visuais do tipo “toast”
 * no topo da interface, para feedback rápido ao utilizador.
 */

import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root', // Serviço disponível globalmente na aplicação
})

export class NotificacaoService {

  /**
   * Exibe uma notificação de sucesso com estilo verde.
   *
   * @param mensagem Texto a ser exibido ao utilizador
   */
  sucesso(mensagem: string) {
    this.mostrarToast(mensagem, 'bg-success text-white');
  }

  /**
   * Exibe uma notificação de erro com estilo vermelho.
   *
   * @param mensagem Texto a ser exibido ao utilizador
   */
  erro(mensagem: string) {
    this.mostrarToast(mensagem, 'bg-danger text-white');
  }

  /**
   * Cria e exibe uma notificação tipo toast no topo da tela.
   *
   * - Cria dinamicamente um `<div>`
   * - Aplica classes e estilos inline com animação
   * - Insere dentro de `#toast-container`
   * - Remove automaticamente após 3 segundos
   *
   * @param mensagem Texto exibido dentro do toast
   * @param classe Classes visuais Bootstrap (cor do estado)
   */
  private mostrarToast(mensagem: string, classe: string) {
    const container = document.getElementById('toast-container');
    const toastEl = document.createElement('div');

    toastEl.className = `toast show fw-semibold ${classe}`;
    toastEl.style.position = 'fixed';
    toastEl.style.top = '20px';
    toastEl.style.left = '50%';
    toastEl.style.transform = 'translateX(-50%)';
    toastEl.style.padding = '12px 24px';
    toastEl.style.borderRadius = '8px';
    toastEl.style.fontSize = '1rem';
    toastEl.style.zIndex = '2000';
    toastEl.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';
    toastEl.style.opacity = '0';
    toastEl.style.transition = 'opacity 0.3s ease-in-out';

    toastEl.innerText = mensagem;

    // Insere no container configurado no HTML da app
    container?.appendChild(toastEl);

    // Fade-in suave
    requestAnimationFrame(() => {
      toastEl.style.opacity = '1';
    });

    // Fade-out e remoção após 3 segundos
    setTimeout(() => {
      toastEl.style.opacity = '0';
      setTimeout(() => toastEl.remove(), 300);
    }, 3000);
  }
}
