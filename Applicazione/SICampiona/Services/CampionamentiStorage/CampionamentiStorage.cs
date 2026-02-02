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
using System.Text;
using System.Text.Json;

namespace SICampiona.Services
{
	public class CampionamentiStorage(IHttpClientFactory clientFactory, ILocalStorageService localStorage, ILogger<CampionamentiStorage> logger, IConfigurazioneOperatoreService infoOperatore) : ICampionamentiStorage
	{
        const string CampionamentiLocalStorage = "CampionamentiLocalStorage";

		private List<Campionamento> _campionamenti;

        /// <summary>
        /// Aggiorna i dati di un campionamento
        /// </summary>
        /// <param name="campionamento"></param>
        /// <returns></returns>
        public async Task<Result<Campionamento>> Aggiornamento(Campionamento campionamento)
		{
			try
			{
				await CaricaCampionamentiDaLocalStorage();

				if (!_campionamenti.Exists(i => i.IdCampionamento == campionamento.IdCampionamento))
					return ResultFactory.Failure<Campionamento>("Campionamento non trovato", $"IdCampionamento: {campionamento.IdCampionamento}");

				var campionamentoDaAggiornare = _campionamenti.First(i => i.IdCampionamento == campionamento.IdCampionamento);

				// Se i campionamenti sono uguali non è necessario aggiornare
				if (CampionamentiUguali(campionamentoDaAggiornare, campionamento))
					return ResultFactory.Success(campionamentoDaAggiornare);

				campionamento.SeModificato = true;
				campionamento.UltimoAggiornamento.DataOra = DateTime.Now;
				AssegnaSiglaVerbale(campionamento);
				_campionamenti.Remove(campionamentoDaAggiornare);
				_campionamenti.Add(campionamento);
				await SalvaCampionamentiInLocalStorage();
				return ResultFactory.Success(campionamentoDaAggiornare);
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<Campionamento>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		/// <summary>
		/// Confronta due campionamenti
		/// </summary>
		/// <param name="campionamento1"></param>
		/// <param name="campionamento2"></param>
		/// <returns></returns>
		private static bool CampionamentiUguali(Campionamento campionamento1, Campionamento campionamento2)
		{
			if (campionamento1 == null || campionamento2 == null)
				return false;
			var campionamento1Json = JsonSerializer.Serialize(campionamento1);
			var campionamento2Json = JsonSerializer.Serialize(campionamento2);
			return campionamento1Json == campionamento2Json;
		}

		private static void AssegnaSiglaVerbale(Campionamento campionamento)
		{
			if (campionamento.DataOraChiusuraCampionamento == null)
				return;

			// E' utilizzato il codice del punto di campionamento valorizzato
			string codicePuntoDiPrelievo = null;
			if (campionamento.PuntoDiPrelievoARPAL != null)
				codicePuntoDiPrelievo = campionamento.PuntoDiPrelievoARPAL.Codice;
			else if (campionamento.PuntoDiPrelievoInseritoDaOperatore != null)
				codicePuntoDiPrelievo = campionamento.PuntoDiPrelievoInseritoDaOperatore.Codice;

			if (string.IsNullOrEmpty(codicePuntoDiPrelievo))
				return;

			var siglaVerbale = $"SC_{campionamento.DataPrelievo:yyyyMMdd}_{codicePuntoDiPrelievo}_ID{campionamento.IdCampionamento}";
			campionamento.SiglaVerbale = siglaVerbale;
		}

		/// <summary>
		/// Aggiunge un nuovo campionamento
		/// </summary>
		/// <param name="nuovoCampionamento"></param>
		/// <returns></returns>
		public async Task<Result<Campionamento>> Aggiunta(NuovoCampionamento nuovoCampionamento)
		{
			logger.LogDebug("{Metodo} - Dati nuovo campionamento: {DatiNuovoCampionamento}", MethodBase.GetCurrentMethod().Name, JsonSerializer.Serialize(nuovoCampionamento));
			try
			{
				// Chiamata API per l'aggiunta del campionamento
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var jsonContent = JsonSerializer.Serialize(nuovoCampionamento);
				var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

				var response = await client.PostAsync("campionamenti/nuovo", httpContent);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<Campionamento>>(responseContent);
					if (apiResponse.Success)
					{
						var campionamentoCreato = apiResponse.Data;
						// Aggiunta del campionamento allo storage locale
						_campionamenti.Add(campionamentoCreato);
						await SalvaCampionamentiInLocalStorage();
						return ResultFactory.Success(campionamentoCreato);
					}
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<Campionamento>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<Campionamento>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<Campionamento>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		/// <summary>
		/// Carica i dettagli di un campionamento
		/// </summary>
		/// <param name="idCampionamento"></param>
		/// <returns></returns>
		public async Task<Result<Campionamento>> Dettagli(int idCampionamento)
		{
			try
			{
				await CaricaCampionamentiDaLocalStorage();

				if (_campionamenti.Exists(i => i.IdCampionamento == idCampionamento))
				{
					// Copia profonda del campionamento per evitare modifiche accidentali
					var campionamento = _campionamenti.First(i => i.IdCampionamento == idCampionamento);
					string json = JsonSerializer.Serialize(campionamento);
					Campionamento copiaCampionamento = JsonSerializer.Deserialize<Campionamento>(json);
					return ResultFactory.Success(copiaCampionamento);
				}
				else
					return ResultFactory.Failure<Campionamento>("Campionamento non trovato", $"IdCampionamento: {idCampionamento}");
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<Campionamento>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		/// <summary>
		/// Carica l'elenco dei campionamenti
		/// </summary>
		/// <returns></returns>
		public async Task<Result<List<InformazioniCampionamento>>> Elenco()
		{
			try
			{
				await CaricaCampionamentiDaLocalStorage();

				var informazioniCampionamenti = _campionamenti.Select(i => new InformazioniCampionamento()
				{
					IdCampionamento = i.IdCampionamento,
					Matrice = i.Matrice.Denominazione,
                    CodiceMatrice = i.Matrice.Codice,
                    Argomento = i.Argomento.Denominazione,
					Cliente = i.Cliente.Denominazione,
					PuntoDiPrelievo = DescrizionePuntoPrelievo(i),
					DataOraChiusuraCampionamento = i.DataOraChiusuraCampionamento,
					SeModificato = i.SeModificato,
					Eliminato = i.Eliminato,
					UrlPDFVerbale = i.UrlPDFVerbale,
					DataPrelievo = i.DataPrelievo
				}).OrderBy(i => i.Cliente)
				.ToList();

				return ResultFactory.Success(informazioniCampionamenti);
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<InformazioniCampionamento>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		/// <summary>
		/// Determina la descrizione del punto di campionamento
		/// </summary>
		/// <param name="campionamento"></param>
		/// <returns></returns>
		private static string DescrizionePuntoPrelievo(Campionamento campionamento)
		{
			// Usa la descrizione del punto di prelievo impostato e se nulla usa il codice
			PuntoDiPrelievo puntoDiPrelievo = campionamento.PuntoDiPrelievoARPAL;
			puntoDiPrelievo ??= campionamento.PuntoDiPrelievoInseritoDaOperatore;
			if (!string.IsNullOrEmpty(puntoDiPrelievo.Denominazione))
				return puntoDiPrelievo.Denominazione;
			else
				return puntoDiPrelievo.Codice;
		}

		/// <summary>
		/// Elimina un campionamento con cancellazione logica 
		/// </summary>
		/// <param name="idCampionamento"></param>
		/// <returns></returns>
		public async Task<Result> Eliminazione(int idCampionamento)
		{
			try
			{
				await CaricaCampionamentiDaLocalStorage();

				if (_campionamenti.Exists(i => i.IdCampionamento == idCampionamento))
				{
					var campionamento = _campionamenti.First(i => i.IdCampionamento == idCampionamento);
					campionamento.SeModificato = true;
					campionamento.Eliminato = true;
					campionamento.UltimoAggiornamento.DataOra = DateTime.Now;
					await SalvaCampionamentiInLocalStorage();
					return ResultFactory.Success();
				}
				else
				{
					return ResultFactory.Failure("Campionamento non trovato");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		/// <summary>
		/// Sincronizza i dati con il server
		/// </summary>
		/// <returns></returns>
		public async Task<Result> Sincronizzazione()
		{
			logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);
			try
			{
				await CaricaCampionamentiDaLocalStorage();

				// Selezione dei campionamenti modificati per l'invio al server
				var campionamentiDaInviare = _campionamenti.Where(i => i.SeModificato).ToList();

				// Invio dei campionamenti al server
				var resultInvio = await InviaCampionamentiAlServer(campionamentiDaInviare);
				if (!resultInvio.IsSuccess)
				{
					return ResultFactory.Failure(resultInvio.ErrorMessage, resultInvio.ErrorDetails);
				}

				// Lettura dei campionamenti aggiornati dal server
				var resultCaricamento = await CaricaCampionamentiDaServer();
				if (resultCaricamento.IsSuccess)
				{
					_campionamenti = resultCaricamento.Data;
					await SalvaCampionamentiInLocalStorage();
					return ResultFactory.Success();
				}
				else
				{
					return ResultFactory.Failure(resultInvio.ErrorMessage, resultInvio.ErrorDetails);
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		private async Task<Result<List<Campionamento>>> CaricaCampionamentiDaServer()
		{
			logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var response = await client.GetAsync("campionamenti/aperti");

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Campionamento>>>(responseContent);
					if (apiResponse.Success)
						return ResultFactory.Success(apiResponse.Data);
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<List<Campionamento>>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<List<Campionamento>>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (HttpRequestException ex)
			{
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, ex.Message);
				return ResultFactory.Failure<List<Campionamento>>($"Caricamento campionamenti dal server - Errore connessione per la ricezione dei dati ({ex.Message})");
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<Campionamento>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		private async Task SalvaCampionamentiInLocalStorage()
		{
			await localStorage.SetItemAsync(CampionamentiLocalStorage, _campionamenti);
		}

		private async Task CaricaCampionamentiDaLocalStorage()
		{
			// I dati sono caricati da localStorage solo se non presenti in memoria
			if (_campionamenti != null)
				return;
			_campionamenti = await localStorage.GetItemAsync<List<Campionamento>>(CampionamentiLocalStorage);
			_campionamenti ??= [];
		}

		public async Task<Result<bool>> SeModificati()
		{
			try
			{
				await CaricaCampionamentiDaLocalStorage();
				return ResultFactory.Success(_campionamenti.Exists(i => i.SeModificato));
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<bool>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		private async Task<Result<bool>> InviaCampionamentiAlServer(List<Campionamento> campionamenti)
		{
			logger.LogDebug("{Metodo} - Richiesta: {Richiesta}", MethodBase.GetCurrentMethod().Name, JsonSerializer.Serialize(campionamenti));
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var jsonContent = JsonSerializer.Serialize(campionamenti);
				var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

				var response = await client.PutAsync("campionamenti/aggiornamento", httpContent);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<InfoCampionamentoCompletato>>>(responseContent);
					if (!apiResponse.Success)
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<bool>($"Errore chiamata API - Errore: {apiResponse.ErrorMessage} - Dettagli: {apiResponse.ErrorDetails}");
					}
					else
					{
						return ResultFactory.Success(true);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<bool>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (HttpRequestException ex)
			{
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, ex.Message);
				return ResultFactory.Failure<bool>($"Invio campionamenti al server - Errore connessione per l'invio dei dati ({ex.Message})");
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<bool>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<Campionamento>> Duplicazione(RichiestaDuplicazioneCampionamento richiestaDuplicazioneCampionamento)
		{
			logger.LogDebug("{Metodo} - Dati duplicazione campionamento: {RichiestaDuplicazioneCampionamento}",
				MethodBase.GetCurrentMethod().Name, JsonSerializer.Serialize(richiestaDuplicazioneCampionamento));
			try
			{
				// Chiamata API per la duplicazione del campionamento
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var jsonContent = JsonSerializer.Serialize(richiestaDuplicazioneCampionamento);
				var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

				var response = await client.PostAsync("campionamenti/duplica", httpContent);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<Campionamento>>(responseContent);
					if (apiResponse.Success)
					{
						var campionamentoCreato = apiResponse.Data;
						// Aggiunta del campionamento allo storage locale
						_campionamenti.Add(campionamentoCreato);
						await SalvaCampionamentiInLocalStorage();
						return ResultFactory.Success(campionamentoCreato);
					}
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<Campionamento>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<Campionamento>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<Campionamento>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<Campionamento>> DuplicazioneCampionamentoCompletato(RichiestaDuplicazioneCampionamentoCompletato richiestaDuplicazioneCampionamentoCompletato)
		{
			logger.LogDebug("{Metodo} - Dati duplicazione campionamento completato: {RichiestaDuplicazioneCampionamentoCompletato}",
				MethodBase.GetCurrentMethod().Name, JsonSerializer.Serialize(richiestaDuplicazioneCampionamentoCompletato));
			try
			{
				// Chiamata API per la duplicazione del campionamento
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var jsonContent = JsonSerializer.Serialize(richiestaDuplicazioneCampionamentoCompletato);
				var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

				var response = await client.PostAsync("campionamenti/duplicacampionamentocompletato", httpContent);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<Campionamento>>(responseContent);
					if (apiResponse.Success)
					{
						var campionamentoCreato = apiResponse.Data;
						// Aggiunta del campionamento allo storage locale
						_campionamenti.Add(campionamentoCreato);
						await SalvaCampionamentiInLocalStorage();
						return ResultFactory.Success(campionamentoCreato);
					}
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<Campionamento>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<Campionamento>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<Campionamento>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		/// <summary>
		/// Segna il campionamento come "modificato" impostandone la proprietà "SeModificato" a true
		/// </summary>
		/// <param name="idCampionamento"></param>
		/// <returns></returns>
        public async Task<Result> SegnaComeModificato(int idCampionamento)
        {
            try
            {
                await CaricaCampionamentiDaLocalStorage();

                if (_campionamenti.Exists(i => i.IdCampionamento == idCampionamento))
                {
                    var campionamento = _campionamenti.First(i => i.IdCampionamento == idCampionamento);
                    campionamento.SeModificato = true;
                    campionamento.UltimoAggiornamento.DataOra = DateTime.Now;
                    await SalvaCampionamentiInLocalStorage();
                    return ResultFactory.Success();
                }
                else
                {
                    return ResultFactory.Failure("Campionamento non trovato");
                }
            }
            catch (Exception ex)
            {
                string dettagliEccezione = Utils.GetExceptionDetails(ex);
                logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
                return ResultFactory.Failure($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
            }
        }
    }
}
