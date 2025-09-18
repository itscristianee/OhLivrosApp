// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OhLivrosApp.Data;
using OhLivrosApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace OhLivrosApp.Areas.Identity.Pages.Account
{

    public class RegisterModel : PageModel
    {

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context
            )
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        /// este objeto será usado para fazer a transposição de dados entre este
        /// ficheiro (de programação) e a sua respetiva visualização
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// se for instanciado, este atributo terá o link para onde a aplicação
        /// será redirecionada, após Registo
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Se estiver especificado a Autenticação por outros fornecedores
        /// de autenticação, este atributo terá essa lista de outros fornecedores
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// define os atributos que estarão presentes na interface da página
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// email do novo utilizador
            /// </summary>
            [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
            [EmailAddress(ErrorMessage = "Tem de escrever um {0} válido.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            /// password associada ao utilizador
            /// </summary>
            [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
            [StringLength(20, ErrorMessage = "A {0} tem de ter, pelo menos, {2} e um máximo de {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            /// confirmação da password
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar password")]
            [Compare(nameof(Password), ErrorMessage = "A password e a sua confirmação não coincidem.")]
            public string ConfirmPassword { get; set; }



            /// <summary>
            /// Incorporação dos dados de um Utilizador
            /// no formulário de Registo
            /// </summary>
            [Required]
            public Utilizador Utilizador { get; set; } = new();
        }


        /// <summary>
        /// Este método 'responde' aos pedidos feitos em HTTP GET
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }


        /// <summary>
        /// Trata o POST do registo: cria IdentityUser + regista Utilizador (tua tabela)
        /// numa única transação. Envia email de confirmação e autentica (se permitido).
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            // 1) Criar o utilizador do Identity
            var user = CreateUser();
            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            // 2) Transação: Identity + Utilizador (tua BD)
            using var tx = await _context.Database.BeginTransactionAsync();

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            try
            {
                // PONTE CORRETA: guarda o Id do Identity no teu Utilizador
                Input.Utilizador.UserName = user.Id;

                _context.Add(Input.Utilizador);
                await _context.SaveChangesAsync();

                await tx.CommitAsync(); // tudo ok: confirma a transação
            }
            catch (Exception)
            {
                await tx.RollbackAsync();          // volta atrás
                await _userManager.DeleteAsync(user); // remove o Identity criado
                ModelState.AddModelError("", "Ocorreu um erro ao guardar os dados do utilizador. Tente novamente.");
                return Page();
            }

            _logger.LogInformation("Utilizador criado com sucesso.");

            // 3) Envio do email de confirmação
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId, code, returnUrl },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Confirmar email",
                $"Por favor confirme a sua conta clicando <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>aqui</a>."
            );

            // 4) Redireciona conforme configuração de conta confirmada
            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
        }


        /// <summary>
        /// Cria um objeto vazio do tipo IdentityUser
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
