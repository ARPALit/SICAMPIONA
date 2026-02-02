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
using SICampionaAPI.Services;
using System.Reflection;

namespace SICampionaAPI.Features.Operatore
{
    public class CaricaConfigurazione(ILogger<OperatoriController> logger, IAnagrafiche anagrafiche)
    {
        public class Query(string codiceFiscaleOperatore)
        {
            public string CodiceFiscaleOperatore { get; } = codiceFiscaleOperatore;
        }

        public class Result(ConfigurazioneOperatore configurazione)
        {
            public ConfigurazioneOperatore Configurazione { get; } = configurazione;
        }

        public ResultDto<Result> Run(Query query)
        {
            try
            {
                logger.LogDebug("{Metodo} - CodiceFiscale: {CodiceFiscale}", MethodBase.GetCurrentMethod().Name, query.CodiceFiscaleOperatore);

                // La feature è mantenuta anche se semplice perché può essere necessaria una elaborazione del profilo operatore
                // che non viene fatta dal servizio Anagrafiche
                return new ResultDto<Result>(new Result(anagrafiche.ConfigurazioneOperatore(query.CodiceFiscaleOperatore)));
            }
            catch (Exception ex)
            {
                string errorDetails = Utils.GetExceptionDetails(ex);
                return new ResultDto<Result>(new Error("Errore Carica configurazione", errorDetails, ErrorType.Application));
            }
        }
    }
}
