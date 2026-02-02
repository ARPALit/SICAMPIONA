using SICampionaAPI.Common;
using SICampionaAPI.Data;
using SICampionaAPI.Features.Operatore;
using SICampionaAPI.Services;
using System.Reflection;
using System.Text.Json;

namespace SICampionaAPI.Features.Campionamenti
{
    public class DuplicazioneCampionamento(ILogger<OperatoriController> logger, IRepositoryCampionamenti repositoryCampionamenti, IAnagrafiche anagrafiche)
    {
        public class Command(RichiestaDuplicazioneCampionamento richiesta, string codiceFiscaleOperatore)
        {
            public RichiestaDuplicazioneCampionamento Richiesta { get; } = richiesta;
            public string CodiceFiscaleOperatore { get; } = codiceFiscaleOperatore;
        }

        public ResultDto<Result> Run(Command command)
        {
            try
            {
                logger.LogDebug("{Metodo} - Richiesta: {Richiesta}", MethodBase.GetCurrentMethod().Name, JsonSerializer.Serialize(command.Richiesta));

                // Lettura configurazione operatore
                var caricaConfigurazione = new CaricaConfigurazione(logger, anagrafiche);
                var resultConfigurazioneOperatore = caricaConfigurazione.Run(new CaricaConfigurazione.Query(command.CodiceFiscaleOperatore));
                if (resultConfigurazioneOperatore.Failed)
                    return new ResultDto<Result>(new Error($"Errore duplicazione campionamento: {resultConfigurazioneOperatore.Error.Message}", resultConfigurazioneOperatore.Error.Details));

                // Duplicazione campionamento				
                Campionamento campionamento = DuplicaCampionamento(command, resultConfigurazioneOperatore.Data.Configurazione);

                return new ResultDto<Result>(new Result(campionamento));
            }
            catch (Exception ex)
            {
                string errorDetails = Utils.GetExceptionDetails(ex);
                return new ResultDto<Result>(new Error("Errore duplicazione campionamento", errorDetails, ErrorType.Application));
            }
        }

        public class Result(Campionamento campionamentoCreato)
        {
            public Campionamento CampionmentoCreato { get; } = campionamentoCreato;
        }

        public class RichiestaDuplicazioneCampionamento
        {
            public int IdCampionamentoDaDuplicare { get; set; }
            public PuntoDiPrelievo PuntoDiPrelievoARPAL { get; set; }
            public PuntoDiPrelievo PuntoDiPrelievoInseritoDaOperatore { get; set; }

            /// <summary>
            /// Indica se il nuovo campionamento è collegato all'originale
            /// </summary>
            public bool CreaCollegamento { get; set; }
            /// <summary>
            /// Eventuale data prelievo del nuovo campionamento
            /// </summary>
            public DateOnly? NuovaDataPrelievo { get; set; }
            public string TipoCampionamento { get; set; }
        }

        /// <summary>
        /// Duplicazione campionamento con dati della richiesta e dell'operatore
        /// </summary>
        /// <param name="command"></param>
        /// <param name="configurazioneOperatore"></param>
        /// <returns></returns>
        private Campionamento DuplicaCampionamento(Command command, ConfigurazioneOperatore configurazioneOperatore)
        {
            logger.LogDebug("{Metodo} - Codice anagrafica operatore: {CodiceAnagraficaOperatore} Richiesta: {Richiesta}",
                MethodBase.GetCurrentMethod().Name, configurazioneOperatore.CodiceAnagrafica, JsonSerializer.Serialize(command.Richiesta));

            // Caricamento campionamento da duplicare
            var campionamentoDaDuplicare = repositoryCampionamenti.DettagliCampionamento(command.Richiesta.IdCampionamentoDaDuplicare);

            // Se è un collegamento al precedente campionamento, viene impostato il campo campione collegato
            if (command.Richiesta.CreaCollegamento)
            {
                // Se disponibile, utilizza la sigla verbale
                if (!String.IsNullOrEmpty(campionamentoDaDuplicare.SiglaVerbale))
                {
                    campionamentoDaDuplicare.CampionamentoCollegato = campionamentoDaDuplicare.SiglaVerbale;
                }
                else
                {
                    // Utilizza punto e data
                    // Decide se punto Arpal o operatore
                    string codicePunto;
                    if (campionamentoDaDuplicare.PuntoDiPrelievoARPAL != null)
                        codicePunto = campionamentoDaDuplicare.PuntoDiPrelievoARPAL.Codice;
                    else
                        codicePunto = campionamentoDaDuplicare.PuntoDiPrelievoInseritoDaOperatore.Codice;
                    campionamentoDaDuplicare.CampionamentoCollegato = $"{codicePunto} {campionamentoDaDuplicare.DataPrelievo.ToString("dd/MM/yyyy")}";
                    // Se disponibile, aggiunge ora
                    if (campionamentoDaDuplicare.OraPrelievo.HasValue)
                    {
                        campionamentoDaDuplicare.CampionamentoCollegato += $" {campionamentoDaDuplicare.OraPrelievo.Value.ToString("hh:mm")}";
                    }
                }
            }

            // Sostituzione delle informazioni sul punto di prelievo
            campionamentoDaDuplicare.PuntoDiPrelievoARPAL = command.Richiesta.PuntoDiPrelievoARPAL;
            campionamentoDaDuplicare.PuntoDiPrelievoInseritoDaOperatore = command.Richiesta.PuntoDiPrelievoInseritoDaOperatore;

            // Sostituzione delle informazioni sull'ultimo aggiornamento
            campionamentoDaDuplicare.UltimoAggiornamento = new UltimoAggiornamento
            {
                Operatore = new Operatore
                {
                    Codice = configurazioneOperatore.CodiceAnagrafica,
                    Nome = configurazioneOperatore.Nome,
                    Cognome = configurazioneOperatore.Cognome,
                    CodiceFiscale = configurazioneOperatore.CodiceFiscale
                },
                DataOra = DateTimeOffset.Now
            };

            campionamentoDaDuplicare.DataOraCreazioneCampionamento = DateTimeOffset.Now;
            campionamentoDaDuplicare.CodiceFiscaleOperatoreCreazioneCampionamento = configurazioneOperatore.CodiceFiscale;
            campionamentoDaDuplicare.TipoCampionamento = command.Richiesta.TipoCampionamento;

            // Se viene indicata una nuova data prelievo, la imposta
            if (command.Richiesta.NuovaDataPrelievo.HasValue)
            {
                campionamentoDaDuplicare.DataPrelievo = command.Richiesta.NuovaDataPrelievo.Value;
            }

            // Svuota altri campi
            campionamentoDaDuplicare.OraPrelievo = null;
            campionamentoDaDuplicare.DataOraChiusuraCampionamento = null;
            campionamentoDaDuplicare.NotaAccettazione = null;
            campionamentoDaDuplicare.NumeroCampione = null;
            campionamentoDaDuplicare.DataAperturaCampione = null;
            campionamentoDaDuplicare.OraAperturaCampione = null;

            int idCampionamento = repositoryCampionamenti.AggiuntaCampionamento(campionamentoDaDuplicare);

            return repositoryCampionamenti.DettagliCampionamento(idCampionamento);
        }
    }
}
