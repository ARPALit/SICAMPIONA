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

using Blazored.LocalStorage;
using SICampiona.Common;
using SICampiona.Model;
using System.Reflection;
using System.Text.Json;

namespace SICampiona.Services
{
    public class ConfigurazioneOperatoreService(IHttpClientFactory clientFactory, ILogger<Anagrafiche> logger, ILocalStorageService localStorage) : IConfigurazioneOperatoreService
    {
        private ConfigurazioneOperatore _configurazione;

        public async Task<Result> ScaricaConfigurazioneDaServer()
        {
            logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);
            try
            {
                using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

                var response = await client.GetAsync("operatori/configurazione");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ConfigurazioneOperatore>>(responseContent);
                    if (apiResponse.Success)
                    {
                        var configurazione = apiResponse.Data;
                        // Salva la configurazione in localstorage
                        await localStorage.SetItemAsync("ConfigurazioneOperatore", configurazione);
                        _configurazione = configurazione;
                        return ResultFactory.Success();
                    }
                    else
                    {
                        logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
                        return ResultFactory.Failure($"Errore chiamata API - Errore: {apiResponse.ErrorMessage} - Dettagli: {apiResponse.ErrorDetails}");
                    }
                }
                else
                {
                    logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                    return ResultFactory.Failure($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, ex.Message);
                return ResultFactory.Failure($"CaricaConfigurazione - Errore connessione per il prelievo dei dati operatore ({ex.Message})");
            }
            catch (Exception ex)
            {
                string dettagliEccezione = Utils.GetExceptionDetails(ex);
                logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
                return ResultFactory.Failure($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
            }
        }

        public string CodiceFiscale()
        {
            return _configurazione.CodiceFiscale;
        }

        public string NomeCognome()
        {
            return $"{_configurazione.Nome} {_configurazione.Cognome}";
        }

        public List<ConfigurazioneOperatore.Ente> Enti()
        {
            return _configurazione.Enti
                .OrderBy(i => i.RagioneSociale)
                .ToList();
        }

        public List<ConfigurazioneOperatore.Ente.Matrice> Matrici(string codiceEnte)
        {
            if (_configurazione.Enti.Exists(i => i.Codice == codiceEnte))
            {
                var ente = _configurazione.Enti.First(i => i.Codice == codiceEnte);
                return ente.Matrici.OrderBy(i => i.Descrizione).ToList();
            }
            else
                return [];
        }

        public List<ConfigurazioneOperatore.Ente.Matrice.Argomento> Argomenti(string codiceEnte, string codiceMatrice)
        {
            if (_configurazione.Enti.Exists(i => i.Codice == codiceEnte))
            {
                var ente = _configurazione.Enti.First(i => i.Codice == codiceEnte);
                if (ente.Matrici.Exists(i => i.Codice == codiceMatrice))
                {
                    var matrice = ente.Matrici.First(i => i.Codice == codiceMatrice);
                    return [.. matrice.Argomenti.OrderBy(i => i.Descrizione)];
                }
                else
                    return [];
            }
            else
                return [];
        }

        public async Task CaricaConfigurazioneDaLocalStorage()
        {
            _configurazione = await localStorage.GetItemAsync<ConfigurazioneOperatore>("ConfigurazioneOperatore");
        }

        public bool ConfigurazioneDisponibile
        {
            get => _configurazione != null;
        }

        public void CancellaConfigurazione()
        {
            _configurazione = null;
        }

        public string AccessToken { get; set; }

        public bool Autorizzato { get; set; } = false;

        public bool ConfigurazioneSenzaEnti
        {
            get => _configurazione == null || _configurazione.Enti.Count == 0;
        }

        public bool VersioneApplicazioneVerificata { get; set; } = false;
    }
}
