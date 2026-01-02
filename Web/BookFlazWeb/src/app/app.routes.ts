import { Routes } from '@angular/router';
import { VerAnunciosComponent } from './ver-anuncios/ver-anuncios.component';

export const routes: Routes = [
  {
    path: '',
    component: VerAnunciosComponent,
    title: 'Ver Anúncios'
  },

  {
    path: 'criar/anuncio',
    loadComponent: () =>
      import('./features/anuncios/criar-anuncio/criar-anuncio.component')
        .then(m => m.CriarAnuncioComponent),
    title: 'Criar Anúncio'
  },

  { path: 'transacao/:id', loadComponent: () =>
    import('./features/transacoes/transacao/transacao.component')
      .then(m => m.TransacaoComponent),
    title: 'Transação'
  }


 
];
