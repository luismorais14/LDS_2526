using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Repositories;
using BookFlaz.Infrastructure.Data;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace BookFlaz.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IPasswordHasher<Cliente> hasher;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Construtor que injeta as dependências necessárias para o serviço de gestão de clientes.
        /// </summary>
        /// <param name="context">Instância do contexto de dados <see cref="BooksContext"/>.</param>
        /// <param name="hasher">Instância do serviço de hashing de senhas <see cref="IPasswordHasher{Cliente}"/>.
        /// </param> 
        public ClienteService(IClienteRepository repo, IPasswordHasher<Cliente> hasher, IEmailService emailService)
        {
            _clienteRepository = repo;
            this.hasher = hasher;
            _emailService = emailService;
        }

        /// <summary>
        /// Cria um novo utilizador na plataforma.
        /// </summary>
        /// <param name="dto">Dados necessários para a criação do utilizador.</param>
        /// <returns>Tupla indicando sucesso ou falha da operação e mensagem associada.</returns>
        public async Task<(bool Success, String Message)> CriarUtilizadorAsync(RegistarClienteDTO client)
        {
            var existingUser = await _clienteRepository.ObterPorEmailAsync(client.Email);

            if (existingUser != null)
            {
                return (false, "Já existe um utilizador com este email.");
            }

            var clientModel = client.Adapt<Cliente>();

            clientModel.SetPasswordHash(hasher.HashPassword(clientModel, clientModel.PasswordHash));

            string email = clientModel.Email;
            if (!IsEmailValid(email))
            {
                return (false, "Email inválido.");
            }

            int idade = DateTime.Now.Year - clientModel.Dob.Year;
            if (idade < 18)
            {
                return (false, "Deve ter uma idade mínima de 18 anos.");
            }

            var result = await _clienteRepository.GuardarAsync(clientModel);
            if (!result)
            {
                return (false, "Erro ao criar o cliente");
            }

            return (true, "Cliente criado com sucesso.");
        }

        public async Task<(bool Success, String Message)> LoginUtilizadorAsync(string email, string password)
        {
            var existingUser = await _clienteRepository.ObterPorEmailAsync(email);

            if (existingUser == null)
            {
                return (false, "Cliente não existe.");
            }

            var result = hasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, password);

            switch (result)
            {
                case PasswordVerificationResult.Success:
                    return (true, "Login efetuado com sucesso.");
                case PasswordVerificationResult.Failed:
                    return (false, "Senha incorreta.");
                case PasswordVerificationResult.SuccessRehashNeeded:
                    existingUser.SetPasswordHash(hasher.HashPassword(existingUser, password));
                    await _clienteRepository.AtualizarAsync(existingUser);
                    return (true, "Login efetuado com sucesso.");
                default:
                    return (false, "Credenciais inválidas");
            }
        }

        public async Task<(bool Success, String Message)> EditarUtilizadorAsync(long id, ClienteDTO client)
        {
            var existingUser = await _clienteRepository.ObterPorIdAsync(id);

            if (existingUser == null)
            {
                return (false, "Cliente não existe.");
            }

            client.Adapt(existingUser);


            if (!string.IsNullOrWhiteSpace(client.PasswordHash))
            {
                existingUser.SetPasswordHash(hasher.HashPassword(existingUser, client.PasswordHash));
            }

            var result = await _clienteRepository.AtualizarAsync(existingUser);
            if (!result)
            {
                return (false, "Erro ao atualizar o cliente.");
            }
            return (true, "Cliente atualizado com sucesso.");
        }

        public async Task<(bool Success, String Message)> EliminarUtilizadorAsync(long id)
        {
            var existingUser = await _clienteRepository.ObterPorIdAsync(id);
            if (existingUser == null)
            {
                return (false, "Cliente não existe.");
            }

            existingUser.Inativar();
            
            var result = await _clienteRepository.AtualizarAsync(existingUser);
            if (!result)
            {
                return (false, "Erro ao eliminar o cliente.");
            }
            return (true, "Cliente eliminado com sucesso.");
        }

        /// <summary>
        /// Valida o formato do email.
        /// </summary>
        /// <param name="email">Email a ser validado.</param>
        /// <returns>True se o email for válido, caso contrário false.</returns>
        private static bool IsEmailValid(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsAdminAsync(long idUser)
        {
            try
            {
                if (idUser <= 0)
                    throw new ArgumentOutOfRangeException(nameof(idUser), "ID inválido.");

                var user = await _clienteRepository.ObterPorIdAsync(idUser)
                           ?? throw new NotFoundException("Utilizador não encontrado.");

                return user.IsAdmin; 
            }
            catch (NotFoundException) 
            { 
                throw;
            }
            catch (ArgumentOutOfRangeException) 
            { 
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao verificar privilégios de administrador.", ex);
            }
        }

        public async Task<bool> RecebeNotificacaoAsync(long idUser)
        {
            try
            {
                if (idUser <= 0)
                    throw new ArgumentOutOfRangeException(nameof(idUser), "ID inválido.");

                var user = await _clienteRepository.ObterPorIdAsync(idUser);
                if (user is null)
                    throw new NotFoundException("Utilizador não encontrado.");

                return user.NotificacoesAtivas;
            }
            catch (NotFoundException) 
            { 
                throw; 
            }
            catch (ArgumentOutOfRangeException) 
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao obter preferência de notificações.", ex);
            }
        }

        public async Task<bool> AlterarPreferenciaNotificacaoAsync(long id, bool preferencia)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentOutOfRangeException(nameof(id), "ID inválido.");

                var user = await _clienteRepository.ObterPorIdAsync(id)
                           ?? throw new NotFoundException("Utilizador não encontrado.");

                if (user.NotificacoesAtivas == preferencia)
                    return true;

                user.EditarNotificacoesAtivas(preferencia);
                await _clienteRepository.AtualizarAsync(user);

                return true;
            }
            catch (NotFoundException) 
            { 
                throw; 
            }
            catch (ArgumentOutOfRangeException) 
            { 
                throw; 
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao alterar a preferência de notificações.", ex);
            }
        }
        
        /// <summary>
        /// Inicia o processo de reset de password.
        /// </summary>
        /// <param name="email">O email do utilizador.</param>
        public async Task<(bool Success, string Message)> PedirResetPasswordAsync(string email)
        {
            var cliente = await _clienteRepository.ObterPorEmailAsync(email);
            if (cliente == null || !cliente.IsAtivo)
            {
                return (true, "Se existir uma conta com este email, um link de recuperação foi enviado.");
            }

            var token = Guid.NewGuid().ToString("N");

            cliente.SetPasswordResetToken(token);
            await _clienteRepository.AtualizarAsync(cliente);

            var resetLink = $"https://www.bookflaz.com/resetar-password?token={token}&email={cliente.Email}";
    
            string baseDir = AppContext.BaseDirectory;
            string templatePath = Path.Combine(baseDir, "EmailTemplates", "ResetPassword.html");
            string htmlBody = await File.ReadAllTextAsync(templatePath);

            htmlBody = htmlBody.Replace("[RESET_LINK_AQUI]", resetLink);

            try
            {
                await _emailService.SendEmailAsync(
                    cliente.Email,
                    "BookFlaz - Recuperação de Palavra-passe",
                    htmlBody
                );
                return (true, "Se existir uma conta com este email, um link de recuperação foi enviado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO DE SMTP]: {ex.Message}");
                return (false, "Ocorreu um erro ao enviar o email. Tente mais tarde.");
            }
        }

        public Task<Cliente?> ObterClientePorId(long id)
        {
            return _clienteRepository.ObterPorIdAsync(id);
        }
    }
}