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

namespace SICampionaAPI.Features.Analiti
{
    [ApiController]
    [Route("analiti")]
    public class AnalitiController(ILogger<AnalitiController> logger, IAnagrafiche anagrafiche) : ControllerBase
    {
        /// <summary>
        /// Ricerca analiti per matrice, argomento, pacchetto, descrizione e linea
        /// </summary>
        /// <param name="codiceMatrice"></param>
        /// <param name="codiceArgomento"></param>
        /// <param name="codicePacchetto"></param>
        /// <param name="parteDelNome"></param>
        /// <param name="linea">laboratorio/territorio</param>
        /// <returns></returns>
        [HttpGet("ricerca")]
        [ProducesResponseType<ApiResponse<List<Analita>>>(StatusCodes.Status200OK)]
        public IActionResult Ricerca([Required] string codiceMatrice, string codiceArgomento, string codicePacchetto, string parteDelNome, string linea)
        {
            try
            {
                logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice} Codice argomento: {CodiceArgomento} Codice pacchetto: {CodicePacchetto} Parte del nome: {ParteDelNome} Linea: {Linea}",
                    MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento, codicePacchetto, parteDelNome, linea);

                return Ok(new ApiResponse<List<Analita>>() { Data = anagrafiche.Analiti(codiceMatrice, codiceArgomento, codicePacchetto, parteDelNome, linea) });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<List<Analita>>()
                {
                    Success = false,
                    ErrorMessage = "Errore ricerca analiti - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Ricerca pacchetto per matrice
        /// </summary>
        /// <param name="codiceMatrice"></param>
        /// <param name="codiceArgomento"></param>
        /// <param name="codiceSedeAccettazione"></param>
        /// <returns></returns>
        [HttpGet("pacchetti")]
        [ProducesResponseType<ApiResponse<List<Pacchetto>>>(StatusCodes.Status200OK)]
        public IActionResult Pacchetti([Required] string codiceMatrice, string codiceArgomento, string codiceSedeAccettazione)
        {
            try
            {
                logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice} Codice argomento: {CodiceArgomento} Codice sede accettazione: {CodiceSedeAccettazione}"
                    , MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento, codiceSedeAccettazione);

                return Ok(new ApiResponse<List<Pacchetto>>() { Data = anagrafiche.Pacchetti(codiceMatrice, codiceArgomento, codiceSedeAccettazione) });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<List<Pacchetto>>()
                {
                    Success = false,
                    ErrorMessage = "Errore ricerca pacchetti - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Ricerca contanitori per pacchetto
        /// </summary>
        /// <param name="codicePacchetto"></param>
        /// <returns></returns>
        [HttpGet("contenitori")]
        [ProducesResponseType<ApiResponse<List<Contenitore>>>(StatusCodes.Status200OK)]
        public IActionResult Contenitori([Required] string codicePacchetto)
        {
            try
            {
                logger.LogDebug("{Metodo} - Codice pacchetto: {CodicePacchetto}", MethodBase.GetCurrentMethod().Name, codicePacchetto);

                return Ok(new ApiResponse<List<Contenitore>>() { Data = anagrafiche.Contenitori(codicePacchetto) });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<List<Contenitore>>()
                {
                    Success = false,
                    ErrorMessage = "Errore ricerca contenitori - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Matrici
        /// </summary>
        /// <returns></returns>
        [HttpGet("matrici")]
        [ProducesResponseType<ApiResponse<List<MatriceNomeDescrizione>>>(StatusCodes.Status200OK)]
        public IActionResult Matrici()
        {
            try
            {
                logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

                return Ok(new ApiResponse<List<MatriceNomeDescrizione>>() { Data = anagrafiche.Matrici() });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<List<Contenitore>>()
                {
                    Success = false,
                    ErrorMessage = "Errore elenco matrici - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

    }
}
