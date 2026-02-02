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
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;
using QuestPDF.Infrastructure;
using SICampionaBG.Data;
using System.Configuration;

namespace SICampionaBG
{
	internal static class Program
	{
		static void Main()
		{
			// Caricamento configurazione da appsettings.json
			var configurationRoot = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			// Lettura configurazione NLog
			LogManager.Configuration = new NLogLoggingConfiguration(configurationRoot.GetSection("NLog"));

			// Creazione logger
			var logger = LogManager.GetCurrentClassLogger();

			try
			{
				logger.Info($"Avvio programma - Versione: {Utils.Version()} - Hostname: {System.Net.Dns.GetHostName()}");

				QuestPDF.Settings.License = LicenseType.Community;

				// Lettura impostazioni di configurazione da appsettings.json in un'istanza di Configurazione
				var configurazione = configurationRoot.GetSection("Configurazione").Get<Configurazione>() ?? throw new ConfigurationErrorsException("Sezione 'Configurazione' mancante in appsettings");
				// Validazione impostazioni di configurazione
				configurazione.Valida();

				// Repository campionamenti
				var repositoryCampionamenti = new RepositoryCampionamenti(configurazione);

				// Servizio dati sui campioni
				var campioni = new Services.Campioni(configurazione);

				// Creazione PDF verbale
				var creazionePDFVerbale = new Features.CreazionePDFVerbale.PdfVerbale(configurazione, repositoryCampionamenti);
				creazionePDFVerbale.CreaVerbali();

				// Lettura numero campione
				var numeroCampione = new Features.Campione.NumeroCampione(configurazione, repositoryCampionamenti, campioni);
				numeroCampione.LetturaNumeroCampione();

				// Lettura stato campione
				var statoCampione = new Features.Campione.StatoCampione(configurazione, repositoryCampionamenti, campioni);
				statoCampione.LetturaStatoCampione();

				// Lettura rapporto di prova
				var rapportoDiProva = new Features.Campione.RapportoDiProva(configurazione, repositoryCampionamenti, campioni);
				rapportoDiProva.LetturaRapportoDiProva();

                // Lettura temperatura accettazione campione
                var temperaturaAccettazioneCampione = new Features.Campione.TemperaturaCampione(configurazione, repositoryCampionamenti, campioni);
                temperaturaAccettazioneCampione.LetturaTemperaturaCampione();

                // Invio email verbale
                var emailVerbale = new Features.EmailVerbale.EmailVerbale(configurazione, repositoryCampionamenti);
                emailVerbale.Invio();

                logger.Info("Elaborazione completata");
			}
			catch (Exception exception)
			{
				logger.Error(exception, Utils.GetExceptionDetails(exception));
				Environment.ExitCode = -1;
			}
			finally
			{
				logger.Info("Flush messaggi di log");
				LogManager.Flush();
			}
		}
	}
}
