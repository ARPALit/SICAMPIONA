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
    internal class TemperaturaCampione(Configurazione configurazione, IRepositoryCampionamenti repositoryCampionamenti, ICampioni campioni)
    {
        readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void LetturaTemperaturaCampione()
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name}");

            if (!configurazione.LetturaTemperaturaAccettazioneAbilitata)
            {
                logger.Info("Lettura temperatura accettazione disabilitata");
                return;
            }

            var campionamenti = repositoryCampionamenti.CampionamentiSenzaTemperaturaAccettazione();
            if (campionamenti.Count == 0)
            {
                logger.Info("Nessun campionamento da elaborare");
                return;
            }
            foreach (var campionamento in campionamenti)
                LetturaTemperaturaAccettazioneCampione(campionamento, repositoryCampionamenti);

            logger.Debug("Lettura temperatura accettazione campione completata");
        }

        private void LetturaTemperaturaAccettazioneCampione(Campionamento campionamento, IRepositoryCampionamenti repositoryCampionamenti)
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name} - IdCampionamento: {campionamento.IdCampionamento}");
            try
            {
                var temperaturaAccettazione = campioni.TemperaturaAccettazioneCampione(campionamento.NumeroCampione);
                if (string.IsNullOrEmpty(temperaturaAccettazione))
                    logger.Info($"Campionamento {campionamento.IdCampionamento} Numero campione: {campionamento.NumeroCampione} - temperatura accettazione non disponibile");
                else
                {
                    logger.Info($"Campionamento {campionamento.IdCampionamento}: temperatura accettazione {temperaturaAccettazione}");
                    repositoryCampionamenti.AggiornaTemperaturaAccettazioneCampione(campionamento.IdCampionamento, temperaturaAccettazione);
                }
                logger.Debug($"Lettura temperatura accettazione IdCampionamento: {campionamento.IdCampionamento} completata");
            }
            catch (Exception exception)
            {
                logger.Error(exception, Utils.GetExceptionDetails(exception));
            }
        }
    }
}
