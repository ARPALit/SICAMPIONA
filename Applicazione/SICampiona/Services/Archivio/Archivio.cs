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
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace SICampiona.Services
{
	public class Archivio(ILogger<Archivio> logger, IHttpClientFactory clientFactory, IConfigurazioneOperatoreService infoOperatore) : IArchivio
	{
		public async Task<Result<List<InfoCampionamentoCompletato>>> ElencoCampionamentiCompletati(RichiestaCampionamentiCompletati richiesta)
		{
			logger.LogDebug("{Metodo} - Richiesta: {Richiesta}", MethodBase.GetCurrentMethod().Name, JsonSerializer.Serialize(richiesta));
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var jsonContent = JsonSerializer.Serialize(richiesta);
				var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

				var response = await client.PostAsync("campionamenti/completati", httpContent);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<InfoCampionamentoCompletato>>>(responseContent);
					if (apiResponse.Success)
						return ResultFactory.Success(apiResponse.Data);
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<List<InfoCampionamentoCompletato>>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<List<InfoCampionamentoCompletato>>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<InfoCampionamentoCompletato>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}

		public async Task<Result<List<Cliente>>> ClientiCampionamentiCompletati(string codiceEnte)
		{
			logger.LogDebug("{Metodo} - Codice ente: {codiceEnte}", MethodBase.GetCurrentMethod().Name, codiceEnte);
			try
			{
				using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var response = await client.GetAsync($"campionamenti/completati/clienti/{codiceEnte}");

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Cliente>>>(responseContent);
					if (apiResponse.Success)
						return ResultFactory.Success(apiResponse.Data);
					else
					{
						logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
						return ResultFactory.Failure<List<Cliente>>(apiResponse.ErrorMessage);
					}
				}
				else
				{
					logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					return ResultFactory.Failure<List<Cliente>>($"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}");
				}
			}
			catch (Exception ex)
			{
				string dettagliEccezione = Utils.GetExceptionDetails(ex);
				logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
				return ResultFactory.Failure<List<Cliente>>($"{MethodBase.GetCurrentMethod().Name} - Errore: {dettagliEccezione}");
			}
		}
	}
}
