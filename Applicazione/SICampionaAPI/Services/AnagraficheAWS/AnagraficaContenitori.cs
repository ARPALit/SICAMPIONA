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
    public class AnagraficaContenitori(ILogger<Anagrafiche> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        public List<Contenitore> Contenitori(string codicePacchetto)
        {
            logger.LogDebug("{Metodo} - Codice pacchetto: {CodicePacchetto}", MethodBase.GetCurrentMethod().Name, codicePacchetto);

            var elencoContenitori = new List<Contenitore>();

            var datiContenitori = LetturaContenitoriDaAWS(codicePacchetto);

            if (datiContenitori != null)
            {
                elencoContenitori = datiContenitori.Select(datoContenitore => new Contenitore
                {
                    Tipo = Utils.TrimAndNull(datoContenitore.Tipo),
                    Quantita = datoContenitore.Quantita
                }).ToList();
            }
            else
            {
                logger.LogWarning("Nessun contenitore trovato per il codice pacchetto: {CodicePacchetto}", codicePacchetto);
            }

            return elencoContenitori;
        }

        private List<Contenitore> LetturaContenitoriDaAWS(string codicePacchetto)
        {
            logger.LogDebug("{Metodo} - Codice pacchetto: {CodicePacchetto}", MethodBase.GetCurrentMethod().Name, codicePacchetto);

            var contenitori = new List<Contenitore>();

            var endpointAnaliti = configuration["APIAnagraficheEndpointContenitori"]
                .Replace("[codice_pacchetto]", codicePacchetto)
                .Replace("[apikey]", configuration["APIAnagraficheAPIKey"]);

            using var client = clientFactory.CreateClient("APIAnagrafiche");

            var response = client.GetAsync(endpointAnaliti).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<RispostaAwsContenitori>(responseContent);
                foreach (var item in result.Items)
                {
                    var contenitore = new Contenitore
                    {
                        Tipo = item.Tipo,
                        Quantita = item.Quantita
                    };
                    contenitori.Add(contenitore);
                }
                logger.LogDebug("{Metodo} - Codice pacchetto: {CodicePacchetto} Trovati {NumeroContenitori}",
                    MethodBase.GetCurrentMethod().Name, codicePacchetto, contenitori.Count);
                return contenitori;
            }
            else
            {
                logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new HttpClientException($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Risposta API per la richieata di contenitori
        /// </summary>
        private sealed class RispostaAwsContenitori
        {
            [JsonPropertyName("items")]
            public Item[] Items { get; set; }
            public class Item
            {
                [JsonPropertyName("bottle_type")]
                public string Tipo { get; set; }
                [JsonPropertyName("quantity")]
                public int Quantita { get; set; }
            }
        }
    }
}
