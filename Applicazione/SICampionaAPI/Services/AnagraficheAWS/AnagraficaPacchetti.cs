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
    public class AnagraficaPacchetti(ILogger<Anagrafiche> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        public List<Pacchetto> Pacchetti(string codiceMatrice, string codiceArgomento, string codiceSedeAccettazione)
        {
            logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice} Codice argomento: {CodiceArgomento} Codice sede accettazione: {CodiceSedeAccettazione}",
                MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento, codiceSedeAccettazione);

            var elencoPacchetti = new List<Pacchetto>();

            var datiPacchetti = LetturaDatiPacchettiDaAPIAWS(codiceMatrice, codiceArgomento, codiceSedeAccettazione);

            if (datiPacchetti != null)
            {
                elencoPacchetti = datiPacchetti.Select(datoPacchetto => new Pacchetto
                {
                    Codice = Utils.TrimAndNull(datoPacchetto.Codice),
                    Descrizione = Utils.TrimAndNull(datoPacchetto.Descrizione),
                    Sede = Utils.TrimAndNull(datoPacchetto.Sede)
                }).ToList();
            }
            else
            {
                logger.LogWarning("Nessun pacchetto trovato per il codice matrice: {CodiceMatrice}", codiceMatrice);
            }

            return elencoPacchetti;
        }

        private List<RispostaAwsPacchetti.Item> LetturaDatiPacchettiDaAPIAWS(string codiceMatrice, string codiceArgomento, string codiceSedeAccettazione)
        {
            logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice} Codice argomento: {CodiceArgomento} Codice sede accettazione: {CodiceSedeAccettazione}",
                MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento, codiceSedeAccettazione);

            var endpoint = configuration["APIAnagraficheEndpointPacchetti"]
                .Replace("[codice_matrice]", codiceMatrice)
                .Replace("[codice_argomento]", codiceArgomento)
                .Replace("[sede_accettazione]", codiceSedeAccettazione)
                .Replace("[apikey]", configuration["APIAnagraficheAPIKey"]);

            using var client = clientFactory.CreateClient("APIAnagrafiche");

            var response = client.GetAsync(endpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<RispostaAwsPacchetti>(responseContent);
                return [.. result.Items];
            }
            else
            {
                logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new HttpClientException($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Risposta API per la richiesta di pacchetti
        /// </summary>
        private sealed class RispostaAwsPacchetti
        {
            [JsonPropertyName("items")]
            public Item[] Items { get; set; }
            public class Item
            {
                [JsonPropertyName("pack_identity")]
                public string Codice { get; set; }

                [JsonPropertyName("pack_name")]
                public string Descrizione { get; set; }

                [JsonPropertyName("sede")]
                public string Sede { get; set; }

            }
        }
    }
}
