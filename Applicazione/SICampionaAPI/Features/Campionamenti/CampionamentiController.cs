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
using SICampionaAPI.Data;
using SICampionaAPI.Features.Campionamenti;
using SICampionaAPI.Features.Operatore;
using SICampionaAPI.Services;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using static SICampionaAPI.Features.Campionamenti.CreazioneCampionamento;
using static SICampionaAPI.Features.Campionamenti.DuplicazioneCampionamento;
using static SICampionaAPI.Features.Campionamenti.DuplicazioneCampionamentoCompletato;

namespace SICampionaAPI.Features.Campionamento
{
    [ApiController]
    [Route("campionamenti")]
    public class CampionamentiController(ILogger<OperatoriController> logger, IConfiguration configuration,
        IRepositoryCampionamenti repositoryCampionamenti, IAnagrafiche anagrafiche) : ControllerBase
    {
        /// <summary>
        /// Elenco dei campionamenti aperti per l'operatore
        /// </summary>
        /// <returns></returns>
        [HttpGet("aperti")]
        [ProducesResponseType<ApiResponse<List<Campionamenti.Campionamento>>>(StatusCodes.Status200OK)]
        public IActionResult CampionamentiAperti()
        {
            try
            {
                var campionamentiAperti = new ElencoCampionamentiAperti(logger, repositoryCampionamenti, anagrafiche);
                var result = campionamentiAperti.Run(new ElencoCampionamentiAperti.Query((string)HttpContext.Items["CodiceFiscale"], configuration["URLApplicazione"]));

                if (result.Failed)
                {
                    logger.LogError("{ExceptionMessage} - {ExceptionDetails}", result.Error.Message, result.Error.Details);
                    return Ok(new ApiResponse<List<Campionamenti.Campionamento>>()
                    {
                        Success = false,
                        ErrorMessage = "Errore lettura campionamenti aperti - Contattare l'amministratore",
                        ErrorDetails = $"{result.Error.Message} - {result.Error.Details}"
                    });
                }

                return Ok(new ApiResponse<List<Campionamenti.Campionamento>>()
                {
                    Data = result.Data.Campionamenti
                });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<Campionamenti.Campionamento>()
                {
                    Success = false,
                    ErrorMessage = "Errore lettura campionamenti aperti - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Inserimento campionamento
        /// </summary>
        /// <param name="richiesta"></param>
        /// <returns></returns>
        [HttpPost("nuovo")]
        [ProducesResponseType<ApiResponse<Campionamenti.Campionamento>>(StatusCodes.Status200OK)]
        public IActionResult NuovoCampionamento([FromBody] RichiestaNuovoCampionamento richiesta)
        {
            try
            {
                var creazioneCampionamento = new CreazioneCampionamento(logger, repositoryCampionamenti, anagrafiche);
                var result = creazioneCampionamento.Run(new CreazioneCampionamento.Command(richiesta, (string)HttpContext.Items["CodiceFiscale"]));

                if (result.Failed)
                {
                    logger.LogError("{ExceptionMessage} - {ExceptionDetails}", result.Error.Message, result.Error.Details);
                    return Ok(new ApiResponse<Campionamenti.Campionamento>()
                    {
                        Success = false,
                        ErrorMessage = "Errore creazione campionamento - Contattare l'amministratore",
                        ErrorDetails = $"{result.Error.Message} - {result.Error.Details}"
                    });
                }

                return Ok(new ApiResponse<Campionamenti.Campionamento>()
                {
                    Data = result.Data.CampionmentoCreato
                });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<Campionamenti.Campionamento>()
                {
                    Success = false,
                    ErrorMessage = "Errore creazione campionamento - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Aggiornamento campionamenti
        /// </summary>
        /// <param name="elencoCampionamenti"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        [HttpPut("aggiornamento")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        public IActionResult AggiornamentoCampionamenti([FromBody] List<Campionamenti.Campionamento> elencoCampionamenti)
        {
            try
            {
                var aggiornamentoCampionamenti = new AggiornamentoCampionamenti(logger, repositoryCampionamenti, anagrafiche);
                var result = aggiornamentoCampionamenti.Run(new AggiornamentoCampionamenti.Command(elencoCampionamenti, (string)HttpContext.Items["CodiceFiscale"]));

                if (result.Failed)
                {
                    logger.LogError("{ExceptionMessage} - {ExceptionDetails}", result.Error.Message, result.Error.Details);
                    return Ok(new ApiResponse<string>()
                    {
                        Success = false,
                        ErrorMessage = "Errore aggiornamento campionamenti - Contattare l'amministratore",
                        ErrorDetails = $"{result.Error.Message} - {result.Error.Details}"
                    });
                }

                return Ok(new ApiResponse<string>()
                {
                    Data = null
                });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<string>()
                {
                    Success = false,
                    ErrorMessage = "Errore aggiornamento campionamenti - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Campionamenti completati per l'operatore
        /// </summary>
        /// <param name="richiesta"></param>
        /// <returns></returns>
        [HttpPost("completati")]
        [ProducesResponseType<ApiResponse<List<ElencoCampionamentiCompletati.InfoCampionamentoCompletato>>>(StatusCodes.Status200OK)]
        public IActionResult CampionamentiCompletati([FromBody] ElencoCampionamentiCompletati.RichiestaCampionamentiCompletati richiesta)
        {
            try
            {
                var campionamentiCompletati = new ElencoCampionamentiCompletati(logger, repositoryCampionamenti);
                var result = campionamentiCompletati.Run(new Campionamenti.ElencoCampionamentiCompletati
                    .Query((string)HttpContext.Items["CodiceFiscale"], richiesta, configuration["URLApplicazione"]));

                if (result.Failed)
                {
                    logger.LogError("{ExceptionMessage} - {ExceptionDetails}", result.Error.Message, result.Error.Details);
                    return Ok(new ApiResponse<List<ElencoCampionamentiCompletati.InfoCampionamentoCompletato>>()
                    {
                        Success = false,
                        ErrorMessage = "Errore lettura campionamenti completati - Contattare l'amministratore",
                        ErrorDetails = $"{result.Error.Message} - {result.Error.Details}"
                    });
                }

                return Ok(new ApiResponse<List<ElencoCampionamentiCompletati.InfoCampionamentoCompletato>>()
                {
                    Data = result.Data.Campionamenti
                });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<ElencoCampionamentiCompletati.InfoCampionamentoCompletato>()
                {
                    Success = false,
                    ErrorMessage = "Errore lettura campionamenti completati - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Dati dei campionamenti che non hanno il numero campione e per i quali esiste il file PDF del verbale
        /// </summary>
        /// <param name="APIKey"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        [HttpGet("senzanumerocampione/{APIKey}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<CampionamentoExport>>))]
        public IActionResult CampionamentiSenzaNumeroCampione([Required] string APIKey)
        {
            try
            {
                if (APIKey != configuration["APIKeyEndpointSenzaNumeroCampione"])
                    return Ok(new ApiResponse<CampionamentoExport>()
                    {
                        Success = false,
                        ErrorMessage = "APIKey non valida"
                    });

                var campionamenti = new ElencoCampionamentiSenzaNumeroCampione(logger, repositoryCampionamenti);
                var result = campionamenti.Run();

                if (result.Failed)
                {
                    logger.LogError("{ExceptionMessage} - {ExceptionDetails}", result.Error.Message, result.Error.Details);
                    return Ok(new ApiResponse<List<CampionamentoExport>>()
                    {
                        Success = false,
                        ErrorMessage = "Errore lettura campionamenti senza numero campione - Contattare l'amministratore",
                        ErrorDetails = $"{result.Error.Message} - {result.Error.Details}"
                    });
                }

                return Ok(new ApiResponse<List<CampionamentoExport>>()
                {
                    Data = result.Data.Campionamenti
                });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<CampionamentoExport>()
                {
                    Success = false,
                    ErrorMessage = "Errore lettura campionamenti senza numero campione - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Clienti dei campionamenti completati
        /// </summary>
        /// <param name="CodiceEnte"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        [HttpGet("completati/clienti/{CodiceEnte}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<Cliente>>))]
        public IActionResult ClientiCampionamentiCompletati([Required] string CodiceEnte)
        {
            try
            {
                var clienti = new ClientiCampionamentiCompletati(logger, repositoryCampionamenti);
                var result = clienti.Run(new ClientiCampionamentiCompletati.Query((string)HttpContext.Items["CodiceFiscale"], CodiceEnte));

                if (result.Failed)
                {
                    logger.LogError("{ExceptionMessage} - {ExceptionDetails}", result.Error.Message, result.Error.Details);
                    return Ok(new ApiResponse<List<Cliente>>()
                    {
                        Success = false,
                        ErrorMessage = "Errore lettura clienti campionamenti completati - Contattare l'amministratore",
                        ErrorDetails = $"{result.Error.Message} - {result.Error.Details}"
                    });
                }

                return Ok(new ApiResponse<List<Cliente>>()
                {
                    Data = result.Data.Clienti
                });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<Cliente>()
                {
                    Success = false,
                    ErrorMessage = "Errore lettura clienti campionamenti completati - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Duplicazione campionamento
        /// </summary>
        /// <param name="richiesta"></param>
        /// <returns></returns>
        [HttpPost("duplica")]
        [ProducesResponseType<ApiResponse<Campionamenti.Campionamento>>(StatusCodes.Status200OK)]
        public IActionResult DuplicaCampionamento([FromBody] RichiestaDuplicazioneCampionamento richiesta)
        {
            try
            {
                var duplicazioneCampionamento = new DuplicazioneCampionamento(logger, repositoryCampionamenti, anagrafiche);
                var result = duplicazioneCampionamento.Run(new DuplicazioneCampionamento.Command(richiesta, (string)HttpContext.Items["CodiceFiscale"]));

                if (result.Failed)
                {
                    logger.LogError("{ExceptionMessage} - {ExceptionDetails}", result.Error.Message, result.Error.Details);
                    return Ok(new ApiResponse<Campionamenti.Campionamento>()
                    {
                        Success = false,
                        ErrorMessage = "Errore duplicazione campionamento - Contattare l'amministratore",
                        ErrorDetails = $"{result.Error.Message} - {result.Error.Details}"
                    });
                }

                return Ok(new ApiResponse<Campionamenti.Campionamento>()
                {
                    Data = result.Data.CampionmentoCreato
                });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<Campionamenti.Campionamento>()
                {
                    Success = false,
                    ErrorMessage = "Errore duplicazione campionamento - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Duplicazione campionamento completato
        /// </summary>
        /// <param name="richiesta"></param>
        /// <returns></returns>
        [HttpPost("duplicacampionamentocompletato")]
        [ProducesResponseType<ApiResponse<Campionamenti.Campionamento>>(StatusCodes.Status200OK)]
        public IActionResult DuplicaCampionamentoCompletato([FromBody] RichiestaDuplicazioneCampionamentoCompletato richiesta)
        {
            try
            {
                var duplicazioneCampionamentoCompletato = new DuplicazioneCampionamentoCompletato(logger, repositoryCampionamenti, anagrafiche);
                var result = duplicazioneCampionamentoCompletato.Run(new DuplicazioneCampionamentoCompletato.Command(richiesta, (string)HttpContext.Items["CodiceFiscale"]));

                if (result.Failed)
                {
                    logger.LogError("{ExceptionMessage} - {ExceptionDetails}", result.Error.Message, result.Error.Details);
                    return Ok(new ApiResponse<Campionamenti.Campionamento>()
                    {
                        Success = false,
                        ErrorMessage = "Errore duplicazione campionamento completato - Contattare l'amministratore",
                        ErrorDetails = $"{result.Error.Message} - {result.Error.Details}"
                    });
                }

                return Ok(new ApiResponse<Campionamenti.Campionamento>()
                {
                    Data = result.Data.CampionmentoCreato
                });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<Campionamenti.Campionamento>()
                {
                    Success = false,
                    ErrorMessage = "Errore duplicazione campionamento completato - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        /// <summary>
        /// Elenco tipi campionamento per codice matrice
        /// </summary>
        /// <param name="codiceMatrice"></param>
        /// <returns></returns>
        [HttpGet("tipi")]
        [ProducesResponseType<List<string>>(StatusCodes.Status200OK)]
        public IActionResult TipiCampionamento([Required] string codiceMatrice)
        {
            try
            {
                logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice}", MethodBase.GetCurrentMethod().Name, codiceMatrice);

                return Ok(new ApiResponse<List<string>>() { Data = repositoryCampionamenti.ElencoTipiCampionamento(codiceMatrice) });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<List<string>>()
                {
                    Success = false,
                    ErrorMessage = "Errore elenco tipi campionamento - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }

        [HttpPost("uploadverbalefirmato/{IdCampionamento}")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        public IActionResult UploadVerbaleFirmato(int IdCampionamento, [FromForm] IFormFile file)
        {
            try
            {
                // Verifica che il file sia stato caricato
                if (file == null || file.Length == 0)
                {
                    logger.LogError("File non caricato");
                    return Ok(new ApiResponse<string>()
                    {
                        Success = false,
                        ErrorMessage = "File non caricato"
                    });
                }

                // Verifica che l'estensione del file sia PDF
                if (!Path.GetExtension(file.FileName).ToLower().Equals(".pdf"))
                {
                    logger.LogError("Estensione file {FileName} non valida", file.FileName);
                    return Ok(new ApiResponse<string>()
                    {
                        Success = false,
                        ErrorMessage = $"Estensione file {file.FileName} non valida"
                    });
                }

                var uploadVerbaleFirmato = new UploadVerbaleFirmato(logger, repositoryCampionamenti);
                var result = uploadVerbaleFirmato.Run(new UploadVerbaleFirmato.Command(IdCampionamento, file));

                if (result.Failed)
                {
                    logger.LogError("{ExceptionMessage} - {ExceptionDetails}", result.Error.Message, result.Error.Details);
                    return Ok(new ApiResponse<string>()
                    {
                        Success = false,
                        ErrorMessage = "Errore upload file verbale firmato - Contattare l'amministratore",
                        ErrorDetails = $"{result.Error.Message} - {result.Error.Details}"
                    });
                }

                return Ok(new ApiResponse<string>()
                {
                    Data = string.Empty
                });
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionDetails}", Utils.GetExceptionDetails(ex));
                return Ok(new ApiResponse<string>()
                {
                    Success = false,
                    ErrorMessage = "Errore duplicazione campionamento completato - Contattare l'amministratore",
                    ErrorDetails = Utils.GetExceptionDetails(ex)
                });
            }
        }
    }
}
