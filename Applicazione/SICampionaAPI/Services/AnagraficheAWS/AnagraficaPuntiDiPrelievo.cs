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
    public class AnagraficaPuntiDiPrelievo(ILogger<Anagrafiche> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        public List<PuntoDiPrelievo> PuntiDiPrelievo(string codiceIstatComune, string denominazioneCodice = null)
        {
            logger.LogDebug("{Metodo} - Codice istat comune: {CodiceIstatComune} Parte della denominazione o del codice: {DenominazioneCodice}",
                                MethodBase.GetCurrentMethod().Name, codiceIstatComune, denominazioneCodice);

            var listaPuntiDiPrelievo = new List<PuntoDiPrelievo>();

            foreach (var puntoDiPrelievoAWS in LetturaDatiPuntoDiPrelievoDaAPIAWS(codiceIstatComune, denominazioneCodice))
            {
                var puntoDiPrelievo = new PuntoDiPrelievo
                {
                    Codice = Utils.TrimAndNull(puntoDiPrelievoAWS.Codice),
                    Denominazione = Utils.TrimAndNull(puntoDiPrelievoAWS.Denominazione),
                    Indirizzo = Utils.TrimAndNull(puntoDiPrelievoAWS.Indirizzo),
                    Comune = new Comune()
                    {
                        CodiceIstat = Utils.TrimAndNull(puntoDiPrelievoAWS.CodiceIstatComune),
                        Denominazione = Utils.TrimAndNull(puntoDiPrelievoAWS.DenominazioneComune)
                    },
                    Coordinate = new Coordinate()
                    {
                        Latitudine = Utils.TrimAndNull(puntoDiPrelievoAWS.Latitudine),
                        Longitudine = Utils.TrimAndNull(puntoDiPrelievoAWS.Longitudine)
                    }
                };
                listaPuntiDiPrelievo.Add(puntoDiPrelievo);
            }

            return listaPuntiDiPrelievo.OrderBy(c => c.Denominazione).ToList();
        }

        private List<RispostaAwsPuntiDiPrelievo.Item> LetturaDatiPuntoDiPrelievoDaAPIAWS(string codiceIstatComune, string param)
        {
            logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

            var endpoint = configuration["APIAnagraficheEndpointPuntiDiPrelievo"]
                .Replace("[param]", param)
                .Replace("[codice_istat_comune]", codiceIstatComune)
                .Replace("[apikey]", configuration["APIAnagraficheAPIKey"]);

            using var client = clientFactory.CreateClient("APIAnagrafiche");

            var response = client.GetAsync(endpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<RispostaAwsPuntiDiPrelievo>(responseContent);
                return [.. result.Items];
            }
            else
            {
                logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new HttpClientException($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Risposta API per la richiesta dei punti di prelievo
        /// </summary>
        private sealed class RispostaAwsPuntiDiPrelievo
        {
            [JsonPropertyName("items")]
            public Item[] Items { get; set; }
            public class Item
            {
                [JsonPropertyName("identity")]
                public string Codice { get; set; }

                [JsonPropertyName("description")]
                public string Denominazione { get; set; }

                [JsonPropertyName("indirizzo")]
                public string Indirizzo { get; set; }

                [JsonPropertyName("comune")]
                public string CodiceIstatComune { get; set; }
                [JsonPropertyName("nome_comune")]
                public string DenominazioneComune { get; set; }

                [JsonPropertyName("latitudine")]
                public string Latitudine { get; set; }

                [JsonPropertyName("longitudine")]
                public string Longitudine { get; set; }
            }
        }
    }
}

