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
using SICampionaAPI.Features.Analiti;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SICampionaAPI.Services.AnagraficheAWS
{
    public class AnagraficaMatrici(ILogger<Anagrafiche> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        public List<MatriceNomeDescrizione> Matrici()
        {
            logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

            var elencoMatrici = new List<MatriceNomeDescrizione>();

            var datiMatrici = LetturaDatiMatriciDaAPIAWS();

            if (datiMatrici != null)
            {
                elencoMatrici = datiMatrici.Select(datoMatrice => new MatriceNomeDescrizione
                {
                    Codice = Utils.TrimAndNull(datoMatrice.Codice),
                    Nome = Utils.TrimAndNull(datoMatrice.Nome),
                    Descrizione = Utils.TrimAndNull(datoMatrice.Descrizione)
                }).ToList();
            }
            else
            {
                logger.LogWarning("Nessuna matrice trovata");
            }

            return elencoMatrici;
        }

        private List<RispostaAwsMatrici.Item> LetturaDatiMatriciDaAPIAWS()
        {
            logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

            var endpoint = configuration["APIAnagraficheEndpointMatrici"]
                .Replace("[apikey]", configuration["APIAnagraficheAPIKey"]);

            using var client = clientFactory.CreateClient("APIAnagrafiche");

            var response = client.GetAsync(endpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<RispostaAwsMatrici>(responseContent);
                return [.. result.Items];
            }
            else
            {
                logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new HttpClientException($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Risposta API per la richiesta di matrici
        /// </summary>
        private sealed class RispostaAwsMatrici
        {
            [JsonPropertyName("items")]
            public Item[] Items { get; set; }
            public class Item
            {
                [JsonPropertyName("identity")]
                public string Codice { get; set; }

                [JsonPropertyName("name")]
                public string Nome { get; set; }

                [JsonPropertyName("description")]
                public string Descrizione { get; set; }
            }
        }
    }
}
