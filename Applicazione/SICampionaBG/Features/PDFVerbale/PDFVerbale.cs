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
using NLog;
using QuestPDF.Fluent;
using SICampionaBG.Data;
using SICampionaBG.Model;
using System.Reflection;
using System.Text;

namespace SICampionaBG.Features.CreazionePDFVerbale
{
    internal class PdfVerbale(Configurazione configurazione, IRepositoryCampionamenti repositoryCampionamenti)
    {
        readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void CreaVerbali()
        {
            logger.Debug(MethodBase.GetCurrentMethod().Name);
            if (!configurazione.CreazionePDFVerbaleAbilitata)
            {
                logger.Info("Creazione PDF verbale disabilitata");
                return;
            }

            var campionamenti = repositoryCampionamenti.CampionamentiChiusiSenzaVerbale();
            if (campionamenti.Count == 0)
            {
                logger.Info("Nessun campionamento da elaborare");
                return;
            }
            foreach (var campionamento in campionamenti)
                CreaPDFVerbaleCampionamento(campionamento, configurazione, repositoryCampionamenti);

            logger.Debug("Creazione PDF verbali completata");
        }

        private void CreaPDFVerbaleCampionamento(Campionamento campionamento, Configurazione configurazione, IRepositoryCampionamenti repositoryCampionamenti)
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name} - IdCampionamento: {campionamento.IdCampionamento}");

            try
            {
                // Verifica che sigla verbale sia presente
                if (string.IsNullOrEmpty(campionamento.SiglaVerbale))
                {
                    logger.Error($"Campionamento {campionamento.IdCampionamento} senza sigla verbale - PDF non creato");
                    return;
                }
                // Verifica che il suffisso della cartella sia presente
                if (string.IsNullOrEmpty(campionamento.SuffissoCartella))
                {
                    logger.Error($"Campionamento {campionamento.IdCampionamento} senza suffisso cartella");
                    return;
                }

                var cartellaFilePDF = Path.Combine(configurazione.CartellaRadiceCampionamenti,
                    $"{campionamento.IdCampionamento}_{campionamento.SuffissoCartella}");

                // Se la cartella non esiste è creata
                if (!Directory.Exists(cartellaFilePDF))
                {
                    logger.Debug($"La cartella {cartellaFilePDF} non esiste - Creazione");
                    Directory.CreateDirectory(cartellaFilePDF);
                }

                string nomeFileVerbale = NomeFileVerbale(campionamento);
                var filePDF = Path.Combine(cartellaFilePDF, nomeFileVerbale);
                logger.Debug($"File PDF: {filePDF}");

                // Creazione del modello dati da usere per generare il PDF
                logger.Debug("Creazione modello dati");
                var verbaleModel = CreazioneModelloDati(campionamento, repositoryCampionamenti);

                // Generazione del PDF
                logger.Debug("Generazione PDF");
                var document = new VerbaleDocument(verbaleModel);
                document.GeneratePdf(filePDF);

                // Aggiornamento del nome del file verbale nel database
                logger.Debug("Aggiornamento nome file verbale nel database");
                repositoryCampionamenti.AggiornaFileVerbale(campionamento.IdCampionamento, nomeFileVerbale);

                // Aggiornamento dello stato di invio dell'email del verbale nel database (se è presente un indirizzo email)
                logger.Debug("Aggiornamento stato email invio verbale nel database");
                if (!string.IsNullOrEmpty(campionamento.EmailInvioVerbale))
                {
                    repositoryCampionamenti.AggiornaStatoEmailInvioVerbale(campionamento.IdCampionamento, configurazione.StatoEmailInvioVerbale_DaInviare);
                }

                logger.Debug($"Creazione verbale IdCampionamento: {campionamento.IdCampionamento} completata");
            }
            catch (Exception exception)
            {
                logger.Error(exception, Utils.GetExceptionDetails(exception));
            }
        }

        internal static string NomeFileVerbale(Campionamento campionamento)
        {
            // Il nome del file è è la sigla del verbale, esclusi caratteri non validi, con l'estensione PDF
            return $"{Utils.SostituisceCaratteriNonValidiPerNomeFile(campionamento.SiglaVerbale)}.pdf";
        }

        private static VerbaleModel CreazioneModelloDati(Campionamento campionamento, IRepositoryCampionamenti repositoryCampionamenti)
        {
            // L'operatore è il primo prelevatore
            var primoOperatore = campionamento.Prelevatori[0];

            PuntoDiPrelievo puntoDiPrelievo;
            if (campionamento.PuntoDiPrelievoInseritoDaOperatore == null)
                puntoDiPrelievo = campionamento.PuntoDiPrelievoARPAL;
            else
                puntoDiPrelievo = campionamento.PuntoDiPrelievoInseritoDaOperatore;

            var misureInLoco = campionamento.MisureInLoco
                .Select(i => new VerbaleModel.MisuraInLoco()
                {
                    Descrizione = i.Descrizione,
                    Metodo = i.DescrizioneMetodo,
                    ValoreConUM = $"{i.Valore} {i.UnitaMisura}",
                    Note = i.NoteOperatore
                }).ToList();

            var parametri = campionamento.Analiti
                .Where(i => !i.RimossoDaOperatore)
                .Select(i => new VerbaleModel.Parametro()
                {
                    Descrizione = i.Descrizione,
                    Metodo = i.DescrizioneMetodo,
                    UM = i.UnitaMisura,
                    Limite = i.ValoreLimite?.ToString(),
                    Pacchetto = i.DescrizionePacchetto
                }).ToList();

            // Conversione della lista di prelevatori in una stringa
            var sbPrelevatori = new StringBuilder();
            foreach (var prelevatore in campionamento.Prelevatori)
            {
                if (sbPrelevatori.Length > 0)
                    sbPrelevatori.Append(", ");
                sbPrelevatori.Append($"{prelevatore.Cognome} {prelevatore.Nome}");
            }

            // DataOraAperturaCampione è calcolata se sono presenti data e ora apertura campione
            string dataOraAperturaCampione = null;
            if (campionamento.DataAperturaCampione.HasValue && campionamento.OraAperturaCampione.HasValue)
            {
                dataOraAperturaCampione = $"{campionamento.DataAperturaCampione:dd/MM/yyyy} {campionamento.OraAperturaCampione.Value:HH:mm}";
            }

            string argomento = campionamento.Argomento.Denominazione;

            // Nel caso specifico della matrice con codice "MTX0000002" (ACQUE D.C.U.- ACQUE DESTINATE AL CONSUMO UMANO)
            // l'argomento è sostituito con "D.Lgs. 18/2023, D.M.S 14/06/2017, D.lgs. 28/2016 e D.M. 02/08/17"
            //
            // Nota: soluzione temporanea in attesa di definizione della personalizzazione verbali
            //
            if (campionamento.Matrice.Codice == "MTX0000002")
                argomento = "D.Lgs. 18/2023, D.M.S 14/06/2017, D.lgs. 28/2016 e D.M. 02/08/17";

            // Carica descrizione e note associate al tipo di campionamento (se ci sono)
            var (Descrizione, Note) = repositoryCampionamenti.CaricaInformazioniTipoCampionamento(campionamento.Matrice.Codice, campionamento.TipoCampionamento);

			return new VerbaleModel
			{
				Matrice = campionamento.Matrice.Denominazione,
				Argomento = argomento,
				Ente = campionamento.Ente.Denominazione,
				SiglaVerbale = campionamento.SiglaVerbale,
				Operatore = $"{primoOperatore.Cognome} {primoOperatore.Nome}",
				DataOraCampionamento = $"{campionamento.DataPrelievo:dd/MM/yyyy} {campionamento.OraPrelievo.Value:HH:mm}",
				PuntoDiPrelievoCodice = puntoDiPrelievo.Codice,
				PuntoDiPrelievoDenominazione = puntoDiPrelievo.Denominazione,
				PuntoDiPrelievoIndirizzo = puntoDiPrelievo.Indirizzo,
				PuntoDiPrelievoComune = puntoDiPrelievo.Comune.Denominazione,
				MisureInLoco = misureInLoco,
				Parametri = parametri,
				DescrizioneAttivita = campionamento.DescrizioneAttivita,
				Prelevatori = sbPrelevatori.ToString(),
				DateOraVerbale = campionamento.DataOraChiusuraCampionamento.Value.ToString("dd/MM/yyyy HH:mm"),
				DateOraAperturaCampione = dataOraAperturaCampione,
				EmailInvioVerbale = campionamento.EmailInvioVerbale,
				PecInvioVerbale = campionamento.PecInvioVerbale,
				CampionamentoCollegato = campionamento.CampionamentoCollegato,
				DescrizioneTipoCampionamento = Descrizione,
				NoteTipoCampionamento = Note,
                CampioneBianco = campionamento.CampioneBianco,
                LuogoAperturaCampione = campionamento.LuogoAperturaCampione,
                Note = campionamento.Note
            };
		}
	}
}
