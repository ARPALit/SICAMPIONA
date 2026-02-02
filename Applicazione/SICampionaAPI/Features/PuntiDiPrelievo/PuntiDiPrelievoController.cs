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
using SICampionaAPI.Features.Campionamenti;
using SICampionaAPI.Services;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SICampionaAPI.Features.PuntiDiPrelievo
{
    [ApiController]
    [Route("punti-di-prelievo")]
    public class PuntiDiPrelievoController(ILogger<PuntiDiPrelievoController> logger, IAnagrafiche anagrafiche) : ControllerBase
    {
        /// <summary>
        /// Ricerca punto di prelievo per comune e denominazione
        /// </summary>
        /// <param name="codiceIstatComune"></param>
        /// <param name="parteDenominazioneCodice"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        [HttpGet("ricerca")]
        [ProducesResponseType<ApiResponse<List<PuntoDiPrelievo>>>(StatusCodes.Status200OK)]
        public IActionResult RicercaPuntoDiPrelievo([Required] string codiceIstatComune, string parteDenominazioneCodice)
        {
            try
            {
                logger.LogDebug("{Metodo} - Codice Istat: {CodiceIstat} Parte della denominazione o del codice: {ParteDenominazioneCodice}",
                    MethodBase.GetCurrentMethod().Name, codiceIstatComune, parteDenominazioneCodice);

                return Ok(new ApiResponse<List<PuntoDiPrelievo>>() { Data = anagrafiche.PuntiDiPrelievo(codiceIstatComune, parteDenominazioneCodice) });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<List<PuntoDiPrelievo>>()
                {
                    Success = false,
                    ErrorMessage = "Errore ricerca punto di prelievo - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }
    }
}
