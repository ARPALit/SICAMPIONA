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
using SICampionaAPI.Features.Campionamenti;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SICampionaAPI.Services.AnagraficheAWS
{
    public class AnagraficaPrelevatori(ILogger<Anagrafiche> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        public List<Prelevatore> Prelevatori(string codiceMatrice, string codiceArgomento)
        {
            logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice} Codice argomento: {CodiceArgomento}",
                MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento);

            var prelevatori = new List<Prelevatore>();

            foreach (var prelevatoreAWS in LetturaDatiPrelevatoriDaAPIAWS(codiceMatrice, codiceArgomento))
            {
                // Nella risposta dell'API AWS sono presenti più item per ogni ente e argomento
                // ma con lo stesso codice prelevatore. Per evitare duplicati nella lista
                // di prelevatori, viene verificato che non sia già presente un prelevatore
                // con lo stesso codice
                if (prelevatori.Exists(p => p.Codice == prelevatoreAWS.Codice))
                    continue;
                var prelevatore = new Prelevatore
                {
                    Codice = Utils.TrimAndNull(prelevatoreAWS.Codice),
                    Nome = Utils.TrimAndNull(prelevatoreAWS.Nome),
                    Cognome = Utils.TrimAndNull(prelevatoreAWS.Cognome),
                };
                prelevatori.Add(prelevatore);
            }

            return prelevatori;
        }

        private List<RispostaAwsPrelevatori.Item> LetturaDatiPrelevatoriDaAPIAWS(string codiceEnte, string codiceArgomento)
        {
            logger.LogDebug("{Metodo} - Codice ente: {CodiceMatrice} Codice argomento: {CodiceArgomento}",
                MethodBase.GetCurrentMethod().Name, codiceEnte, codiceArgomento);

            var endpoint = configuration["APIAnagraficheEndpointPrelevatori"]
                .Replace("[codice_ente]", codiceEnte).Replace("[codice_argomento]", codiceArgomento)
                .Replace("[apikey]", configuration["APIAnagraficheAPIKey"]);

            using var client = clientFactory.CreateClient("APIAnagrafiche");

            var response = client.GetAsync(endpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<RispostaAwsPrelevatori>(responseContent);
                return [.. result.Items];
            }
            else
            {
                logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new HttpClientException($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Risposta API per la richiesta di prelevatori
        /// </summary>
        private sealed class RispostaAwsPrelevatori
        {
            [JsonPropertyName("items")]
            public Item[] Items { get; set; }
            public class Item
            {
                [JsonPropertyName("cod_ana_ente")]
                public string CodiceEnte { get; set; }
                [JsonPropertyName("cod_argomento")]
                public string CodiceArgomento { get; set; }
                [JsonPropertyName("cod_ana")]
                public string Codice { get; set; }
                [JsonPropertyName("nome")]
                public string Nome { get; set; }
                [JsonPropertyName("cognome")]
                public string Cognome { get; set; }
            }
        }
    }
}
