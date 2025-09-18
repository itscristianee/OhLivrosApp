using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OhLivrosApp.Models;
using OhLivrosApp.Models.DTO;
using OhLivrosApp.Repositorios;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static System.Reflection.Metadata.BlobBuilder;

namespace OhLivrosApp.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Guardar refer�ncia ao reposit�rio injetado
        private readonly IHomeRepositorio _homeRepositorio;

        // Recebe o logger (para registo de mensagens) e o reposit�rio (inje��o de depend�ncia no Controller)
        public HomeController(ILogger<HomeController> logger, IHomeRepositorio homeRepositorio)
        {
            _homeRepositorio = homeRepositorio;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string termo = "", int generoId = 0)
        {
            var livros = await _homeRepositorio.GetLivros(termo, generoId);
            var generos = await _homeRepositorio.GetGeneros();

            // Preenche o DTO de exibi��o com listas e filtros
            var livrosModel = new LivroModelDTO
            {
                Livros = livros,
                Generos = generos,
                Termo = termo,
                GeneroId = generoId
            };

            return View(livrosModel); 
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
