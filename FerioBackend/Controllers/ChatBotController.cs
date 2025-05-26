using FerioBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace FerioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatBotController : ControllerBase
    {
        private readonly FerioDbContext _context;
        private readonly IConfiguration _config;

        public ChatBotController(FerioDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskChatbot([FromBody] ChatRequest request)
        {
            var pregunta = request.Message?.ToLower() ?? "";
            var contexto = await ConstruirContextoDesdeBD(pregunta);

            var mensajes = new List<object>();

            if (!string.IsNullOrWhiteSpace(contexto))
            {
                mensajes.Add(new { role = "system", content = "Datos de la feria:\n" + contexto });
            }

            mensajes.Add(new { role = "user", content = request.Message });

            var respuestaIA = await ConsultarOpenRouterConContexto(mensajes);

            return Ok(new { response = respuestaIA });
        }

        private async Task<string> ConstruirContextoDesdeBD(string pregunta)
        {
            var sb = new StringBuilder();

            var categorias = await _context.Categorias.ToListAsync();
            var stands = await _context.Stands
                .Include(s => s.Usuario)
                .Include(s => s.StandCategoria)
                    .ThenInclude(sc => sc.Categoria)
                .ToListAsync();

            var mensajes = await _context.Mensajes
                .Include(m => m.Usuario)
                .Include(m => m.Stand)
                .ToListAsync();

            // STANDS
            sb.AppendLine("Listado de Stands:");
            foreach (var stand in stands)
            {
                var categoriasStr = string.Join(", ", stand.StandCategoria.Select(sc => sc.Categoria.Nombre));
                sb.AppendLine($"- {stand.Nombre} | Categorías: {categoriasStr} | Horario: {stand.HorarioAtencion} | Contacto: {stand.Contacto} | Ubicación: {stand.Ubicacion}");
            }

            // CATEGORÍAS
            sb.AppendLine("\nCategorías disponibles:");
            foreach (var categoria in categorias)
            {
                sb.AppendLine($"- {categoria.Nombre}");
            }

            // MENSAJES
            sb.AppendLine("\nMensajes enviados:");
            foreach (var m in mensajes.Take(10)) // solo los últimos 10 por claridad
            {
                sb.AppendLine($"- [{m.FechaEnvio}] {m.Titulo}: {m.Contenido} (Usuario ID: {m.UsuarioId}, Stand: {(m.Stand?.Nombre ?? "N/A")})");
            }

            return sb.ToString();
        }

        private async Task<string> ConsultarOpenRouterConContexto(List<object> mensajes)
        {
            var apiKey = _config["OpenRouter:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return "No se encontró la clave de API de OpenRouter.";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
            client.DefaultRequestHeaders.Add("X-Title", "FerioBackend");

            var requestBody = new
            {
                model = "mistralai/mixtral-8x7b-instruct",
                messages = mensajes
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
                return $"Error al contactar con OpenRouter: {response.StatusCode}";

            var jsonString = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                var respuesta = root.GetProperty("choices")[0]
                                    .GetProperty("message")
                                    .GetProperty("content")
                                    .GetString();

                return respuesta ?? "Respuesta vacía.";
            }
            catch (Exception ex)
            {
                return $"Error al leer la respuesta de OpenRouter: {ex.Message}";
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
