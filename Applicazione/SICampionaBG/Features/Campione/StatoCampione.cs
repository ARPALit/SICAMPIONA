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
	internal class StatoCampione(Configurazione configurazione, IRepositoryCampionamenti repositoryCampionamenti, ICampioni campioni)
	{
		readonly Logger logger = LogManager.GetCurrentClassLogger();

		public void LetturaStatoCampione()
		{
			logger.Debug($"{MethodBase.GetCurrentMethod().Name}");

			if (!configurazione.LetturaStatoCampioneAbilitata)
			{
				logger.Info("Lettura stato campione disabilitata");
				return;
			}

			var campionamenti = repositoryCampionamenti.CampionamentiConStatoCampioneDaAggiornare();
			if (campionamenti.Count == 0)
			{
				logger.Info("Nessun campionamento da elaborare");
				return;
			}
			foreach (var campionamento in campionamenti)
				LetturaStatoCampioneCampionamento(campionamento, repositoryCampionamenti);

			logger.Debug("Lettura stati campione completata");
		}

		private void LetturaStatoCampioneCampionamento(Campionamento campionamento, IRepositoryCampionamenti repositoryCampionamenti)
		{
			logger.Debug($"{MethodBase.GetCurrentMethod().Name} - IdCampionamento: {campionamento.IdCampionamento}");
			try
			{
				var statoCampione = campioni.StatoCampione(campionamento.NumeroCampione);
				if (string.IsNullOrEmpty(statoCampione))
					logger.Info($"Campionamento {campionamento.IdCampionamento} Numero campione: {campionamento.NumeroCampione} - stato campione non disponibile");
				else
				{
					logger.Info($"Campionamento {campionamento.IdCampionamento}: stato campione {statoCampione}");
					repositoryCampionamenti.AggiornaStatoCampione(campionamento.IdCampionamento, statoCampione);
				}
				logger.Debug($"Lettura stato campione IdCampionamento: {campionamento.IdCampionamento} completata");
			}
			catch (Exception exception)
			{
				logger.Error(exception, Utils.GetExceptionDetails(exception));
			}
		}
	}
}
