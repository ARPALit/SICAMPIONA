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
using NLog;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SICampionaBG.Services
{
	internal class Campioni : ICampioni
	{
		private readonly Configurazione _configurazione;
		private readonly HttpClient clientHttp;
		private readonly Logger logger = LogManager.GetCurrentClassLogger();

		public Campioni(Configurazione configurazione)
		{
			_configurazione = configurazione;
			clientHttp = new HttpClient() { BaseAddress = new Uri(configurazione.APICampioniBaseUrl) };
		}

		public string NumeroCampione(string siglaVerbale)
		{
			logger.Debug($"{MethodBase.GetCurrentMethod().Name} - Sigla verbale: {siglaVerbale}");

			var endpoint = _configurazione.APICampioniNumeroCampione
				.Replace("[sigla_verbale]", siglaVerbale)
				.Replace("[apikey]", _configurazione.APICampionamentiAPIKey);

			var response = clientHttp.GetAsync(endpoint).Result;

			if (response.IsSuccessStatusCode)
			{
				var responseContent = response.Content.ReadAsStringAsync().Result;
				var result = JsonSerializer.Deserialize<RispostaAwsNumeroCampioneEPuntoPrelievo>(responseContent);
				if (result.Items.Length == 0)
					return null;
				else
				{
					string numeroCampione = Utils.TrimAndNull(result.Items[0].NumeroCampione);
					logger.Info($"Sigla verbale: {siglaVerbale} -> Numero campione: {numeroCampione}");
					return numeroCampione;
				}
			}
			else
			{
				string errore = $"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}";
				logger.Error(errore);
				throw new HttpRequestException(errore);
			}
		}

		public Byte[] PDFRapportoDiProva(string numeroCampione)
		{
			logger.Debug($"{MethodBase.GetCurrentMethod().Name} - Numero campione: {numeroCampione}");

			var endpoint = _configurazione.APICampioniRapportoDiProva
				.Replace("[numero_campione]", numeroCampione)
                .Replace("[apikey]", _configurazione.APICampionamentiAPIKey);

			var response = clientHttp.GetAsync(endpoint).Result;

			if (response.IsSuccessStatusCode)
			{
				if (response.Content.Headers.ContentType.MediaType != "application/pdf")
				{
					logger.Info($"Numero campione: {numeroCampione}: la risposta delle API non è 'application/pdf' (ricevuto: {response.Content.Headers.ContentType.MediaType})");
					return null;
				}
				if (response.Content.Headers.ContentLength == null || response.Content.Headers.ContentLength == 0)
				{
					logger.Info($"Numero campione: {numeroCampione}: la risposta delle API non contiene il file");
					return null;
				}
				return response.Content.ReadAsByteArrayAsync().Result;
			}
			else
			{
				string errore = $"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}";
				logger.Error(errore);
				throw new HttpRequestException(errore);
			}
		}

		public string StatoCampione(string numeroCampione)
		{
			logger.Debug($"{MethodBase.GetCurrentMethod().Name} - Numero campione: {numeroCampione}");

			var endpoint = _configurazione.APICampioniStatoCampione
				.Replace("[numero_campione]", numeroCampione)
                .Replace("[apikey]", _configurazione.APICampionamentiAPIKey);

			var response = clientHttp.GetAsync(endpoint).Result;

			if (response.IsSuccessStatusCode)
			{
				var responseContent = response.Content.ReadAsStringAsync().Result;
				var result = JsonSerializer.Deserialize<RispostaAWStatoCampione>(responseContent);
				if (result.Items.Length == 0)
					return null;
				else
				{
					string statoCampione = Utils.TrimAndNull(result.Items[0].StatoCampione);
					logger.Info($"Numero campione: {numeroCampione} -> Stato campione: {statoCampione}");
					return statoCampione;
				}
			}
			else
			{
				string errore = $"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}";
				logger.Error(errore);
				throw new HttpRequestException(errore);
			}
		}

        public string TemperaturaAccettazioneCampione(string numeroCampione)
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name} - Numero campione: {numeroCampione}");

            var endpoint = _configurazione.APICampioniStatoCampione
                .Replace("[numero_campione]", numeroCampione)
                .Replace("[apikey]", _configurazione.APICampionamentiAPIKey);

            var response = clientHttp.GetAsync(endpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<RispostaAWTemperaturaAccettazioneCampione>(responseContent);
                if (result.Items.Length == 0)
                    return null;
                else
                {
                    string temperaturaAccettazione = Utils.TrimAndNull(result.Items[0].TemperaturaAccettazione);
                    logger.Info($"Numero campione: {numeroCampione} -> Temperatura accettazione: {temperaturaAccettazione}");
                    return temperaturaAccettazione;
                }
            }
            else
            {
                string errore = $"Errore chiamata client http - Risposta: {response.StatusCode} - {response.ReasonPhrase}";
                logger.Error(errore);
                throw new HttpRequestException(errore);
            }
        }
    }

	/// <summary>
	/// Risposta API per la richieata numero campione
	/// </summary>
	internal class RispostaAwsNumeroCampioneEPuntoPrelievo
	{
		[JsonPropertyName("items")]
		public Item[] Items { get; set; }
		public class Item
		{
			[JsonPropertyName("id_text")]
			public string NumeroCampione { get; set; }

			[JsonPropertyName("sampling_point")]
			public string PuntoPrelievoCodice { get; set; }

			[JsonPropertyName("description")]
			public string PuntoPrelievoDenominazione { get; set; }

			[JsonPropertyName("sampling_point_indirizzo")]
			public string PuntoPrelievoIndirizzo { get; set; }

			[JsonPropertyName("sampling_point_comune")]
			public string PuntoPrelievoDenominazioneComune { get; set; }
		}
	}

	/// <summary>
	/// Risposta API per la richieata stato campione
	/// </summary>
	internal class RispostaAWStatoCampione
	{
		[JsonPropertyName("items")]
		public Item[] Items { get; set; }
		public class Item
		{
			[JsonPropertyName("id_text")]
			public string NumeroCampione { get; set; }

			[JsonPropertyName("stato")]
			public string StatoCampione { get; set; }
        }
    }

    /// <summary>
    /// Risposta API per la richieata temperatura accettazione campione
    /// </summary>
    internal class RispostaAWTemperaturaAccettazioneCampione
    {
        [JsonPropertyName("items")]
        public Item[] Items { get; set; }
        public class Item
        {
            [JsonPropertyName("id_text")]
            public string NumeroCampione { get; set; }

            [JsonPropertyName("temperatura")]
            public string TemperaturaAccettazione { get; set; }
        }
    }
}
