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
using SICampionaAPI.Features.Clienti;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SICampionaAPI.Services.AnagraficheAWS
{
    public class AnagraficaClienti(ILogger<Anagrafiche> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        public List<ClienteConIndirizzo> Clienti(string parteDenominazioneCodice)
        {
            logger.LogDebug("{Metodo} - Parte della Denominazione o del Codice: {ParteDenominazioneCodice}", MethodBase.GetCurrentMethod().Name, parteDenominazioneCodice);

            var elencoClienti = new List<ClienteConIndirizzo>();

            var datiClienti = LetturaDatiClientiDaAPIAWS(parteDenominazioneCodice);

            if (datiClienti != null)
            {
                foreach (var datoCliente in datiClienti)
                {

                    var cliente = new ClienteConIndirizzo
                    {
                        Codice = Utils.TrimAndNull(datoCliente.Codice),
                        Denominazione = Utils.TrimAndNull(datoCliente.Denominazione),
                        CodiceFiscale = Utils.TrimAndNull(datoCliente.CodiceFiscale),
                        PartitaIVA = Utils.TrimAndNull(datoCliente.PartitaIVA),
                        IndirizzoCompleto = IndirizzoCompleto(datoCliente)
                    };

                    elencoClienti.Add(cliente);
                }
            }
            else
            {
                logger.LogWarning("Nessun cliente trovato per parte della Denominazione o del Codice: {ParteDenominazioneCodice}", parteDenominazioneCodice);
            }

            return elencoClienti.OrderBy(c => c.Denominazione).ToList();
        }

        /// <summary>
        /// Ricerca anche parziale per i valori identity o description dei risultati AWS
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <exception cref="HttpClientException"></exception>
        private List<RispostaAwsClienti.Item> LetturaDatiClientiDaAPIAWS(string param)
        {
            logger.LogDebug("{Metodo} - Param: {Param}", MethodBase.GetCurrentMethod().Name, param);

            var endpoint = configuration["APIAnagraficheEndpointClienti"]
                .Replace("[param]", param)
                .Replace("[apikey]", configuration["APIAnagraficheAPIKey"]);

            using var client = clientFactory.CreateClient("APIAnagrafiche");

            var response = client.GetAsync(endpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<RispostaAwsClienti>(responseContent);
                return [.. result.Items];
            }
            else
            {
                logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new HttpClientException($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        private string IndirizzoCompleto(RispostaAwsClienti.Item datoCliente)
        {
            logger.LogTrace("{Metodo}", MethodBase.GetCurrentMethod().Name);

            var indirizzoCompleto = string.Empty;

            if (!string.IsNullOrWhiteSpace(datoCliente.Via))
                indirizzoCompleto += datoCliente.Via;

            if (!string.IsNullOrWhiteSpace(datoCliente.Civico))
                indirizzoCompleto += " " + datoCliente.Civico;

            if (!string.IsNullOrWhiteSpace(datoCliente.Comune))
                indirizzoCompleto += " - " + datoCliente.Comune;

            if (!string.IsNullOrWhiteSpace(datoCliente.Provincia))
                indirizzoCompleto += " (" + datoCliente.Provincia + ")";

            if (indirizzoCompleto.Length > 0)
                return indirizzoCompleto.Trim();
            else return null;
        }

        /// <summary>
        /// Risposta API per la richiesta di clienti
        /// </summary>
        private sealed class RispostaAwsClienti
        {
            [JsonPropertyName("items")]
            public Item[] Items { get; set; }
            public class Item
            {
                [JsonPropertyName("identity")]
                public string Codice { get; set; }

                [JsonPropertyName("company_name")]
                public string Denominazione { get; set; }

                [JsonPropertyName("codice_fiscale")]
                public string CodiceFiscale { get; set; }

                [JsonPropertyName("p_iva")]
                public string PartitaIVA { get; set; }

                [JsonPropertyName("address1")]
                public string Via { get; set; }

                [JsonPropertyName("address2")]
                public string Civico { get; set; }

                [JsonPropertyName("address3")]
                public string Comune { get; set; }

                [JsonPropertyName("address4")]
                public string Provincia { get; set; }
            }
        }
    }
}
