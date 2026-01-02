/**
 * Componente responsável pela criação de anúncios de livros na aplicação.
 *
 * Implementa:
 *  - Formulário reativo com validação de campos
 *  - Upload e pré-visualização de imagens
 *  - Auto-preenchimento de título/autor via Google Books API usando ISBN
 *  - Submissão dos dados e imagens ao backend
 */

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormGroup,
} from '@angular/forms';

import { NotificacaoService } from '../../../core/services/notificacao.service';

import {
  validarImagem,
  gerarPreview,
  MAX_IMAGENS,
} from '../utils/imagem.helper';
import { Categoria } from '../../../Models/Categoria/categoria';
import { ApiServiceService } from '../../../services/api-service.service';
import { MatIcon } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';

@Component({
  selector: 'app-criar-anuncio',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIcon,
    RouterModule,
  ],
  templateUrl: './criar-anuncio.component.html',
})
export class CriarAnuncioComponent {
  /**
   * Formulário reativo do anúncio
   * Contém: ISBN, título, autor, categoria, estado, tipo anúncio e preço
   */
  form!: FormGroup;
  imagens: File[] = []; // Lista real de ficheiros selecionados
  previewImagens: string[] = []; // Lista apenas das previews base64 para mostrar no UI
  categorias: Categoria[] = []; // Dados carregados do backend para dropdown de categorias

  // Feedback ao utilizador em validações específicas
  erroImagem: string | null = null;
  erroIsbn: string | null = null;

  submitted = false; // Indica se o utilizador tentou submeter o formulário

  constructor(
    private fb: FormBuilder,
    private apiService: ApiServiceService,
    private notificacao: NotificacaoService,
    private router: Router
  ) {}

  /**
   * Inicializa o formulário e carrega categorias disponíveis
   * Reage à mudança do tipo de anúncio para mostrar ou remover o campo preço
   */
  ngOnInit() {
    this.form = this.fb.group({
      isbn: ['', [Validators.required]],
      titulo: [{ value: '', disabled: true }],
      autor: [{ value: '', disabled: true }],
      categoriaId: ['', Validators.required],
      estadoLivro: ['', Validators.required],
      tipoAnuncio: ['', Validators.required],
      preco: ['', [Validators.min(0.01), Validators.max(10000)]],
    });

    // Quando o tipo muda, o preço pode ficar desativado, neste caso na 'DOACAO'
    this.form.get('tipoAnuncio')?.valueChanges.subscribe((value) => {
      this.mostrarCampoPreco(value !== 'DOACAO');
    });

    this.carregarCategorias(); // carrega todas as categorias
  }

  /**
   * Ativa ou desativa a validação do campo "preço" consoante o tipo de anúncio.
   *
   * - Quando `ativo = false` (anúncio de doação):
   *    - Remove as validações do campo preço
   *    - Limpa o valor atual do campo
   *
   * - Quando `ativo = true`:
   *    - Define o campo como obrigatório
   *    - Aplica limites mínimo e máximo ao valor
   *
   * No fim, é sempre chamado `updateValueAndValidity()` para
   * forçar o Angular a recalcular o estado de validação do campo.
   *
   * @param ativo Indica se o campo preço deve estar ativo e validado
   */
  mostrarCampoPreco(ativo: boolean) {
    const precoControl = this.form.get('preco');

    if (!ativo) {
      // Tipo de anúncio não exige preço
      precoControl?.clearValidators(); // Remove validação
      precoControl?.setValue(''); // limpa o valor para não enviar lixo ao backend
    } else {
      // Tipo de anúncio pago
      precoControl?.setValidators([
        Validators.required, // Passa a ser obrigatório
        Validators.min(0.01), // Valor deve estar entre 0.01 e 10000
        Validators.max(10000), // Valor deve estar entre 0.01 e 10000
      ]);
    }

    precoControl?.updateValueAndValidity(); // Recalcula o estado (valid/invalid) após alteração das regras
  }

  /**
   * Carrega categorias do backend para dropdown
   * Apenas categorias marcadas como disponíveis
   */
  carregarCategorias() {
    this.apiService.getCategorias().subscribe({
      next: (res) => (this.categorias = res), // Preenche dropdown
      error: (err) => console.error('Erro ao carregar categorias', err), // Log útil para desenvolvimento
    });
  }

  /**
   * Processa as imagens selecionadas pelo utilizador no input de ficheiros.
   *
   * @param event Evento emitido pelo input type="file"
   */
  onImagensSelecionadas(event: Event) {
    const input = event.target as HTMLInputElement;

    // Se não existir ficheiros então não faz nada
    if (!input.files?.length) {
      return;
    }

    const files = Array.from(input.files); // Converte FileList em array de File

    this.erroImagem = null; // Limpa erros anteriores

    for (const file of files) {
      const erro = validarImagem(file); // Faz a validação do tipo e do tamanho

      if (erro) {
        // Se houver algum erro
        this.erroImagem = erro; // Mostra motivo do erro específico
        continue; // Passa para o próximo ficheiro
      }

      if (this.imagens.length >= MAX_IMAGENS) {
        // Se tiver o limite máximo de imagens "5 neste momento"
        this.erroImagem = `Não podes adicionar mais de ${MAX_IMAGENS} imagens.`;
        return; // Aqui já não pode adicionar mais nenhuma
      }

      this.imagens.push(file); // Adiciona a imagem a lista de imagens

      //Gera pré-visualização (async → push quando estiver pronta)
      gerarPreview(file).then((base64) => {
        this.previewImagens.push(base64);
      });
    }
  }

  /**
   * Remove uma imagem previamente adicionada ao anúncio.
   *
   * @param i Índice da imagem a ser removida
   */
  removerImagem(i: number) {
    this.imagens.splice(i, 1); // Remove o ficheiro real da imagem

    this.previewImagens.splice(i, 1); // Remove a preview exibida no UI
  }

  /**
   * Valida formato do ISBN (10 ou 13 dígitos)
   * Se for válido, tenta buscar automaticamente os dados do livro
   */
  validarISBN() {
    const isbn = this.form.get('isbn')?.value.replace(/[-\s]/g, ''); // Remove traços e espaços

    this.erroIsbn = null; // Limpa erro anterior

    // Regex: apenas ISBN com 10 ou 13 dígitos são aceites
    if (!/^\d{10}$|^\d{13}$/.test(isbn)) {
      this.erroIsbn = 'ISBN deve ter 10 ou 13 dígitos.';
      return; // Bloqueia a pesquisa na API
    }

    // Faz a pesquisa na api
    this.buscarLivroGoogle(isbn);
  }

  /**
   * Consulta a API Google Books para preencher automaticamente
   * os campos "título" e "autor" com base no ISBN fornecido.
   *
   * @param isbn ISBN já validado (10 ou 13 dígitos)
   */
  buscarLivroGoogle(isbn: string) {
    this.apiService.pesquisaPorISBN(isbn).subscribe({
      // Obtém os dados, e faz a atualização dos campos do formulário
      next: (info) => {
        this.form.patchValue({
          titulo: info.title,
          autor: info.authors,
        });
      },

      // Em caso de erro, envia para o utilizador
      error: (err) => {
        this.erroIsbn = err;
      },
    });
  }

  /**
   * Valida o formulário e as imagens e, se estiver tudo correto,
   * constrói um FormData e envia o anúncio para o backend.
   *
   * Regras de validação aplicadas:
   *  - Todos os campos obrigatórios do formulário têm de ser válidos
   *  - Tem de existir pelo menos uma imagem
   *  - Não pode ultrapassar o número máximo de imagens permitido
   *  - O ISBN tem de ter 10 ou 13 dígitos (após remover espaços e hífens)
   *
   * Em caso de erro:
   *  - Define mensagens específicas em `erroImagem` e/ou `erroIsbn`
   *  - Mostra uma notificação genérica para o utilizador
   *
   * Em caso de sucesso:
   *  - Envia os dados via `AnuncioService`
   *  - Mostra notificação de sucesso
   */
  publicarAnuncio() {
    this.submitted = true; // Marca que o utilizador tentou submeter o formulário

    this.form.markAllAsTouched(); // Força a exibição de todos os erros de validação do reactive form

    // Limpa mensagens de erro anteriores
    this.erroImagem = null;
    this.erroIsbn = null;
    let temErroExtra = false;

    // Pelo menos uma imagem obrigatória
    if (this.imagens.length === 0) {
      this.erroImagem = 'Adiciona pelo menos uma imagem do livro.';
      temErroExtra = true;
    }

    // Não pode exceder o número máximo de imagens
    if (this.imagens.length > MAX_IMAGENS) {
      this.erroImagem = `Não podes adicionar mais de ${MAX_IMAGENS} imagens.`;
      temErroExtra = true;
    }

    // Normaliza ISBN removendo espaços e hífens
    const isbnRaw = this.form.get('isbn')?.value || '';
    const isbn = isbnRaw.replace(/[-\s]/g, '');

    // Validar formato do ISBN (10 ou 13 dígitos)
    if (!/^\d{10}$|^\d{13}$/.test(isbn)) {
      this.erroIsbn = 'ISBN deve ter 10 ou 13 dígitos.';
      temErroExtra = true;
    }

    // Se o formulário tiver erros de validação ou regras extra falharem
    if (this.form.invalid || temErroExtra) {
      this.notificacao.erro('Corrige os erros antes de publicar o anúncio.');

      return;
    }

    // getRawValue() devolve também campos desativados (ex.: título/autor)
    const formValues = this.form.getRawValue();
    const fd = new FormData();

    // Campos simples do anúncio
    fd.append('LivroIsbn', formValues.isbn);
    fd.append('CategoriaId', formValues.categoriaId);
    fd.append('EstadoLivro', formValues.estadoLivro);
    fd.append('TipoAnuncio', formValues.tipoAnuncio);

    // Preço é opcional no caso de doação
    if (formValues.preco) {
      fd.append('Preco', formValues.preco);
    }

    // Adiciona cada imagem ao FormData
    this.imagens.forEach((img) => fd.append('Imagens', img));

    // Envia anúncio ao backend e trata feedback visual
    this.apiService.criarAnuncio(fd).subscribe({
      next: () => {
        this.notificacao.sucesso('Anúncio criado com sucesso!');

        // Espera 1 segundo só para o utilizador ver a notificação
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 1000);
      },
      error: () => {
        this.notificacao.erro('Erro ao criar o anúncio.');
        // Mantém-se na página atual — não fazer navigate
      },
    });
  }
}
