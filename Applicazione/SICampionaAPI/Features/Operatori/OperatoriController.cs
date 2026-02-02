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
using System.Reflection;

namespace SICampionaAPI.Features.Operatore
{
    [ApiController]
    [Route("operatori")]
    public class OperatoriController(ILogger<OperatoriController> logger, IAnagrafiche anagrafiche) : ControllerBase
    {
        /// <summary>
        /// Configurazione operatore in base al codice fiscale letto dall'header
        /// </summary>
        /// <returns></returns>
        [HttpGet("configurazione")]
        public IActionResult Configurazione()
        {
            try
            {
                logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

                // Lettura codice fiscale operatore dagli items della request
                var codiceFiscaleOperatore = (string)HttpContext.Items["CodiceFiscale"];

                // Lettura configurazione
                var caricaConfigurazione = new CaricaConfigurazione(logger, anagrafiche);
                var result = caricaConfigurazione.Run(new CaricaConfigurazione.Query(codiceFiscaleOperatore));

                if (result.Failed)
                {
                    logger.LogError("{ExceptionMessage} - {ExceptionDetails}", result.Error.Message, result.Error.Details);
                    return Ok(new ApiResponse<ConfigurazioneOperatore>()
                    {
                        Success = false,
                        ErrorMessage = "Errore lettura configurazione operatore - Contattare l'amministratore",
                        ErrorDetails = $"{result.Error.Message} - {result.Error.Details}"
                    });
                }

                return Ok(new ApiResponse<ConfigurazioneOperatore>() { Data = result.Data.Configurazione });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<ConfigurazioneOperatore>()
                {
                    Success = false,
                    ErrorMessage = "Errore lettura configurazione operatore - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }
    }
}
