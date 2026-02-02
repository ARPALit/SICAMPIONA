/*
 * Nome del progetto: SICampiona
 * Copyright (C) 2025 Agenzia regionale per la protezione dell'ambiente ligure
 *
 * Questo programma è software libero: puoi ridistribuirlo e/o modificarlo
 * secondo i termini della GNU Affero General Public License pubblicata dalla
 * Free Software Foundation, sia la versione 3 della licenza, sia (a tua scelta)
 * qualsiasi versione successiva.
 *
 * Questo programma è distribuito nella speranza che possa essere utile,
 * ma SENZA ALCUNA GARANZIA; senza nemmeno la garanzia implicita di
 * COMMERCIABILITÀ o IDONEITÀ PER UNO SCOPO PARTICOLARE. Vedi la
 * GNU Affero General Public License per ulteriori dettagli.
 *
 * Dovresti aver ricevuto una copia della GNU Affero General Public License
 * insieme a questo programma. In caso contrario, vedi <https://www.gnu.org/licenses/>.
*/

using SICampionaAPI.Common;
using SICampionaAPI.Features.Operatore;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SICampionaAPI
{
    public class AutenticazioneMiddleware(RequestDelegate next, ILogger<OperatoriController> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // Lettura valore della route APIKey
            var apiKey = context.Request.RouteValues["APIKey"];
            // Se è presente l'APIKey non è verificato il token si autenticazione né letto il codice fiscale
            if (apiKey != null)
            {
                logger.LogDebug("APIKey presente: {APIKey} - Verifica token autenticazione non necessaria", apiKey);
                await next(context);
                return;
            }

            // Se è la richiesta di un file PDF ignorare l'autenticazione
            if (context.Request.Path.Value.EndsWith(".pdf"))
            {
                logger.LogDebug("Richiesta file PDF - Verifica token autenticazione non necessaria");
                await next(context);
                return;
            }

            // Se è la richiesta è la radice del sito ignorare l'autenticazione
            if (context.Request.Path.Value == "/")
            {
                logger.LogDebug("Richiesta radice - Verifica token autenticazione non necessaria");
                await next(context);
                return;
            }

            string token = LetturaTokenDaAuthorizationHeader(context.Request.Headers);
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ApiResponse<ConfigurazioneOperatore>()
                {
                    Success = false,
                    ErrorMessage = "Token non presente nell'header Authorization"
                });
                return;
            }

            UserInfo userInfo = await GetUserInfoAsync(token);
            // Se non sono disponibili le informazioni utente l'autenticazione fallisce
            if (userInfo == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ApiResponse<ConfigurazioneOperatore>()
                {
                    Success = false,
                    ErrorMessage = "Autenticazione fallita"
                });
                return;
            }

            // Altrimenti registra nel log le informazioni utente
            logger.LogInformation("Operatore autenticato: {Name} - {FiscalNumber} - {Email} - {FamilyName}",
                userInfo.Name, userInfo.FiscalNumber, userInfo.Email, userInfo.FamilyName);

            // e inserisce il codice fiscale negli items della request estraendolo da UserInfo.FiscalNumber
            // senza la parte iniziale "TINIT-"
            context.Items["CodiceFiscale"] = userInfo.FiscalNumber.Substring(6);

            await next(context);
        }

        private string LetturaTokenDaAuthorizationHeader(IHeaderDictionary headers)
        {
            logger.LogDebug("Lettura token dall'header Authorization");
            logger.LogDebug("Header Authorization: {Authorization}", headers.Authorization.ToString());

            // Lettura del token bearer dall'header Authorization rimuovendo la stringa "Bearer "
            string token = headers.Authorization.ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return null;
            return token;
        }

        private sealed class UserInfo
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("fiscalNumber")]
            public string FiscalNumber { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("familyName")]
            public string FamilyName { get; set; }
        }

        /// <summary>
        /// Lettura informazioni utente da IdentityServer
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<UserInfo> GetUserInfoAsync(string token)
        {
            try
            {
                var client = clientFactory.CreateClient("IdentityServer");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(configuration["URLUserInfoIdentityServer"]);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<UserInfo>(responseContent);
                    return userInfo;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // In caso di fallimento registra la risposta e ritorna null
                    logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase} - {Content}",
                        response.StatusCode, response.ReasonPhrase, responseContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Registra l'eccezione e ritorna null
                logger.LogError(ex, "Eccezione durante la chiamata al client http");
                return null;
            }
        }
    }
    public static class AutenticazioneMiddlewareExtensions
    {
        public static IApplicationBuilder AutenticazioneMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AutenticazioneMiddleware>();
        }
    }
}
