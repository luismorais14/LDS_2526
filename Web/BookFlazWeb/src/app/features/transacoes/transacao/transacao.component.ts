import { Component, OnInit } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TransacaoService } from '../../../services/transacao.service';
import { ClienteService } from '../../../services/cliente.service';
import { ApiServiceService } from '../../../services/api-service.service';
import { PedidoTransacao } from '../../../Models/PedidoTransacao/pedidotransacao';

@Component({
  selector: 'app-transacao',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transacao.component.html',
  styleUrls: ['./transacao.component.css']
})
export class TransacaoComponent implements OnInit {
  pedido?: PedidoTransacao;
  isLoading = true;
  erroMsg = '';

  usarPontos = false;
  meusPontosDisponiveis = 0;
  pontosAUsar = 0;
  
  readonly VALOR_POR_PONTO = 0.05; 
  readonly DESCONTO_MAXIMO_PERCENTUAL = 0.5;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private transacaoService: TransacaoService,
    private clienteService: ClienteService,
    private apiService: ApiServiceService,
    private location: Location
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.carregarDados(id);
    } else {
      this.erroMsg = 'Pedido inválido.';
      this.isLoading = false;
    }
  }

  carregarDados(pedidoId: number): void {
    this.isLoading = true;

    this.transacaoService.getPedidoById(pedidoId).subscribe({
      next: (dadosPedido) => {
        this.pedido = dadosPedido;
        
        if (this.pedido && this.pedido.anuncioId) {
          this.apiService.getAnuncioPorId(this.pedido.anuncioId).subscribe({
            next: (anuncio) => {
              if (this.pedido) {
                this.pedido.tituloAnuncio = anuncio.titulo;
                
                if (anuncio.imagens) {
                  const imagens = anuncio.imagens.split(';');
                  const imagemValida = imagens.find(img => img && img.trim() !== '' && img.toLowerCase() !== 'null');
                  if (imagemValida) this.pedido.imagemAnuncio = imagemValida;
                }
              }
            },
            error: (err) => console.warn('Não foi possível carregar detalhes do anúncio', err)
          });
        }

        this.loadClientePontos();
      },
      error: (err) => {
        this.erroMsg = 'Erro ao carregar os detalhes do pedido.';
        this.isLoading = false;
      }
    });
  }

  private loadClientePontos(): void {
    console.log('--- A pedir perfil do cliente... ---');

    this.clienteService.getCliente().subscribe({
      next: (data: any) => {
        console.log('RESPOSTA PERFIL:', data);

        if (data && typeof data.pontos === 'number') {
            this.meusPontosDisponiveis = data.pontos;
        } else {
            console.warn('Atenção: A API não devolveu "pontos" ou devolveu nulo.', data);
            this.meusPontosDisponiveis = 0;
        }

        console.log('Pontos definidos no componente:', this.meusPontosDisponiveis);

        this.pontosAUsar = this.maxPontosPermitidos;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('ERRO AO CARREGAR PONTOS:', err);
        this.meusPontosDisponiveis = 0;
        this.isLoading = false;
      }
    });
  }


  get maxPontosPermitidos(): number {
    if (!this.pedido) return 0;
    const descontoMaximoEmEuros = this.pedido.valorProposto * this.DESCONTO_MAXIMO_PERCENTUAL;
    const maxPontosPelaRegra = Math.floor(descontoMaximoEmEuros / this.VALOR_POR_PONTO);
    
    return Math.min(this.meusPontosDisponiveis, maxPontosPelaRegra);
  }

  get valorDesconto(): number {
    return this.usarPontos ? (this.pontosAUsar * this.VALOR_POR_PONTO) : 0;
  }

  get totalFinal(): number {
    if (!this.pedido) return 0;
    return this.pedido.valorProposto - this.valorDesconto;
  }

  confirmarPagamento(): void {
    if (!this.pedido) return;

    const dto = {
      pedidoId: this.pedido.id,
      pontosUsados: this.usarPontos ? this.pontosAUsar : 0,
      valorFinal: this.totalFinal
    };

    this.transacaoService.criarTransacao(dto).subscribe({
      next: () => {
        alert('Pagamento confirmado com sucesso!');
        this.router.navigate(['/']); 
      },
      error: (err) => {
        alert(err.error?.mensagem || 'Erro ao processar o pagamento.');
        this.router.navigate(['/']);
      }
    });
  }

  cancelar(): void {
    this.location.back();
  }
}