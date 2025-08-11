using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BackendTorneosS.Servicios
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public OpenAIService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _config = config;
        }

        public async Task<string> EnviarMensajeAsync(string mensajeUsuario)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "Eres un asistente experto en hábitos financieros personales. Solo responde preguntas relacionadas con finanzas personales, ahorro, inversión, presupuesto y gastos. Si te preguntan algo fuera de ese tema, responde que no estás autorizado a hablar de eso." },
                    new { role = "user", content = mensajeUsuario }
                }
            };

            var contenido = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var respuesta = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", contenido);
            if (!respuesta.IsSuccessStatusCode)
            {
                var errorDetalle = await respuesta.Content.ReadAsStringAsync();
                return $"Error de OpenAI: {respuesta.StatusCode} - {errorDetalle}";
            }


            var resultado = await respuesta.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(resultado);
            var respuestaIA = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return respuestaIA?.Trim() ?? "Sin respuesta";
        }
    }
}
