using Humanizer.Localisation;
using Microsoft.AspNetCore.Mvc;
using OhLivrosApp.Models;
using OhLivrosApp.Repositorios;
using System.Collections.Generic;
using System.Diagnostics;

namespace OhLivrosApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Guardar referência ao repositório injetado
        private readonly HomeRepositorio _homeRepositorio;

        // Recebe o logger (para registo de mensagens) e o repositório (injeção de dependência no Controller)
        public HomeController(ILogger<HomeController> logger, HomeRepositorio homeRepositorio)
        {
            _homeRepositorio = homeRepositorio;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string termo = "", int generoId = 0)
        {
            IEnumerable <Livro> livros = _homeRepositorio.GetLivros(termo,generoId).Result;
            IEnumerable<Genero> generos = await _homeRepositorio.GetGeneros();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Sobre()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
