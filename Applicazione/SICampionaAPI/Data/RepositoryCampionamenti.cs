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

using Microsoft.EntityFrameworkCore;
using SICampionaAPI.Features.Campionamenti;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace SICampionaAPI.Data
{
    public class RepositoryCampionamenti(ILogger<RepositoryCampionamenti> logger) : IRepositoryCampionamenti
    {

        #region Aggionamento campionamento

        public void AggiornamentoCampionamento(Campionamento campionamento)
        {
            logger.LogDebug("{Metodo} - IdCampionamento: {IdCampionamento}", MethodBase.GetCurrentMethod().Name, campionamento.IdCampionamento);

            using var context = new SICampionaContext();
            var campionamentoDB = context.CAMPIONAMENTI
                .Include(i => i.PRELEVATORI)
                .Include(i => i.ANALITI).ThenInclude(i => i.CONTENITORI)
                .Include(i => i.MISURE_IN_LOCO)
                .FirstOrDefault(i => i.ID_CAMPIONAMENTO == campionamento.IdCampionamento);

            if (campionamentoDB == null)
                throw new ArgumentException($"Id campionamento {campionamento.IdCampionamento} non trovato");

            // Nel caso in cui il campionamento è registrato su DB come eliminato non viene aggiornato
            if (campionamentoDB.ELIMINATO)
            {
                logger.LogInformation("{Metodo} - IdCampionamento: {IdCampionamento} - Il campionamento non è aggiornato perché è registrato su DB come eliminato",
                    MethodBase.GetCurrentMethod().Name, campionamento.IdCampionamento);
                return;
            }

            // Non possono essere modificati:
            // Ente
            // Matrice
            // Argomento
            // Data e ora creazione campionamento 
            // Sede accettazione

            // TODO: La nota dell'accettazione sarà aggiornata con un metodo dedicato

            campionamentoDB.CLIENTE_CODICE = campionamento.Cliente.Codice;
            campionamentoDB.CLIENTE_DENOMINAZIONE = campionamento.Cliente.Denominazione;
            campionamentoDB.CLIENTE_CODICE_FISCALE = campionamento.Cliente.CodiceFiscale;
            campionamentoDB.CLIENTE_PARTITA_IVA = campionamento.Cliente.PartitaIVA;

            campionamentoDB.PUNTO_ARPAL_CODICE = campionamento.PuntoDiPrelievoARPAL?.Codice;
            campionamentoDB.PUNTO_ARPAL_DENOMINAZIONE = campionamento.PuntoDiPrelievoARPAL?.Denominazione;
            campionamentoDB.PUNTO_ARPAL_INDIRIZZO = campionamento.PuntoDiPrelievoARPAL?.Indirizzo;
            campionamentoDB.PUNTO_ARPAL_COMUNE_CODICE = campionamento.PuntoDiPrelievoARPAL?.Comune?.CodiceIstat;
            campionamentoDB.PUNTO_ARPAL_COMUNE_DENOMINAZ = campionamento.PuntoDiPrelievoARPAL?.Comune?.Denominazione;
            campionamentoDB.PUNTO_ARPAL_LATITUDINE = campionamento.PuntoDiPrelievoARPAL?.Coordinate?.Latitudine;
            campionamentoDB.PUNTO_ARPAL_LONGITUDINE = campionamento.PuntoDiPrelievoARPAL?.Coordinate?.Longitudine;

            campionamentoDB.PUNTO_OPER_CODICE = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Codice;
            campionamentoDB.PUNTO_OPER_DENOMINAZ = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Denominazione;
            campionamentoDB.PUNTO_OPER_INDIRIZZO = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Indirizzo;
            campionamentoDB.PUNTO_OPER_COMUNE_CODICE = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Comune?.CodiceIstat;
            campionamentoDB.PUNTO_OPER_COMUNE_DENOMINAZ = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Comune?.Denominazione;
            campionamentoDB.PUNTO_OPER_LATITUDINE = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Coordinate?.Latitudine;
            campionamentoDB.PUNTO_OPER_LONGITUDINE = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Coordinate?.Longitudine;

            // Prelevatori
            foreach (var prelevatoreDB in campionamentoDB.PRELEVATORI.ToList())
                context.PRELEVATORI.Remove(prelevatoreDB);
            foreach (var prelevatore in campionamento.Prelevatori)
            {
                var prelevatoreDB = new Prelevatore
                {
                    CODICE = prelevatore.Codice,
                    COGNOME = prelevatore.Cognome,
                    NOME = prelevatore.Nome
                };
                campionamentoDB.PRELEVATORI.Add(prelevatoreDB);
            }

            campionamentoDB.DESCRIZIONE_ATTIVITA = campionamento.DescrizioneAttivita;

            campionamentoDB.DATA_PRELIEVO = campionamento.DataPrelievo.ToDateTime(new TimeOnly());
            if (campionamento.OraPrelievo.HasValue)
                campionamentoDB.ORA_PRELIEVO = campionamento.OraPrelievo.Value.ToString("HH:mm");
            else
                campionamentoDB.ORA_PRELIEVO = null;

            if (campionamento.DataOraChiusuraCampionamento.HasValue)
                campionamentoDB.DATA_ORA_CHIUSURA = campionamento.DataOraChiusuraCampionamento.Value.DateTime;
            else
                campionamentoDB.DATA_ORA_CHIUSURA = null;

            campionamentoDB.ULTIMO_AGG_DATA_ORA = campionamento.UltimoAggiornamento.DataOra.DateTime;


            campionamentoDB.ULTIMO_AGG_OPER_CODICE = campionamento.UltimoAggiornamento.Operatore.Codice;
            campionamentoDB.ULTIMO_AGG_OPER_COGNOME = campionamento.UltimoAggiornamento.Operatore.Cognome;
            campionamentoDB.ULTIMO_AGG_OPER_NOME = campionamento.UltimoAggiornamento.Operatore.Nome;
            campionamentoDB.ULTIMO_AGG_OPER_CF = campionamento.UltimoAggiornamento.Operatore.CodiceFiscale;

            campionamentoDB.SIGLA_VERBALE = campionamento.SiglaVerbale;

            AggiornamentoAnaliti(campionamento, context, campionamentoDB);

            AggiornamentoMisureInLoco(campionamento, context, campionamentoDB);

            campionamentoDB.NUMERO_CAMPIONE = campionamento.NumeroCampione;

            campionamentoDB.ELIMINATO = campionamento.Eliminato;

            campionamentoDB.DATA_APERTURA_CAMPIONE = campionamento.DataAperturaCampione.HasValue ? campionamento.DataAperturaCampione.Value.ToDateTime(new TimeOnly()) : null;
            campionamentoDB.ORA_APERTURA_CAMPIONE = campionamento.OraAperturaCampione.HasValue ? campionamento.OraAperturaCampione.Value.ToString("HH:mm") : null;
            campionamentoDB.EMAIL_INVIO_VERBALE = campionamento.EmailInvioVerbale;
            campionamentoDB.PEC_INVIO_VERBALE = campionamento.PECInvioVerbale;
            campionamentoDB.CAMPIONAMENTO_COLLEGATO = campionamento.CampionamentoCollegato;
            campionamentoDB.CAMPIONE_BIANCO = campionamento.CampioneBianco;
            campionamentoDB.TEMPERATURA_ACCETTAZIONE = campionamento.TemperaturaAccettazione;
            campionamentoDB.LUOGO_APERTURA_CAMPIONE = campionamento.LuogoAperturaCampione;
            campionamentoDB.NOTE = campionamento.Note;

            context.SaveChanges();
        }

        private static void AggiornamentoAnaliti(Campionamento campionamento, SICampionaContext context, CampionamentoDB campionamentoDB)
        {
            foreach (var analitaDB in campionamentoDB.ANALITI.ToList())
            {
                foreach (var contenitoreDB in analitaDB.CONTENITORI.ToList())
                    context.CONTENITORI.Remove(contenitoreDB);
                context.ANALITI.Remove(analitaDB);
            }
            foreach (var analita in campionamento.Analiti)
            {
                var analitaDB = new Analita
                {
                    CODICE = analita.Codice,
                    DESCRIZIONE = analita.Descrizione,
                    METODO_CODICE = analita.CodiceMetodo,
                    METODO_DESCRIZIONE = analita.DescrizioneMetodo,
                    PACCHETTO_CODICE = analita.CodicePacchetto,
                    PACCHETTO_DESCRIZIONE = analita.DescrizionePacchetto,
                    VALORE_LIMITE = analita.ValoreLimite,
                    UNITA_MISURA = analita.UnitaMisura,
                    RIMOSSO_DA_OPER = analita.RimossoDaOperatore,
                    AGGIUNTO_DA_OPER = analita.AggiuntoDaOperatore,
                    ORDINAMENTO = analita.Ordinamento
                };
                foreach (var contenitore in analita.Contenitori)
                {
                    var contenitoreDB = new Contenitore
                    {
                        TIPO = contenitore.Tipo,
                        QUANTITA = contenitore.Quantita
                    };
                    analitaDB.CONTENITORI.Add(contenitoreDB);
                }

                campionamentoDB.ANALITI.Add(analitaDB);
            }
        }

        private static void AggiornamentoMisureInLoco(Campionamento campionamento, SICampionaContext context, CampionamentoDB campionamentoDB)
        {
            foreach (var misuraInLocoDB in campionamentoDB.MISURE_IN_LOCO.ToList())
                context.MISURE_IN_LOCO.Remove(misuraInLocoDB);
            foreach (var misuraInLoco in campionamento.MisureInLoco)
            {
                var misuraInLocoDB = new MisuraInLoco
                {
                    CODICE = misuraInLoco.Codice,
                    DESCRIZIONE = misuraInLoco.Descrizione,
                    METODO_CODICE = misuraInLoco.CodiceMetodo,
                    METODO_DESCRIZIONE = misuraInLoco.DescrizioneMetodo,
                    VALORE = misuraInLoco.Valore,
                    ANNOTAZIONI_OPERATORE = misuraInLoco.NoteOperatore,
                    UNITA_MISURA = misuraInLoco.UnitaMisura
                };
                campionamentoDB.MISURE_IN_LOCO.Add(misuraInLocoDB);
            }
        }

        #endregion Aggionamento campionamento

        public int AggiuntaCampionamento(Campionamento campionamento)
        {
            // Creazione di un oggetto CampionamentoDB
            var campionamentoDB = new CampionamentoDB
            {
                ENTE_CODICE = campionamento.Ente.Codice,
                ENTE_DENOMINAZIONE = campionamento.Ente.Denominazione,
                MATRICE_CODICE = campionamento.Matrice.Codice,
                MATRICE_DENOMINAZIONE = campionamento.Matrice.Denominazione,
                ARGOMENTO_CODICE = campionamento.Argomento.Codice,
                ARGOMENTO_DENOMINAZIONE = campionamento.Argomento.Denominazione,
                CLIENTE_CODICE = campionamento.Cliente.Codice,
                CLIENTE_DENOMINAZIONE = campionamento.Cliente.Denominazione,
                CLIENTE_CODICE_FISCALE = campionamento.Cliente.CodiceFiscale,
                CLIENTE_PARTITA_IVA = campionamento.Cliente.PartitaIVA,
                PUNTO_ARPAL_CODICE = campionamento.PuntoDiPrelievoARPAL?.Codice,
                PUNTO_ARPAL_DENOMINAZIONE = campionamento.PuntoDiPrelievoARPAL?.Denominazione,
                PUNTO_ARPAL_INDIRIZZO = campionamento.PuntoDiPrelievoARPAL?.Indirizzo,
                PUNTO_ARPAL_COMUNE_CODICE = campionamento.PuntoDiPrelievoARPAL?.Comune?.CodiceIstat,
                PUNTO_ARPAL_COMUNE_DENOMINAZ = campionamento.PuntoDiPrelievoARPAL?.Comune?.Denominazione,
                PUNTO_ARPAL_LATITUDINE = campionamento.PuntoDiPrelievoARPAL?.Coordinate?.Latitudine,
                PUNTO_ARPAL_LONGITUDINE = campionamento.PuntoDiPrelievoARPAL?.Coordinate?.Longitudine,
                PUNTO_OPER_CODICE = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Codice,
                PUNTO_OPER_DENOMINAZ = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Denominazione,
                PUNTO_OPER_INDIRIZZO = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Indirizzo,
                PUNTO_OPER_COMUNE_CODICE = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Comune?.CodiceIstat,
                PUNTO_OPER_COMUNE_DENOMINAZ = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Comune?.Denominazione,
                PUNTO_OPER_LATITUDINE = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Coordinate?.Latitudine,
                PUNTO_OPER_LONGITUDINE = campionamento.PuntoDiPrelievoInseritoDaOperatore?.Coordinate?.Longitudine,
                DESCRIZIONE_ATTIVITA = campionamento.DescrizioneAttivita,
                DATA_ORA_CREAZIONE = campionamento.DataOraCreazioneCampionamento.DateTime,
                DATA_PRELIEVO = campionamento.DataPrelievo.ToDateTime(new TimeOnly()),
                ORA_PRELIEVO = campionamento.OraPrelievo.HasValue ? campionamento.OraPrelievo.Value.ToString("HH:mm") : null,
                EMAIL_INVIO_VERBALE = campionamento.EmailInvioVerbale,
                PEC_INVIO_VERBALE = campionamento.PECInvioVerbale,
                CAMPIONAMENTO_COLLEGATO = campionamento.CampionamentoCollegato,
                ULTIMO_AGG_DATA_ORA = campionamento.UltimoAggiornamento.DataOra.DateTime,
                ULTIMO_AGG_OPER_CODICE = campionamento.UltimoAggiornamento.Operatore.Codice,
                ULTIMO_AGG_OPER_COGNOME = campionamento.UltimoAggiornamento.Operatore.Cognome,
                ULTIMO_AGG_OPER_NOME = campionamento.UltimoAggiornamento.Operatore.Nome,
                ULTIMO_AGG_OPER_CF = campionamento.UltimoAggiornamento.Operatore.CodiceFiscale,
                SUFFISSO_CARTELLA = Guid.NewGuid().ToString().ToUpper(),
                CF_OPERATORE_CREAZIONE = campionamento.UltimoAggiornamento.Operatore.CodiceFiscale,
                CODICE_SEDE_ACCETTAZIONE = campionamento.CodiceSedeAccettazione,
                TIPO_CAMPIONAMENTO = campionamento.TipoCampionamento,
                CAMPIONE_BIANCO = campionamento.CampioneBianco,
                TEMPERATURA_ACCETTAZIONE = campionamento.TemperaturaAccettazione,
                LUOGO_APERTURA_CAMPIONE = campionamento.LuogoAperturaCampione,
                NOTE = campionamento.Note,
                ANALITI = campionamento.Analiti.Select(analita => new Analita
                {
                    CODICE = analita.Codice,
                    DESCRIZIONE = analita.Descrizione,
                    METODO_CODICE = analita.CodiceMetodo,
                    METODO_DESCRIZIONE = analita.DescrizioneMetodo,
                    PACCHETTO_CODICE = analita.CodicePacchetto,
                    PACCHETTO_DESCRIZIONE = analita.DescrizionePacchetto,
                    VALORE_LIMITE = analita.ValoreLimite,
                    UNITA_MISURA = analita.UnitaMisura,
                    RIMOSSO_DA_OPER = analita.RimossoDaOperatore,
                    AGGIUNTO_DA_OPER = analita.AggiuntoDaOperatore,
                    CONTENITORI = analita.Contenitori.Select(contenitore => new Contenitore
                    {
                        TIPO = contenitore.Tipo,
                        QUANTITA = contenitore.Quantita
                    }).ToList(),
                    ORDINAMENTO = analita.Ordinamento
                }).ToList(),
                MISURE_IN_LOCO = campionamento.MisureInLoco.Select(misuraInLoco => new MisuraInLoco
                {
                    CODICE = misuraInLoco.Codice,
                    DESCRIZIONE = misuraInLoco.Descrizione,
                    METODO_CODICE = misuraInLoco.CodiceMetodo,
                    METODO_DESCRIZIONE = misuraInLoco.DescrizioneMetodo,
                    VALORE = misuraInLoco.Valore,
                    ANNOTAZIONI_OPERATORE = misuraInLoco.NoteOperatore,
                    UNITA_MISURA = misuraInLoco.UnitaMisura
                }).ToList(),
                PRELEVATORI_SELEZIONABILI = campionamento.PrelevatoriSelezionabili.Select(prelevatore => new PrelevatoreSelezionabile
                {
                    CODICE = prelevatore.Codice,
                    COGNOME = prelevatore.Cognome,
                    NOME = prelevatore.Nome
                }).ToList()
            };

            using var context = new SICampionaContext();
            context.CAMPIONAMENTI.Add(campionamentoDB);
            context.SaveChanges();
            int idCampionamento = (int)campionamentoDB.ID_CAMPIONAMENTO;

            context.SaveChanges();

            return idCampionamento;
        }

        public Campionamento DettagliCampionamento(int idCampionamento)
        {
            logger.LogDebug("{Metodo} - IdCampionamento: {IdCampionamento}", MethodBase.GetCurrentMethod().Name, idCampionamento);

            using var context = new SICampionaContext();
            var campionamentoDB = context.CAMPIONAMENTI
                .Include(i => i.PRELEVATORI)
                .Include(i => i.ANALITI).ThenInclude(i => i.CONTENITORI)
                .Include(i => i.MISURE_IN_LOCO)
                .Include(i => i.PRELEVATORI_SELEZIONABILI)
                .FirstOrDefault(i => i.ID_CAMPIONAMENTO == idCampionamento && !i.ELIMINATO);

            if (campionamentoDB == null)
                throw new ArgumentException($"Id campionamento {idCampionamento} non trovato");

            Campionamento campionamento = (Campionamento)campionamentoDB;

            return campionamento;
        }

        public List<Campionamento> ElencoCampionamentiAperti(List<string> codiciEnte)
        {
            logger.LogDebug("{Metodo} - Codici ente: {CodiciEnte}", MethodBase.GetCurrentMethod().Name, string.Join(",", codiciEnte));

            using var context = new SICampionaContext();

            // Seleziona campionamenti
            var campionamentiDb = context.CAMPIONAMENTI
                .Where(i => !i.ELIMINATO && string.IsNullOrEmpty(i.NUMERO_CAMPIONE) && codiciEnte.Contains(i.ENTE_CODICE));

            var elencoCampionamenti = new List<Campionamento>();

            // Per ogni campionamento sono caricati i dati correlati con query singole
            // (per evitare che la traduzione della query creata con le clausole Include generici l'estrazione di moltissime righe inutili)
            foreach (CampionamentoDB campionamentoDb in campionamentiDb)
            {
                campionamentoDb.PRELEVATORI = [.. context.PRELEVATORI.Where(i => i.ID_CAMPIONAMENTO == campionamentoDb.ID_CAMPIONAMENTO)];
                campionamentoDb.ANALITI = [.. context.ANALITI.Include(i => i.CONTENITORI).Where(i => i.ID_CAMPIONAMENTO == campionamentoDb.ID_CAMPIONAMENTO)];
                campionamentoDb.MISURE_IN_LOCO = [.. context.MISURE_IN_LOCO.Where(i => i.ID_CAMPIONAMENTO == campionamentoDb.ID_CAMPIONAMENTO)];
                campionamentoDb.PRELEVATORI_SELEZIONABILI = [.. context.PRELEVATORI_SELEZIONABILI.Where(i => i.ID_CAMPIONAMENTO == campionamentoDb.ID_CAMPIONAMENTO)];

                elencoCampionamenti.Add((Campionamento)campionamentoDb);
            }

            return elencoCampionamenti;
        }

        public List<ElencoCampionamentiCompletati.InfoCampionamentoCompletato> ElencoCampionamentiCompletati(ElencoCampionamentiCompletati.RichiestaCampionamentiCompletati richiesta)
        {
            logger.LogDebug("{Metodo} - Richiesta: {Richiesta}", MethodBase.GetCurrentMethod().Name, JsonSerializer.Serialize(richiesta));

            var campionamentiCompletati = new List<ElencoCampionamentiCompletati.InfoCampionamentoCompletato>();
            using var context = new SICampionaContext();
            var campionamentiDB = context.CAMPIONAMENTI
                .Where(i => i.ENTE_CODICE == richiesta.CodiceEnte)
                .Where(i => !string.IsNullOrEmpty(i.NUMERO_CAMPIONE))
                .Where(i => !i.ELIMINATO);

            if (richiesta.DataCampionamento.HasValue)
            {
                DateTime dataCampionamento = richiesta.DataCampionamento.Value.ToDateTime(new TimeOnly());
                campionamentiDB = campionamentiDB.Where(i => i.DATA_PRELIEVO == dataCampionamento);
            }

            if (!string.IsNullOrWhiteSpace(richiesta.NumeroCampione))
                campionamentiDB = campionamentiDB.Where(i => i.NUMERO_CAMPIONE != null && i.NUMERO_CAMPIONE.ToUpper().Contains(richiesta.NumeroCampione.ToUpper()));

            if (!string.IsNullOrWhiteSpace(richiesta.SiglaVerbale))
                campionamentiDB = campionamentiDB.Where(i => i.SIGLA_VERBALE != null && i.SIGLA_VERBALE.ToUpper().Contains(richiesta.SiglaVerbale.ToUpper()));

            if (!string.IsNullOrWhiteSpace(richiesta.CodiceCliente))
                campionamentiDB = campionamentiDB.Where(i => i.CLIENTE_CODICE == richiesta.CodiceCliente);

            if (richiesta.NumeroMassimoRisultati != null)
            {
                // Prende solo i primi risultati
                campionamentiDB = campionamentiDB.OrderByDescending(i => i.DATA_PRELIEVO).Take(richiesta.NumeroMassimoRisultati.Value);
            }
            else
            {
                // Prende tutti i risultati
                campionamentiDB = campionamentiDB.OrderByDescending(i => i.DATA_PRELIEVO);
            }

            foreach (var campionamentoDB in campionamentiDB)
            {
                var campionamentoCompletato = new ElencoCampionamentiCompletati.InfoCampionamentoCompletato()
                {
                    IdCampionamento = (int)campionamentoDB.ID_CAMPIONAMENTO,
                    CodiceEnte = campionamentoDB.ENTE_CODICE,
                    DenominazioneEnte = campionamentoDB.ENTE_DENOMINAZIONE,
                    CodiceMatrice = campionamentoDB.MATRICE_CODICE,
					DenominazioneMatrice = campionamentoDB.MATRICE_DENOMINAZIONE,
					DenominazioneArgomento = campionamentoDB.ARGOMENTO_DENOMINAZIONE,
					CodiceCliente = campionamentoDB.CLIENTE_CODICE,
                    DenominazioneCliente = campionamentoDB.CLIENTE_DENOMINAZIONE,
                    DataPrelievo = DateOnly.FromDateTime(campionamentoDB.DATA_PRELIEVO),
                    OraPrelievo = !string.IsNullOrEmpty(campionamentoDB.ORA_PRELIEVO) ? TimeOnly.ParseExact(campionamentoDB.ORA_PRELIEVO, "HH:mm", CultureInfo.InvariantCulture) : null,
                    SiglaVerbale = campionamentoDB.SIGLA_VERBALE,
                    NumeroCampione = campionamentoDB.NUMERO_CAMPIONE,
                    FileVerbale = campionamentoDB.FILE_VERBALE,
                    FileRapportoDiProva = campionamentoDB.FILE_RAPPORTO_DI_PROVA,
                    StatoCampione = campionamentoDB.STATO_CAMPIONE,
                    CartellaFile = $"{campionamentoDB.ID_CAMPIONAMENTO}_{campionamentoDB.SUFFISSO_CARTELLA}",
                    FileVerbaleFirmato = campionamentoDB.FILE_VERBALE_FIRMATO,
                    TemperaturaAccettazione = campionamentoDB.TEMPERATURA_ACCETTAZIONE
                };

                campionamentiCompletati.Add(campionamentoCompletato);
            }

            return campionamentiCompletati;
        }

        public List<Campionamento> ElencoCampionamentiSenzaNumeroCampione()
        {
            logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

            using var context = new SICampionaContext();
            var campionamenti = context.CAMPIONAMENTI
                .Include(i => i.PRELEVATORI)
                .Include(i => i.ANALITI).ThenInclude(i => i.CONTENITORI)
                .Include(i => i.MISURE_IN_LOCO)
                .Where(i => i.DATA_ORA_CHIUSURA.HasValue)
                .Where(i => i.FILE_VERBALE != null)
                .Where(i => i.NUMERO_CAMPIONE == null)
                .Where(i => !i.ELIMINATO)
                .Select(i => (Campionamento)i)
                .ToList();

            return campionamenti;
        }

        public List<Cliente> ClientiCampionamentiCompletati(string codiceEnte)
        {
            logger.LogDebug("{Metodo} - Codice ente: {CodiceEnte}", MethodBase.GetCurrentMethod().Name, codiceEnte);

            using var context = new SICampionaContext();
            return context.CAMPIONAMENTI
                .Where(i => i.ENTE_CODICE == codiceEnte)
                .Where(i => i.DATA_ORA_CHIUSURA.HasValue)
                .Where(i => !i.ELIMINATO)
                .Select(i => new Cliente
                {
                    Codice = i.CLIENTE_CODICE,
                    Denominazione = i.CLIENTE_DENOMINAZIONE,
                    CodiceFiscale = i.CLIENTE_CODICE_FISCALE,
                    PartitaIVA = i.CLIENTE_PARTITA_IVA
                })
                .Distinct()
                .OrderBy(i => i.Denominazione)
                .ToList();
        }

        public List<string> ElencoTipiCampionamento(string codiceMatrice)
        {
            logger.LogDebug("{Metodo} - Codice matrice: {CodiceMatrice}", MethodBase.GetCurrentMethod().Name, codiceMatrice);

            using var context = new SICampionaContext();
            return context.TIPI_CAMPIONAMENTO
                .Where(i => i.MATRICE_CODICE == codiceMatrice)
                .Select(i => i.TIPO_CAMPIONAMENTO)
                .OrderBy(i => i)
                .ToList();
        }

        public void SalvaNomeFileVerbaleFirmato(int idCampionamento, string nomeFileFirmato)
        {
            logger.LogDebug("{Metodo} - IdCampionamento: {IdCampionamento} - NomeFileFirmato: {NomeFileFirmato}",
                MethodBase.GetCurrentMethod().Name, idCampionamento, nomeFileFirmato);

            using var context = new SICampionaContext();
            var campionamentoDB = context.CAMPIONAMENTI
                .FirstOrDefault(i => i.ID_CAMPIONAMENTO == idCampionamento && !i.ELIMINATO);

            if (campionamentoDB == null)
                throw new ArgumentException($"Id campionamento {idCampionamento} non trovato");

            campionamentoDB.FILE_VERBALE_FIRMATO = nomeFileFirmato;
            context.SaveChanges();
        }
    }
}
