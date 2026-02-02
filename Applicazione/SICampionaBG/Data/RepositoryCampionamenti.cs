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
using NLog;
using SICampionaBG.Features.Campione;
using SICampionaBG.Model;
using System.Reflection;

namespace SICampionaBG.Data
{
    internal class RepositoryCampionamenti(Configurazione configurazione) : IRepositoryCampionamenti
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void AggiornaFileVerbale(decimal idCampionamento, string nomeFileVerbale)
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name} - Id campionamento: {idCampionamento} Nome file verbale: {nomeFileVerbale}");
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            var campionamento = context.CAMPIONAMENTI.Find(idCampionamento) ?? throw new ArgumentException($"Id campionamento {idCampionamento} non trovato");
            campionamento.FILE_VERBALE = nomeFileVerbale;
            context.SaveChanges();
        }

        public void AggiornaNumeroCampione(decimal idCampionamento, string numeroCampione)
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name} - Id campionamento: {idCampionamento} Numero campione: {numeroCampione}");
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            var campionamento = context.CAMPIONAMENTI.Find(idCampionamento) ?? throw new ArgumentException($"Id campionamento {idCampionamento} non trovato");
            campionamento.NUMERO_CAMPIONE = numeroCampione;
            context.SaveChanges();
        }

        public void AggiornaStatoCampione(decimal idCampionamento, string statoCampione)
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name} - Id campionamento: {idCampionamento} Stato campione: {statoCampione}");
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            var campionamento = context.CAMPIONAMENTI.Find(idCampionamento) ?? throw new ArgumentException($"Id campionamento {idCampionamento} non trovato");
            campionamento.STATO_CAMPIONE = statoCampione;
            context.SaveChanges();
        }

        public List<Campionamento> CampionamentiSenzaNumeroCampione()
        {
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            return context.CAMPIONAMENTI
                .Where(i => !i.ELIMINATO)
                .Where(i => i.DATA_ORA_CHIUSURA.HasValue)
                .Where(i => i.FILE_VERBALE != null)
                .Where(i => i.NUMERO_CAMPIONE == null)
                .Select(i => (Campionamento)i)
                .ToList();
        }

        public List<Campionamento> CampionamentiConStatoCampioneDaAggiornare()
        {
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            return context.CAMPIONAMENTI
                .Where(i => !i.ELIMINATO)
                .Where(i => i.DATA_ORA_CHIUSURA.HasValue)
                .Where(i => i.FILE_RAPPORTO_DI_PROVA == null)
                .Where(i => i.NUMERO_CAMPIONE != null)
                .Select(i => (Campionamento)i)
                .ToList();
        }

        public List<Campionamento> CampionamentiChiusiSenzaVerbale()
        {
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);

            return context.CAMPIONAMENTI
                .Include(i => i.PRELEVATORI)
                .Include(i => i.ANALITI).ThenInclude(i => i.CONTENITORI)
                .Include(i => i.MISURE_IN_LOCO)
                .Where(i => !i.ELIMINATO)
                .Where(i => i.DATA_ORA_CHIUSURA.HasValue)
                .Where(i => i.FILE_VERBALE == null)
                .Select(i => (Campionamento)i)
                .ToList();
        }

        public List<Campionamento> CampionamentiSenzaRapportoDiProva()
        {
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            return context.CAMPIONAMENTI
                .Where(i => !i.ELIMINATO)
                .Where(i => i.DATA_ORA_CHIUSURA.HasValue)
                .Where(i => i.FILE_RAPPORTO_DI_PROVA == null)
                .Where(i => i.NUMERO_CAMPIONE != null)
                .Select(i => (Campionamento)i)
                .ToList();
        }

        public void AggiornaFileRapportoDiProva(decimal idCampionamento, string nomeFileRapportoDiProva)
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name} - Id campionamento: {idCampionamento} Nome file rapporto di prova: {nomeFileRapportoDiProva}");
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            var campionamento = context.CAMPIONAMENTI.Find(idCampionamento) ?? throw new ArgumentException($"Id campionamento {idCampionamento} non trovato");
            campionamento.FILE_RAPPORTO_DI_PROVA = nomeFileRapportoDiProva;
            context.SaveChanges();
        }

        public (string Descrizione, string Note) CaricaInformazioniTipoCampionamento(string codiceMatrice, string tipoCampionamento)
        {
            // Se il tipo di campionamento non è specificato non ci sono informazioni da caricare
            if (string.IsNullOrEmpty(tipoCampionamento))
                return (null, null);

            // Altrimenti si cercano la descrizione e le note associate al tipo nella tabella tipi_campionamento
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            var tipoCampionamentoDB = context.TIPI_CAMPIONAMENTO.FirstOrDefault(i => i.MATRICE_CODICE == codiceMatrice && i.TIPO_CAMPIONAMENTO == tipoCampionamento);
            return tipoCampionamentoDB == null
                ? throw new ArgumentException($"Tipo campionamento '{tipoCampionamento}' per la matrice {codiceMatrice} non trovato nella tabella TIPI_CAMPIONAMENTO")
                : (tipoCampionamentoDB.DESCRIZIONE, tipoCampionamentoDB.NOTE);
        }

        public void AggiornaStatoEmailInvioVerbale(decimal idCampionamento, string statoEmailInvioVerbale)
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name} - Id campionamento: {idCampionamento} Stato email invio verbale: {statoEmailInvioVerbale}");
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            var campionamento = context.CAMPIONAMENTI.Find(idCampionamento) ?? throw new ArgumentException($"Id campionamento {idCampionamento} non trovato");
            campionamento.STATO_EMAIL_INVIO_VERBALE = statoEmailInvioVerbale;
            context.SaveChanges();
        }

        public List<Campionamento> CampionamentiConVerbaleDaInviarePerEmail()
        {
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            return context.CAMPIONAMENTI
                .Where(i => !i.ELIMINATO)
                .Where(i => i.STATO_EMAIL_INVIO_VERBALE == configurazione.StatoEmailInvioVerbale_DaInviare)
                .Select(i => (Campionamento)i)
                .ToList();
        }

        public List<Campionamento> CampionamentiSenzaTemperaturaAccettazione()
        {
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            return context.CAMPIONAMENTI
                .Where(i => !i.ELIMINATO)
                .Where(i => i.DATA_ORA_CHIUSURA.HasValue)
                .Where(i => i.NUMERO_CAMPIONE != null)
                .Where(i => i.TEMPERATURA_ACCETTAZIONE == null)
                .Select(i => (Campionamento)i)
                .ToList();
        }

        public void AggiornaTemperaturaAccettazioneCampione(decimal idCampionamento, string temperaturaAccettazione)
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name} - Id campionamento: {idCampionamento} Temperatura accettazione: {temperaturaAccettazione}");
            using var context = new SICampionaContext(configurazione.StringaConnessioneDB);
            var campionamento = context.CAMPIONAMENTI.Find(idCampionamento) ?? throw new ArgumentException($"Id campionamento {idCampionamento} non trovato");
            campionamento.TEMPERATURA_ACCETTAZIONE = temperaturaAccettazione;
            context.SaveChanges();
        }
    }
}
