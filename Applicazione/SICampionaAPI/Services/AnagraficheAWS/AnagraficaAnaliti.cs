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
    public class AnagraficaAnaliti(ILogger<Anagrafiche> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        public List<Analita> Analiti(string codiceMatrice, string codiceArgomento, string codicePacchetto, string parteDelNome, string linea)
        {
            logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice} Codice argomento: {CodiceArgomento} Codice pacchetto: {CodicePacchetto} Parte del nome: {ParteDelNome} Linea: {Linea}",
                MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento, codicePacchetto, parteDelNome, linea);

            var listaAnaliti = new List<Analita>();

            foreach (var analitaAWS in LetturaDatiAnalitiDaAWS(codiceMatrice, codiceArgomento, codicePacchetto, parteDelNome, linea))
            {
                var analita = new Analita
                {
                    Codice = Utils.TrimAndNull(analitaAWS.Codice),
                    Descrizione = Utils.TrimAndNull(analitaAWS.Descrizione),
                    CodiceMetodo = Utils.TrimAndNull(analitaAWS.CodiceMetodo),
                    DescrizioneMetodo = Utils.TrimAndNull(analitaAWS.DescrizioneMetodo),
                    CodicePacchetto = Utils.TrimAndNull(analitaAWS.CodicePacchetto),
                    DescrizionePacchetto = Utils.TrimAndNull(analitaAWS.DescrizionePacchetto),
                    ValoreLimite = Utils.TrimAndNull(analitaAWS.ValoreLimite),
                    UnitaMisura = Utils.TrimAndNull(analitaAWS.UnitaMisura),
                    Ordinamento = analitaAWS.Ordine
                    // I contenitori non sono caricati perché l'attività è troppo onerosa 
                    // nel caso di una semplice ricerca per nome analita che restituiscce migliaia di risultati
                };
                listaAnaliti.Add(analita);
            }
            return listaAnaliti;
        }

        private List<RispostaAwsAnaliti.Item> LetturaDatiAnalitiDaAWS(string codiceMatrice, string codiceArgomento, string codicePacchetto, string parteDelNome, string linea)
        {
            logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice} Codice argomento: {CodiceArgomento} Codice pacchetto: {CodicePacchetto} Parte del nome: {ParteDelNome} Linea: {Linea}",
                MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento, codicePacchetto, parteDelNome, linea);

            var endpoint = configuration["APIAnagraficheEndpointAnaliti"]
                .Replace("[codice_matrice]", codiceMatrice)
                .Replace("[codice_argomento]", codiceArgomento)
                .Replace("[codice_pacchetto]", codicePacchetto)
                .Replace("[parte_nome_analita]", parteDelNome)
                .Replace("[linea]", linea)
                .Replace("[apikey]", configuration["APIAnagraficheAPIKey"]);

            using var client = clientFactory.CreateClient("APIAnagrafiche");

            var response = client.GetAsync(endpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<RispostaAwsAnaliti>(responseContent);
                return [.. result.Items];
            }
            else
            {
                logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new HttpClientException($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Risposta API per la richieata di analiti
        /// </summary>
        private sealed class RispostaAwsAnaliti
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

                [JsonPropertyName("pack_identity")]
                public string CodicePacchetto { get; set; }

                [JsonPropertyName("pack_name")]
                public string DescrizionePacchetto { get; set; }

                [JsonPropertyName("valore_limite")]
                public string ValoreLimite { get; set; }

                [JsonPropertyName("param_units")]
                public string UnitaMisura { get; set; }

                [JsonPropertyName("order_num")]
                public int Ordine { get; set; }
            }
        }
    }
}
