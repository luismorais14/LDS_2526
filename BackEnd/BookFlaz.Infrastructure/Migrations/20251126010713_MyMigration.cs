using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFlaz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MyMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Dob = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Pontos = table.Column<int>(type: "int", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    NotificacoesAtivas = table.Column<bool>(type: "bit", nullable: false),
                    IsAtivo = table.Column<bool>(type: "bit", nullable: false),
                    ReputacaoMedia = table.Column<double>(type: "float", nullable: false),
                    TotalAvaliacoes = table.Column<int>(type: "int", nullable: false),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Livros",
                columns: table => new
                {
                    Isbn = table.Column<long>(type: "bigint", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Autor = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livros", x => x.Isbn);
                });

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Conteudo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoNotificacao = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Clientes_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Anuncios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    EstadoLivro = table.Column<string>(type: "varchar(20)", nullable: false),
                    TipoAnuncio = table.Column<string>(type: "varchar(20)", nullable: false),
                    EstadoAnuncio = table.Column<string>(type: "varchar(20)", nullable: false),
                    Imagens = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoriaId = table.Column<long>(type: "bigint", nullable: false),
                    LivroIsbn = table.Column<long>(type: "bigint", nullable: false),
                    VendedorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anuncios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anuncios_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Anuncios_Clientes_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Anuncios_Livros_LivroIsbn",
                        column: x => x.LivroIsbn,
                        principalTable: "Livros",
                        principalColumn: "Isbn",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnuncioId = table.Column<long>(type: "bigint", nullable: false),
                    CompradorId = table.Column<long>(type: "bigint", nullable: false),
                    VendedorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversas_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Conversas_Clientes_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Conversas_Clientes_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataAdicionado = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnuncioId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favoritos_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Conteudo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    ConversaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensagens_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Mensagens_Conversas_ConversaId",
                        column: x => x.ConversaId,
                        principalTable: "Conversas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PedidosTransacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValorProposto = table.Column<double>(type: "float", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoAnuncio = table.Column<int>(type: "int", nullable: false),
                    EstadoPedido = table.Column<int>(type: "int", nullable: false),
                    RemetenteId = table.Column<long>(type: "bigint", nullable: false),
                    DestinatarioId = table.Column<long>(type: "bigint", nullable: false),
                    AnuncioId = table.Column<long>(type: "bigint", nullable: false),
                    CompradorId = table.Column<long>(type: "bigint", nullable: false),
                    VendedorId = table.Column<long>(type: "bigint", nullable: false),
                    ConversaId = table.Column<long>(type: "bigint", nullable: false),
                    DiasDeAluguel = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosTransacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosTransacao_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PedidosTransacao_Clientes_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PedidosTransacao_Clientes_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PedidosTransacao_Clientes_RemetenteId",
                        column: x => x.RemetenteId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PedidosTransacao_Clientes_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PedidosTransacao_Conversas_ConversaId",
                        column: x => x.ConversaId,
                        principalTable: "Conversas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transacoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValorFinal = table.Column<double>(type: "float", nullable: false),
                    PontosUsados = table.Column<int>(type: "int", nullable: false),
                    ValorDesconto = table.Column<double>(type: "float", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoTransacao = table.Column<int>(type: "int", nullable: false),
                    PedidoId = table.Column<long>(type: "bigint", nullable: false),
                    CompradorId = table.Column<long>(type: "bigint", nullable: false),
                    VendedorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transacoes_Clientes_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacoes_Clientes_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacoes_PedidosTransacao_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "PedidosTransacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estrelas = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransacaoId = table.Column<long>(type: "bigint", nullable: false),
                    AvaliadoId = table.Column<long>(type: "bigint", nullable: false),
                    AvaliadorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Clientes_AvaliadoId",
                        column: x => x.AvaliadoId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Clientes_AvaliadorId",
                        column: x => x.AvaliadorId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Transacoes_TransacaoId",
                        column: x => x.TransacaoId,
                        principalTable: "Transacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devolucoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clienteId = table.Column<long>(type: "bigint", nullable: false),
                    DataRegisto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataRececaoVendedor = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Confirmada = table.Column<bool>(type: "bit", nullable: false),
                    TransacaoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devolucoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devolucoes_Clientes_clienteId",
                        column: x => x.clienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Devolucoes_Transacoes_TransacaoId",
                        column: x => x.TransacaoId,
                        principalTable: "Transacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentosPontos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    DataMovimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoMovimento = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    TransacaoId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentosPontos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentosPontos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentosPontos_Transacoes_TransacaoId",
                        column: x => x.TransacaoId,
                        principalTable: "Transacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_CategoriaId",
                table: "Anuncios",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_LivroIsbn",
                table: "Anuncios",
                column: "LivroIsbn");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_VendedorId",
                table: "Anuncios",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_AvaliadoId",
                table: "Avaliacoes",
                column: "AvaliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_AvaliadorId",
                table: "Avaliacoes",
                column: "AvaliadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_TransacaoId",
                table: "Avaliacoes",
                column: "TransacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_AnuncioId",
                table: "Conversas",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_CompradorId",
                table: "Conversas",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_VendedorId",
                table: "Conversas",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Devolucoes_clienteId",
                table: "Devolucoes",
                column: "clienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Devolucoes_TransacaoId",
                table: "Devolucoes",
                column: "TransacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_AnuncioId",
                table: "Favoritos",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_ClienteId",
                table: "Favoritos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_ClienteId",
                table: "Mensagens",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_ConversaId",
                table: "Mensagens",
                column: "ConversaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentosPontos_ClienteId",
                table: "MovimentosPontos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentosPontos_TransacaoId",
                table: "MovimentosPontos",
                column: "TransacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_ClientId",
                table: "Notificacoes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosTransacao_AnuncioId",
                table: "PedidosTransacao",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosTransacao_CompradorId",
                table: "PedidosTransacao",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosTransacao_ConversaId",
                table: "PedidosTransacao",
                column: "ConversaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosTransacao_DestinatarioId",
                table: "PedidosTransacao",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosTransacao_RemetenteId",
                table: "PedidosTransacao",
                column: "RemetenteId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosTransacao_VendedorId",
                table: "PedidosTransacao",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_CompradorId",
                table: "Transacoes",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_PedidoId",
                table: "Transacoes",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_VendedorId",
                table: "Transacoes",
                column: "VendedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avaliacoes");

            migrationBuilder.DropTable(
                name: "Devolucoes");

            migrationBuilder.DropTable(
                name: "Favoritos");

            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropTable(
                name: "MovimentosPontos");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "Transacoes");

            migrationBuilder.DropTable(
                name: "PedidosTransacao");

            migrationBuilder.DropTable(
                name: "Conversas");

            migrationBuilder.DropTable(
                name: "Anuncios");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Livros");
        }
    }
}
