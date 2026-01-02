describe('Criar Anúncio', () => {

  // Antes de cada teste, vai para a página de criar anúncio
  beforeEach(() => {
    cy.visit('/criar/anuncio');
  });

  it('Teste Criar Anúncio', () => {

    // Upload de imagens
    cy.get('[data-testid="input-imagens"]').selectFile(
      ['cypress/fixtures/livro1.jpg', 'cypress/fixtures/livro2.jpg'],
      { force: true }
    );

    // Preenchimento de campos obrigatórios
    cy.get('[data-testid="isbn-input"]').type('9788869183157');
    cy.get('[data-testid="categoria-select"]').select(1);
    cy.get('[data-testid="estado-select"]').select(1);
    cy.get('[data-testid="tipo-select"]').select('VENDA');
    cy.get('[data-testid="preco-input"]').type('7.50');

    // Interação do pedido POST ao backend
    cy.intercept('POST', '**/anuncio').as('criar');

    // Submissão do formulário
    cy.get('[data-testid="publicar-btn"]').click();

    // Verificação da resposta da API (status 200 esperado)
    cy.wait('@criar').its('response.statusCode').should('eq', 200);

    // Confirmação de feedback visual de sucesso ao utilizador
    cy.contains('Anúncio criado com sucesso!').should('be.visible');
  });
});


describe('Criar anúncio e verificar pelo título obtido via ISBN', () => {

  const isbn = '9781680101195'; // ISBN do livro Rapunzel

  it('cria um anúncio e verifica que aparece ao pesquisar pelo título', () => {

    cy.visit('/');

    cy.get('button[mat-fab][matTooltip="Criar Anúncio"]').click();
    cy.url().should('include', '/criar/anuncio');

    // Upload imagem
    cy.get('[data-testid="input-imagens"]').selectFile(
      ['cypress/fixtures/livro1.jpg'],
      { force: true }
    );

    // Inserir ISBN
    cy.get('[data-testid="isbn-input"]').clear().type(isbn);

    // Esperar título preenchido automaticamente e GUARDAR valor
    cy.get('input[formcontrolname="titulo"]')
      .invoke('val')
      .should('not.be.empty')
      .as('tituloLivro');

    // Esperar autor também
    cy.get('input[formcontrolname="autor"]')
      .invoke('val')
      .should('not.be.empty');

    // Preencher restantes campos
    cy.get('[data-testid="categoria-select"]').select(1);
    cy.get('[data-testid="estado-select"]').select(1);
    cy.get('[data-testid="tipo-select"]').select('VENDA');
    cy.get('[data-testid="preco-input"]').clear().type('7.50');

    // Submeter
    cy.get('[data-testid="publicar-btn"]')
      .should('not.be.disabled')
      .click();

    cy.contains('Anúncio criado com sucesso!').should('be.visible');

    // Espera navegação de 1 segundo
    cy.wait(1500);

    // Estamos agora na HOME
    cy.url().should('eq', `${Cypress.config().baseUrl}/`);

    // Usar o título guardado para pesquisar
    cy.get('@tituloLivro').then((titulo: any) => {

      // Pesquisar pelo título real
      cy.get('input[placeholder="O que procuras?"]').clear().type(titulo);

      // Deve aparecer pelo menos um anúncio
      cy.get('.ad-card').should('have.length.at.least', 1);

      // Confirma que contém o título
      cy.get('.ad-card').contains(titulo, { matchCase: false });
    });

  });
});
