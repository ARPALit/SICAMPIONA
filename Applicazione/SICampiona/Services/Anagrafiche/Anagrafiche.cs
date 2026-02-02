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

using SICampiona.Common;
using SICampiona.Model;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace SICampiona.Services
{
	public class Anagrafiche(IHttpClientFactory clientFactory, ILogger<Anagrafiche> logger, IConfigurazioneOperatoreService infoOperatore) : IAnagrafiche
	{
		public async Task<Result<List<ClienteConIndirizzo>>> Clienti(string parteDenominazioneCodice)
		{
			logger.LogDebug("{Metodo} - Parte della denominazione o del codice: {ParteDenominazioneCodice}", MethodBase.GetCurrentMethod().Name, parteDenominazioneCodice);
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var endpoint = $"clienti/ricerca?parteDenominazioneCodice={parteDenominazioneCodice.Trim()}";

				var response = await client.GetAsync(endpoint);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ClienteConIndirizzo>>>(responseContent);
					if (apiResponse.Success)
						return ResultFactory.Success(apiResponse.Data);
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<List<ClienteConIndirizzo>>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<List<ClienteConIndirizzo>>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}

			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<ClienteConIndirizzo>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<List<PuntoDiPrelievo>>> PuntiDiPrelievo(string codiceIstatComune, string parteDenominazioneCodice)
		{
			logger.LogDebug("{Metodo} -  Codice Istat comune: {CodiceIstatComune} Parte della denominazione o del codice: {ParteDenominazioneCodice}",
				MethodBase.GetCurrentMethod().Name, codiceIstatComune, parteDenominazioneCodice);
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var endpoint = $"punti-di-prelievo/ricerca?codiceIstatComune={codiceIstatComune}";
				if (!string.IsNullOrEmpty(parteDenominazioneCodice))
					endpoint += $"&parteDenominazioneCodice={parteDenominazioneCodice.Trim()}";

				var response = await client.GetAsync(endpoint);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<PuntoDiPrelievo>>>(responseContent);
					if (apiResponse.Success)
						return ResultFactory.Success(apiResponse.Data);
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<List<PuntoDiPrelievo>>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<List<PuntoDiPrelievo>>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<PuntoDiPrelievo>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<List<Comune>>> Comuni(string parteDellaDenominazione)
		{
			try
			{
				using var httpClient = clientFactory.CreateClient("RisorseLocali");
				var anagraficaComuni = await httpClient.GetFromJsonAsync<List<Comune>>("data/Comuni.json");
				if (string.IsNullOrEmpty(parteDellaDenominazione))
					return ResultFactory.Success(anagraficaComuni.ToList());
				else
					return ResultFactory.Success(anagraficaComuni.Where(i => i.Denominazione.Contains(parteDellaDenominazione, StringComparison.CurrentCultureIgnoreCase)).ToList());
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<Comune>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<List<Analita>>> Analiti(string codiceMatrice, string codiceArgomento = null, string codicePacchetto = null, 
			string parteDelNome = null, string linea = null)
		{
			logger.LogDebug("{Metodo} -  Codice matrice: {CodiceMatrice} Codice pacchetto: {CodicePacchetto} - Parte del nome: {ParteDelNome}",
				MethodBase.GetCurrentMethod().Name, codiceMatrice, codicePacchetto, parteDelNome);
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var endpoint = $"analiti/ricerca?codiceMatrice={codiceMatrice}";

				if (!string.IsNullOrEmpty(codiceArgomento))
					endpoint += $"&codiceArgomento={codiceArgomento}";

				if (!string.IsNullOrEmpty(codicePacchetto))
					endpoint += $"&codicePacchetto={codicePacchetto}";

				if (!string.IsNullOrEmpty(parteDelNome))
					endpoint += $"&parteDelNome={parteDelNome.Trim()}";

                if (!string.IsNullOrEmpty(linea))
                    endpoint += $"&linea={linea.Trim()}";

                var response = await client.GetAsync(endpoint);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Analita>>>(responseContent);
					if (apiResponse.Success)
						return ResultFactory.Success(apiResponse.Data);
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<List<Analita>>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<List<Analita>>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<Analita>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<List<Pacchetto>>> Pacchetti(string codiceMatrice, string codiceArgomento = null, string codiceSedeAccettazione = null)
		{
			logger.LogDebug("{Metodo} -  Codice matrice: {CodiceMatrice} Codice argomento: {CodiceArgomento} Codice sede accettazione: {CodiceSedeAccettazione}",
				MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento, codiceSedeAccettazione);
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var endpoint = $"analiti/pacchetti?codiceMatrice={codiceMatrice}";

				if (!string.IsNullOrEmpty(codiceArgomento))
					endpoint += $"&codiceArgomento={codiceArgomento}";

				if (!string.IsNullOrEmpty(codiceSedeAccettazione))
					endpoint += $"&codiceSedeAccettazione={codiceSedeAccettazione}";

				var response = await client.GetAsync(endpoint);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Pacchetto>>>(responseContent);
					if (apiResponse.Success)
						return ResultFactory.Success(apiResponse.Data);
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<List<Pacchetto>>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<List<Pacchetto>>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<Pacchetto>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<List<Contenitore>>> Contenitori(string codicePacchetto)
		{
			logger.LogDebug("{Metodo} -  Codice pacchetto: {CodicePacchetto}", MethodBase.GetCurrentMethod().Name, codicePacchetto);
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var endpoint = $"analiti/contenitori?codicePacchetto={codicePacchetto}";

				var response = await client.GetAsync(endpoint);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Contenitore>>>(responseContent);
					if (apiResponse.Success)
						return ResultFactory.Success(apiResponse.Data);
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<List<Contenitore>>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<List<Contenitore>>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<Contenitore>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<List<SedeAccettazione>>> SediAccettazione()
		{
			try
			{
				using var httpClient = clientFactory.CreateClient("RisorseLocali");
				var sedi = await httpClient.GetFromJsonAsync<List<SedeAccettazione>>("data/SediAccettazione.json");
				return ResultFactory.Success(sedi.ToList());
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<SedeAccettazione>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<List<MatriceNomeDescrizione>>> Matrici()
		{
			logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var endpoint = $"analiti/matrici";

				var response = await client.GetAsync(endpoint);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<MatriceNomeDescrizione>>>(responseContent);
					if (apiResponse.Success)
						return ResultFactory.Success(apiResponse.Data);
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<List<MatriceNomeDescrizione>>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<List<MatriceNomeDescrizione>>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<MatriceNomeDescrizione>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<List<string>>> TipiCampionamento(string codiceMatrice)
		{
			logger.LogDebug("{Metodo} -  Codice matrice: {CodiceMatrice}", MethodBase.GetCurrentMethod().Name, codiceMatrice);
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var endpoint = $"campionamenti/tipi?codiceMatrice={codiceMatrice}";

				var response = await client.GetAsync(endpoint);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<string>>>(responseContent);
					if (apiResponse.Success)
						return ResultFactory.Success(apiResponse.Data);
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<List<string>>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<List<string>>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<string>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}
	}
}
