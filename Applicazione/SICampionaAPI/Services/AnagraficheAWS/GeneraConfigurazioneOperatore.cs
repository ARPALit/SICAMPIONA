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
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SICampionaAPI.Services.AnagraficheAWS
{
    public class GeneraConfigurazioneOperatore(ILogger<Anagrafiche> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        public ConfigurazioneOperatore Genera(string codiceFiscale)
        {
            logger.LogDebug("{Metodo} - CodiceFiscale: {CodiceFiscale}", MethodBase.GetCurrentMethod().Name, codiceFiscale);

            var datiOperatore = LetturaDatiOperatoreDaAPIAWS(codiceFiscale);

            var configurazioneOperatore = new ConfigurazioneOperatore
            {
                Enti = EstrazioneEnti(datiOperatore)
            };

            // Cognome, nome e codice fiscale (sono uguali per tutti gli item)
            if (datiOperatore.Count != 0)
            {
                var firstItem = datiOperatore[0];
                configurazioneOperatore.CodiceFiscale = firstItem.CodiceFiscale;
                configurazioneOperatore.Cognome = firstItem.Cognome;
                configurazioneOperatore.Nome = firstItem.Nome;
                configurazioneOperatore.CodiceAnagrafica = firstItem.CodiceAnagrafica;

                // Scrittura configurazioneOperatore nel log come Json
                logger.LogDebug("Configurazione operatore: {ConfigurazioneOperatore}", JsonSerializer.Serialize(configurazioneOperatore));
            }
            else
                logger.LogWarning("Nessun dato operatore trovato per il codice fiscale {CodiceFiscale}", codiceFiscale);

            return configurazioneOperatore;
        }

        /// <summary>
        /// Nella risposta dell'API AWS sono presenti più item per ogni ente, matrice e argomento
        /// </summary>
        /// <param name="codiceFiscaleOperatore"></param>
        /// <returns></returns>
        private List<RispostaAwsOperatore.Item> LetturaDatiOperatoreDaAPIAWS(string codiceFiscaleOperatore)
        {
            logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

            var endpoint = configuration["APIAnagraficheEndpointOperatore"]
                .Replace("[CF]", codiceFiscaleOperatore)
                .Replace("[apikey]", configuration["APIAnagraficheAPIKey"]);

            using var client = clientFactory.CreateClient("APIAnagrafiche");

            var response = client.GetAsync(endpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<RispostaAwsOperatore>(responseContent);
                return [.. result.Items];
            }
            else
            {
                logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new HttpClientException($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        private List<ConfigurazioneOperatore.Ente> EstrazioneEnti(List<RispostaAwsOperatore.Item> listaItems)
        {
            logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

            var listaEnti = new List<ConfigurazioneOperatore.Ente>();

            foreach (var item in listaItems)
            {
                // Se non è presente il codice ente ignora l'item
                if (string.IsNullOrWhiteSpace(item.CodiceEnte))
                    continue;

                // Trova o crea l'Ente
                var ente = listaEnti.FirstOrDefault(e => e.Codice == item.CodiceEnte);
                if (ente == null)
                {
                    ente = new ConfigurazioneOperatore.Ente
                    {
                        Codice = item.CodiceEnte,
                        RagioneSociale = item.DescrizioneEnte,
                        CodiceCliente = item.CodiceCliente,
                        Matrici = []
                    };
                    listaEnti.Add(ente);
                }

                // Se non è presente il codice matrice non considera la matrice e l'argomento
                if (string.IsNullOrWhiteSpace(item.CodiceMatrice))
                    continue;

                // Trova o crea la Matrice
                var matrice = ente.Matrici.FirstOrDefault(m => m.Codice == item.CodiceMatrice);
                if (matrice == null)
                {
                    matrice = new ConfigurazioneOperatore.Ente.Matrice
                    {
                        Codice = item.CodiceMatrice,
                        Descrizione = item.DescrizioneMatrice,
                        Argomenti = []
                    };
                    ente.Matrici.Add(matrice);
                }

                // Se non è presente il codice argomento non considera l'argomento
                if (string.IsNullOrWhiteSpace(item.CodiceArgomento))
                    continue;

                // Aggiungi l'Argomento se non esiste
                var argomento = matrice.Argomenti.FirstOrDefault(a => a.Codice == item.CodiceArgomento);
                if (argomento == null)
                {
                    matrice.Argomenti.Add(new ConfigurazioneOperatore.Ente.Matrice.Argomento
                    {
                        Codice = item.CodiceArgomento,
                        Descrizione = item.DescrizioneArgomento
                    });
                }
            }

            logger.LogDebug("Enti trovati: {NumeroEnti}", listaEnti.Count);

            return listaEnti;
        }

        /// <summary>
        /// Risposta API per la configurazione dell'operatore
        /// </summary>
        private sealed class RispostaAwsOperatore
        {
            [JsonPropertyName("items")]
            public Item[] Items { get; set; }
            public class Item
            {
                [JsonPropertyName("cognome")]
                public string Cognome { get; set; }

                [JsonPropertyName("nome")]
                public string Nome { get; set; }

                [JsonPropertyName("cf")]
                public string CodiceFiscale { get; set; }

                [JsonPropertyName("cod_ana")]
                public string CodiceAnagrafica { get; set; }

                [JsonPropertyName("cod_argomento")]
                public string CodiceArgomento { get; set; }

                [JsonPropertyName("argomento")]
                public string DescrizioneArgomento { get; set; }

                [JsonPropertyName("matrice_identity")]
                public string CodiceMatrice { get; set; }

                [JsonPropertyName("matrice_name")]
                public string DescrizioneMatrice { get; set; }

                [JsonPropertyName("cod_ana_ente")]
                public string CodiceEnte { get; set; }

                [JsonPropertyName("rag_soc_ente")]
                public string DescrizioneEnte { get; set; }

                [JsonPropertyName("id_alims_clienti")]
                public string CodiceCliente { get; set; }
            }
        }
    }
}
