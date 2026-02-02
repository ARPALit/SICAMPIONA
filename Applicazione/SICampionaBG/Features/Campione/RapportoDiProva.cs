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
using SICampionaBG.Data;
using SICampionaBG.Model;
using SICampionaBG.Services;
using System.Reflection;

namespace SICampionaBG.Features.Campione
{
	internal class RapportoDiProva(Configurazione configurazione, IRepositoryCampionamenti repositoryCampionamenti, ICampioni campioni)
	{
		readonly Logger logger = LogManager.GetCurrentClassLogger();

		public void LetturaRapportoDiProva()
		{
			logger.Debug($"{MethodBase.GetCurrentMethod().Name}");

			if (!configurazione.LetturaRapportoDiProvaAbilitata)
			{
				logger.Info("Lettura rapporto di prova disabilitata");
				return;
			}

			var campionamenti = repositoryCampionamenti.CampionamentiSenzaRapportoDiProva();
			if (campionamenti.Count == 0)
			{
				logger.Info("Nessun campionamento da elaborare");
				return;
			}
			foreach (var campionamento in campionamenti)
				LetturaRapportoDiProvaCampionamento(campionamento, repositoryCampionamenti);

			logger.Debug("Lettura numeri campione completata");
		}

		private void LetturaRapportoDiProvaCampionamento(Campionamento campionamento, IRepositoryCampionamenti repositoryCampionamenti)
		{
			logger.Debug($"{MethodBase.GetCurrentMethod().Name} - IdCampionamento: {campionamento.IdCampionamento}");
			try
			{
				var pdfRapportoDiProva = campioni.PDFRapportoDiProva(campionamento.NumeroCampione);
				if (pdfRapportoDiProva == null)
					logger.Info($"Campionamento {campionamento.IdCampionamento}: rapporto di prova non disponibile");
				else
				{
					logger.Info($"Campionamento {campionamento.IdCampionamento}: rapporto di prova prelevato");

					var cartellaFilePDF = Path.Combine(configurazione.CartellaRadiceCampionamenti,
						$"{campionamento.IdCampionamento}_{campionamento.SuffissoCartella}");

					// Se la cartella non esiste è creata
					if (!Directory.Exists(cartellaFilePDF))
					{
						logger.Debug($"La cartella {cartellaFilePDF} non esiste - Creazione");
						Directory.CreateDirectory(cartellaFilePDF);
					}

					string nomeFileRapportoDiProva = NomeFileRapportoDiProva(campionamento);

					var filePDF = Path.Combine(cartellaFilePDF, nomeFileRapportoDiProva);
					logger.Debug($"File PDF rapporto di prova: {filePDF}");

					File.WriteAllBytes(filePDF, pdfRapportoDiProva);

					repositoryCampionamenti.AggiornaFileRapportoDiProva(campionamento.IdCampionamento, nomeFileRapportoDiProva);
				}
				logger.Debug($"Lettura rapporto di prova IdCampionamento: {campionamento.IdCampionamento} completata");
			}
			catch (Exception exception)
			{
				logger.Error(exception, Utils.GetExceptionDetails(exception));
			}
		}

		private static string NomeFileRapportoDiProva(Campionamento campionamento)
		{
            // Il nome del file è il numero del campione, esclusi caratteri non validi, con l'estensione PDF
            return $"{Utils.SostituisceCaratteriNonValidiPerNomeFile(campionamento.NumeroCampione)}.pdf";
        }
	}
}
