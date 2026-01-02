import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ChatService } from './chat.service';
import { ChatItem, Mensagem, PedidoTransacao, Conversa } from '../models/chatModels';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {
  @Input() conversaId!: number;

  @Output() fechar = new EventEmitter<void>();

  currentUserId?: number; 
  chatItems: ChatItem[] = [];
  conversa!: Conversa;
  novaMensagem: string = '';
  modoEnvio: 'mensagem' | 'pedido' = 'mensagem';
  precoPedido: number | null = null;
  diasPedido: number | null = null;

  constructor(private chatService: ChatService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {

    if (!this.conversaId) {
      const idParam = this.route.snapshot.paramMap.get('conversaId');
      if (idParam) {
        this.conversaId = Number(idParam);
      }
    }

    if (!this.conversaId) {
      console.error('conversaId não definido (nem por Input, nem por rota).');
      return;
    }

    this.chatService.obterClienteIdAtual().subscribe({
      next: id => {
        console.log('ID do utilizador atual obtido:', id);
        this.currentUserId = id;
      },
      error: err => console.error('Erro ao obter ID do utilizador atual:', err)
    });


    this.chatService.obterMensagensDaConversa(this.conversaId)
      .subscribe({
        next: resposta => {
          this.conversa = resposta.conversa;
          this.chatItems = resposta.itens;
        },
        error: err => console.error('Erro a carregar conversa:', err)
      });
  }

  isMinhaMensagem(item: ChatItem): boolean {
    return item.tipo === 'mensagem' && item.mensagem.clienteId === this.currentUserId;
  }

  onEnviarMensagem(): void {
    if (!this.novaMensagem.trim()) return;

    if (this.currentUserId === undefined) {
      console.error('ID do utilizador atual não está definido');
      return;
    }

    const nova: Mensagem = {
      id: Date.now(),
      conteudo: this.novaMensagem,
      dataEnvio: new Date().toISOString(),
      clienteId: this.currentUserId,
      conversaId: this.conversaId
    };

    this.chatService.enviarMensagem(this.novaMensagem, this.conversa.anuncioId, this.conversaId).subscribe({
      next: () => {
        this.chatItems.push({ tipo: 'mensagem', mensagem: nova });
        this.novaMensagem = '';
      },
      error: err => console.error('Erro ao enviar mensagem:', err)
    });
  }

  onEnviarPedido(): void {
    console.log('Enviando pedido com preço:', this.precoPedido, 'e dias:', this.diasPedido);
    if (this.precoPedido == null || this.precoPedido <= 0) return;
    if (this.diasPedido == null || this.diasPedido <= 0) return;

    if (this.currentUserId === undefined) {
      console.error('ID do utilizador atual não está definido');
      return;
    }

    this.chatService.enviarPedido(
      this.precoPedido,
      this.conversa.anuncioId,
      this.diasPedido,
      this.conversaId
    ).subscribe({
      next: () => {
        const novoPedido: PedidoTransacao = {
          id: Date.now(),
          estado: 'Pendente',
          preco: this.precoPedido!,
          dataCriacao: new Date().toISOString(),
          remetenteId: this.currentUserId!,
          diasDeAluguel: this.diasPedido!
        };

        this.chatItems.push({ tipo: 'pedido', pedido: novoPedido });

        this.precoPedido = null;
        this.diasPedido = null;
        this.modoEnvio = 'mensagem';
      },
      error: err => console.error('Erro ao enviar pedido:', err)
    });
  }

  onAceitarPedido(pedido: PedidoTransacao) {
    this.chatService.aceitarPedido(pedido.id).subscribe({
      next: () => {
        console.log('Pedido aceite com sucesso:', pedido);
        pedido.estado = 'Aceite';
      },
      error: err => console.error('Erro ao aceitar pedido:', err)
    });
  }

  onRecusarPedido(pedido: PedidoTransacao) {
    this.chatService.recusarPedido(pedido.id).subscribe({
      next: () => {
        console.log('Pedido recusado com sucesso:', pedido);
        pedido.estado = 'Rejeitado';
      },
      error: err => console.error('Erro ao recusar pedido:', err)
    });
  }

  onFecharClicked(): void {
    this.fechar.emit();  
  }

  podeGerirPedido(pedido: PedidoTransacao): boolean {
    return pedido.remetenteId !== this.currentUserId;
  }

  podeMostrarBotoes(p: PedidoTransacao): boolean {
    return this.podeGerirPedido(p) && p.estado === 'Pendente';
  }

  onIrParaTransacao(pedido: PedidoTransacao): void {
    this.router.navigate(['/transacao', pedido.id]);
  }
}
