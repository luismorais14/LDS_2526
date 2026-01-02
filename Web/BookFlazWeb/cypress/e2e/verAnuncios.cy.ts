describe ('Ver Anúncios', () => {
  beforeEach(() => {
    cy.visit('/');
  })

  it('Teste pesquisar Anuncios', () => {
    cy.get('[data-cy=search-input]').type('A hipótese do amor');
    cy.contains('A hipótese do amor').should('be.visible');
  });

  it('Scroll na página e clicar em carregar mais anúncios deve carregar mais', () => {
    cy.scrollTo('bottom');
    cy.get('[data-cy=load-more-button]').click();
    cy.get('[data-cy=anuncio-card]').its('length').should('be.gte', 12);
  });

  it("Abrir e fechar conversa de um anúncio", () => {
    cy.get('[data-cy=conversa-item]').first().click();
    cy.get('[data-cy=chat-component]').should('be.visible');
    cy.get('[data-cy=close-chat-button]').click();
    cy.get('[data-cy=chat-component]').should('not.exist');
  })
});
