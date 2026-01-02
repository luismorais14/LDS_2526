import { Transacao, TransacaoResumo, TransacaoFiltro } from './transacao';

describe('Transacao', () => {
  it('should create an instance', () => {
    expect(new Transacao(1, new Date().toISOString(), 1, 2, 1, 25.50, 0, 0, 'PENDENTE')).toBeTruthy();
  });
});

describe('TransacaoResumo', () => {
  it('should create an instance', () => {
    expect(new TransacaoResumo(1, new Date().toISOString(), 'PENDENTE', 1, 2, 25.50, 0, 0, 'COMPRADOR')).toBeTruthy();
  });
});

describe('TransacaoFiltro', () => {
  it('should create an instance', () => {
    expect(new TransacaoFiltro()).toBeTruthy();
  });
});
