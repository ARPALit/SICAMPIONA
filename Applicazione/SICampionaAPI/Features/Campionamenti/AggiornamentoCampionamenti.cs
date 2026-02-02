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

using SICampionaAPI.Common;
using SICampionaAPI.Data;
using SICampionaAPI.Features.Operatore;
using SICampionaAPI.Services;
using System.Reflection;

namespace SICampionaAPI.Features.Campionamenti
{
    public class AggiornamentoCampionamenti(ILogger<OperatoriController> logger, IRepositoryCampionamenti repositoryCampionamenti, IAnagrafiche anagrafiche)
    {
        public class Command(List<Campionamento> elencoCampionamenti, string codiceFiscaleOperatore)
        {
            public List<Campionamento> ElencoCampionamenti { get; } = elencoCampionamenti;
            public string CodiceFiscaleOperatore { get; } = codiceFiscaleOperatore;
        }

        public class Result()
        {
        }

        public ResultDto<Result> Run(Command command)
        {
            try
            {
                logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

                // Lettura configurazione operatore
                var caricaConfigurazione = new CaricaConfigurazione(logger, anagrafiche);
                var resultConfigurazioneOperatore = caricaConfigurazione.Run(new CaricaConfigurazione.Query(command.CodiceFiscaleOperatore));
                if (resultConfigurazioneOperatore.Failed)
                    return new ResultDto<Result>(new Error($"Errore aggiornamento campionamenti: {resultConfigurazioneOperatore.Error.Message}", resultConfigurazioneOperatore.Error.Details));

                // Ogni campionamento ricevuto viene aggiornato nel database (concorrenza ottimistica)
                foreach (var campionamento in command.ElencoCampionamenti)
                {
                    campionamento.UltimoAggiornamento.Operatore.Codice = resultConfigurazioneOperatore.Data.Configurazione.CodiceAnagrafica;
                    campionamento.UltimoAggiornamento.Operatore.Cognome = resultConfigurazioneOperatore.Data.Configurazione.Cognome;
                    campionamento.UltimoAggiornamento.Operatore.Nome = resultConfigurazioneOperatore.Data.Configurazione.Nome;
                    campionamento.UltimoAggiornamento.Operatore.CodiceFiscale = resultConfigurazioneOperatore.Data.Configurazione.CodiceFiscale;
                    repositoryCampionamenti.AggiornamentoCampionamento(campionamento);
                }

                return new ResultDto<Result>(new Result());
            }
            catch (Exception ex)
            {
                string errorDetails = Utils.GetExceptionDetails(ex);
                return new ResultDto<Result>(new Error("Errore aggiornamento campionamenti", errorDetails, ErrorType.Application));
            }
        }
    }
}
