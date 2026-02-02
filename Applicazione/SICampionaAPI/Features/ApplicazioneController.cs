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

namespace SICampionaAPI.Features.PuntiDiPrelievo
{
    [ApiController]
    public class ApplicazioneController(IConfiguration configuration) : ControllerBase
    {
        /// <summary>
        /// Root
        /// </summary>
        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Root()
        {
            return Content($"SICampiona API versione {Utils.Version()} - Versione del front-end richiesta: {configuration["VersioneFrontEnd"]}");
        }

        /// <summary>
        /// Versione del front-end richiesta
        /// </summary>
        [HttpGet]
        [Route("/versionefrontend")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult VersioneFrontEnd()
        {
            return Ok(configuration["VersioneFrontEnd"]);
        }
    }
}
