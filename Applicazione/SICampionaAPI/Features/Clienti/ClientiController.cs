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

using Microsoft.AspNetCore.Mvc;
using SICampionaAPI.Common;
using SICampionaAPI.Services;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SICampionaAPI.Features.Clienti
{
    [ApiController]
    [Route("clienti")]
    public class ClientiController(ILogger<ClientiController> logger, IAnagrafiche anagrafiche) : ControllerBase
    {
        /// <summary>
        /// Ricerca clienti per denominazione
        /// </summary>
        /// <param name="parteDenominazioneCodice"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        [HttpGet("ricerca")]
        [ProducesResponseType<ApiResponse<List<ClienteConIndirizzo>>>(StatusCodes.Status200OK)]
        public IActionResult Ricerca([Required] string parteDenominazioneCodice)
        {
            try
            {
                logger.LogDebug("{Metodo} - {Denominazione}", MethodBase.GetCurrentMethod().Name, parteDenominazioneCodice);

                return Ok(new ApiResponse<List<ClienteConIndirizzo>>() { Data = anagrafiche.Clienti(parteDenominazioneCodice) });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<List<ClienteConIndirizzo>>()
                {
                    Success = false,
                    ErrorMessage = "Errore ricerca clienti - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }
    }
}
