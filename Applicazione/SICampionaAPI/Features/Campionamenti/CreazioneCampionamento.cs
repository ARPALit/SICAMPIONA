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
using System.Text.Json;

namespace SICampionaAPI.Features.Campionamenti
{
    public class CreazioneCampionamento(ILogger<OperatoriController> logger, IRepositoryCampionamenti repositoryCampionamenti, IAnagrafiche anagrafiche)
    {
        public class Command(RichiestaNuovoCampionamento richiesta, string codiceFiscaleOperatore)
        {
            public RichiestaNuovoCampionamento Richiesta { get; } = richiesta;
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
                    return new ResultDto<Result>(new Error($"Errore creazione campionamento: {resultConfigurazioneOperatore.Error.Message}", resultConfigurazioneOperatore.Error.Details));

                // Creazione campionamento				
                Campionamento campionamento = CreaCampionamento(command, resultConfigurazioneOperatore.Data.Configurazione);

                return new ResultDto<Result>(new Result(campionamento));
            }
            catch (Exception ex)
            {
                string errorDetails = Utils.GetExceptionDetails(ex);
                return new ResultDto<Result>(new Error("Errore creazione campionamento", errorDetails, ErrorType.Application));
            }
        }

        public class Result(Campionamento campionamentoCreato)
        {
            public Campionamento CampionmentoCreato { get; } = campionamentoCreato;
        }

        public class RichiestaNuovoCampionamento
        {
            public Ente Ente { get; set; }
            public Matrice Matrice { get; set; }
            public Argomento Argomento { get; set; }
            public Cliente Cliente { get; set; }
            public PuntoDiPrelievo PuntoDiPrelievoARPAL { get; set; }
            public PuntoDiPrelievo PuntoDiPrelievoInseritoDaOperatore { get; set; }
            public DateOnly DataPrelievo { get; set; }
            public List<string> CodiciPacchetto { get; set; }
            public string CodiceSedeAccettazione { get; set; }
            public string CampionamentoCollegato { get; set; }
            public string TipoCampionamento { get; set; }
        }

        /// <summary>
        /// Creazione campionamento con dati della richiesta e dell'operatore
        /// </summary>
        /// <param name="command"></param>
        /// <param name="configurazioneOperatore"></param>
        /// <returns></returns>
        private Campionamento CreaCampionamento(Command command, ConfigurazioneOperatore configurazioneOperatore)
        {
            logger.LogDebug("{Metodo} - Codice anagrafica operatore: {CodiceAnagraficaOperatore} Richiesta: {Richiesta}",
                MethodBase.GetCurrentMethod().Name, configurazioneOperatore.CodiceAnagrafica, JsonSerializer.Serialize(command.Richiesta));

            var campionamento = new Campionamento
            {
                Ente = command.Richiesta.Ente,
                Matrice = command.Richiesta.Matrice,
                Argomento = command.Richiesta.Argomento,
                Cliente = command.Richiesta.Cliente,
                PuntoDiPrelievoARPAL = command.Richiesta.PuntoDiPrelievoARPAL,
                PuntoDiPrelievoInseritoDaOperatore = command.Richiesta.PuntoDiPrelievoInseritoDaOperatore,
                DataOraCreazioneCampionamento = DateTimeOffset.Now,
                DataPrelievo = command.Richiesta.DataPrelievo,
                UltimoAggiornamento = new UltimoAggiornamento
                {
                    Operatore = new Operatore
                    {
                        Codice = configurazioneOperatore.CodiceAnagrafica,
                        Nome = configurazioneOperatore.Nome,
                        Cognome = configurazioneOperatore.Cognome,
                        CodiceFiscale = configurazioneOperatore.CodiceFiscale
                    },
                    DataOra = DateTimeOffset.Now
                },
                Analiti = Analiti(command.Richiesta.Matrice.Codice, command.Richiesta.Argomento.Codice, command.Richiesta.CodiciPacchetto),
                MisureInLoco = anagrafiche.MisureInLoco(command.Richiesta.Matrice.Codice, command.Richiesta.Argomento.Codice, command.Richiesta.CodiciPacchetto),
                PrelevatoriSelezionabili = anagrafiche.Prelevatori(command.Richiesta.Ente.Codice, command.Richiesta.Argomento.Codice),
                CodiceSedeAccettazione = command.Richiesta.CodiceSedeAccettazione,
                CampionamentoCollegato = command.Richiesta.CampionamentoCollegato,
                TipoCampionamento = command.Richiesta.TipoCampionamento
            };

            int idCampionamento = repositoryCampionamenti.AggiuntaCampionamento(campionamento);

            return repositoryCampionamenti.DettagliCampionamento(idCampionamento);
        }

        private List<Analita> Analiti(string codiceMatrice, string codiceArgomento, List<string> codiciPacchetto)
        {
            logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice} - Codice argomento: {CodiceArgomento} Codici pacchetto: {CodiciPacchetto}",
                MethodBase.GetCurrentMethod().Name, codiceMatrice, codiceArgomento, string.Join(",", codiciPacchetto));

            // Contenitori per ogni codice pacchetto
            var contenitori = new Dictionary<string, List<Contenitore>>();
            foreach (string codicePacchetto in codiciPacchetto)
            {
                var contenitoriPacchetto = anagrafiche.Contenitori(codicePacchetto);
                contenitori.Add(codicePacchetto, contenitoriPacchetto);
            }

            var analiti = new List<Analita>();

            // Per ogni codice pacchetto lettura degli analiti dall'anagrafica chiedendo solo la linea "laboratorio"
            foreach (string codicePacchetto in codiciPacchetto)
            {
                var analitiPacchetto = anagrafiche.Analiti(codiceMatrice, codiceArgomento, codicePacchetto, linea: "laboratorio");

                foreach (Analita analita in analitiPacchetto)
                {
                    // Se l'analita non è già presente nella lista analiti
                    // lo aggiunge assegnando i contenitori in base al codice pacchetto
                    if (!analiti.Exists(a => a.Codice == analita.Codice))
                    {
                        analita.Contenitori = contenitori[codicePacchetto];
                        analiti.Add(analita);
                    }
                }
            }

            return analiti;
        }
    }
}
