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
using System.Reflection;

namespace SICampionaAPI.Features.Campionamenti
{
    public class ClientiCampionamentiCompletati(ILogger<OperatoriController> logger, IRepositoryCampionamenti repositoryCampionamenti)
    {
        public class Query(string codiceFiscaleOperatore, string codiceEnte)
        {
            public string CodiceFiscaleOperatore { get; } = codiceFiscaleOperatore;
            public string CodiceEnte { get; } = codiceEnte;
        }

        public class Result(List<Cliente> clienti)
        {
            public List<Cliente> Clienti { get; } = clienti;
        }

        public ResultDto<Result> Run(Query query)
        {
            try
            {
                logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

                // TODO: generare un errore se l'ente non è tra quelli assegnati all'operatore

                var campionamenti = repositoryCampionamenti.ClientiCampionamentiCompletati(query.CodiceEnte);

                return new ResultDto<Result>(new Result(campionamenti));
            }
            catch (Exception ex)
            {
                string errorDetails = Utils.GetExceptionDetails(ex);
                return new ResultDto<Result>(new Error("Errore clienti campionamenti completati", errorDetails, ErrorType.Application));
            }
        }
    }
}
