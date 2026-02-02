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
	internal class NumeroCampione(Configurazione configurazione, IRepositoryCampionamenti repositoryCampionamenti, ICampioni campioni)
	{
		readonly Logger logger = LogManager.GetCurrentClassLogger();

		public void LetturaNumeroCampione()
		{
			logger.Debug($"{MethodBase.GetCurrentMethod().Name}");

			if (!configurazione.LetturaNumeroCampioneAbilitata)
			{
				logger.Info("Lettura numero campione disabilitata");
				return;
			}

			var campionamenti = repositoryCampionamenti.CampionamentiSenzaNumeroCampione();
			if (campionamenti.Count == 0)
			{
				logger.Info("Nessun campionamento da elaborare");
				return;
			}
			foreach (var campionamento in campionamenti)
				LetturaNumeroCampioneCampionamento(campionamento, repositoryCampionamenti);

			logger.Debug("Lettura numeri campione completata");
		}

		private void LetturaNumeroCampioneCampionamento(Campionamento campionamento, IRepositoryCampionamenti repositoryCampionamenti)
		{
			logger.Debug($"{MethodBase.GetCurrentMethod().Name} - IdCampionamento: {campionamento.IdCampionamento}");
			try
			{
				var numeroCampione = campioni.NumeroCampione(campionamento.SiglaVerbale);
				if (string.IsNullOrEmpty(numeroCampione))
					logger.Info($"Campionamento {campionamento.IdCampionamento}: numero campione non disponibile");
				else
				{
					logger.Info($"Campionamento {campionamento.IdCampionamento}: numero campione {numeroCampione}");
					repositoryCampionamenti.AggiornaNumeroCampione(campionamento.IdCampionamento, numeroCampione);
				}
				logger.Debug($"Lettura numero campione IdCampionamento: {campionamento.IdCampionamento} completata");
			}
			catch (Exception exception)
			{
				logger.Error(Utils.GetExceptionDetails(exception));
			}
		}
	}
}
