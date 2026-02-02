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
    public class AnagraficaMisureInLoco(ILogger<Anagrafiche> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        public List<MisuraInLoco> MisureInLoco(string codiceMatrice, string codiceArgomento, List<string> codiciPacchetto)
        {
            logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice} Codice argomento: {CodiceArgomento}",
                MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento);

            var listaMisureInLoco = new List<MisuraInLoco>();
            foreach (var codicePacchetto in codiciPacchetto)
            {
                logger.LogDebug("{Metodo} - Codice pacchetto: {CodicePacchetto}",
                    MethodBase.GetCurrentMethod().Name, codicePacchetto);
                foreach (var misuraInLocoAWS in LetturaDatiMisureInLocoDaAWS(codiceMatrice, codiceArgomento, codicePacchetto))
                {
                    var misuraInLoco = new MisuraInLoco
                    {
                        Codice = Utils.TrimAndNull(misuraInLocoAWS.Codice),
                        Descrizione = Utils.TrimAndNull(misuraInLocoAWS.Descrizione),
                        CodiceMetodo = Utils.TrimAndNull(misuraInLocoAWS.CodiceMetodo),
                        DescrizioneMetodo = Utils.TrimAndNull(misuraInLocoAWS.DescrizioneMetodo),
                        UnitaMisura = Utils.TrimAndNull(misuraInLocoAWS.UnitaMisura)
                    };
                    listaMisureInLoco.Add(misuraInLoco);
                }
            }
            return listaMisureInLoco;
        }

        private List<RispostaAwsMisureInLoco.Item> LetturaDatiMisureInLocoDaAWS(string codiceMatrice, string codiceArgomento, string codicePacchetto)
        {
            logger.LogDebug("{Metodo} - Codice pacchetto: {CodicePacchetto}",
                MethodBase.GetCurrentMethod().Name, codicePacchetto);
            logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice} Codice argomento: {CodiceArgomento} Codice pacchetto: {CodicePacchetto}",
                MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento, codicePacchetto);

            var endpoint = configuration["APIAnagraficheEndpointAnaliti"]
                .Replace("[codice_pacchetto]", codicePacchetto)
                .Replace("[codice_matrice]", codiceMatrice)
                .Replace("[codice_argomento]", codiceArgomento)
                .Replace("[parte_nome_analita]", null)
                .Replace("[linea]", "territorio") // L'API degli analiti se chiamata col filtro linea=territorio restituisce solo le misure in loco
                .Replace("[apikey]", configuration["APIAnagraficheAPIKey"]);

            using var client = clientFactory.CreateClient("APIAnagrafiche");

            var response = client.GetAsync(endpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<RispostaAwsMisureInLoco>(responseContent);
                return [.. result.Items];
            }
            else
            {
                logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new HttpClientException($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Risposta API per la richieata di misure in loco
        /// </summary>
        private sealed class RispostaAwsMisureInLoco
        {
            [JsonPropertyName("items")]
            public Item[] Items { get; set; }
            public class Item
            {
                [JsonPropertyName("analita_identity")]
                public string Codice { get; set; }

                [JsonPropertyName("analita_name")]
                public string Descrizione { get; set; }

                [JsonPropertyName("metodo_identity")]
                public string CodiceMetodo { get; set; }

                [JsonPropertyName("metodo_name")]
                public string DescrizioneMetodo { get; set; }

                [JsonPropertyName("param_units")]
                public string UnitaMisura { get; set; }
            }
        }
    }
}
