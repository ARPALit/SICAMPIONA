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
namespace SICampionaAPI.Features.Campionamenti
{
    /// <summary>
    /// Dati di un campionamento
    /// </summary>
    public class Campionamento
    {
        /// <summary>
        /// Identificativo univoco del campionamento
        /// </summary>
        public int IdCampionamento { get; set; }
        /// <summary>
        /// Ente che effettua il campionamento
        /// </summary>
        public Ente Ente { get; set; }
        /// <summary>
        /// Matrice del campionamento
        /// </summary>
        public Matrice Matrice { get; set; }
        /// <summary>
        /// Argomento del campionamento
        /// </summary>
        public Argomento Argomento { get; set; }
        public Cliente Cliente { get; set; }
        /// <summary>
        /// Punto di prelievo definito da ARPAL
        /// </summary>
        public PuntoDiPrelievo PuntoDiPrelievoARPAL { get; set; }
        /// <summary>
        /// Punto di prelievo inserito dall'operatore
        /// </summary>
        public PuntoDiPrelievo PuntoDiPrelievoInseritoDaOperatore { get; set; }
        public List<Prelevatore> Prelevatori { get; set; }
        public List<Prelevatore> PrelevatoriSelezionabili { get; set; }
        public string DescrizioneAttivita { get; set; }
        public DateTimeOffset DataOraCreazioneCampionamento { get; set; }
        public string CodiceFiscaleOperatoreCreazioneCampionamento { get; set; }
        public DateOnly DataPrelievo { get; set; }
        public TimeOnly? OraPrelievo { get; set; }
        public DateTimeOffset? DataOraChiusuraCampionamento { get; set; }
        public UltimoAggiornamento UltimoAggiornamento { get; set; }
        public string SiglaVerbale { get; set; }
        public NotaAccettazione NotaAccettazione { get; set; }
        public List<Analita> Analiti { get; set; }
        public List<MisuraInLoco> MisureInLoco { get; set; }
        public string NumeroCampione { get; set; }
        public bool Eliminato { get; set; }
        public string FileVerbale { get; set; }
        public string FileRapportoDiProva { get; set; }
        public string StatoCampione { get; set; }
        public string UrlPDFVerbale { get; set; }
        /// <summary>
        /// Cartella, posizionata nella cartella File, in cui sono salvati i file PDF del verbale e del rapporto di prova.
        /// </summary>
        public string CartellaFile { get; set; }
        public string CodiceSedeAccettazione { get; set; }
        public DateOnly? DataAperturaCampione { get; set; }
        public TimeOnly? OraAperturaCampione { get; set; }
        public string EmailInvioVerbale { get; set; }
        public string PECInvioVerbale { get; set; }
        public string CampionamentoCollegato { get; set; }
        public string TipoCampionamento { get; set; }
        public string FileVerbaleFirmato { get; set; }
        public string UrlPDFVerbaleFirmato { get; set; }
        public string CampioneBianco { get; set; }
        public string TemperaturaAccettazione { get; set; }
        public string LuogoAperturaCampione { get; set; }
        public string Note { get; set; }
    }

    public class Analita
    {
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string CodiceMetodo { get; set; }
        public string DescrizioneMetodo { get; set; }
        public string CodicePacchetto { get; set; }
        public string DescrizionePacchetto { get; set; }
        public string ValoreLimite { get; set; }
        public string UnitaMisura { get; set; }
        public List<Contenitore> Contenitori { get; set; }
        /// <summary>
        /// Indica che l'operatore ha rimosso l'analita dall'elenco di quelli
        /// per cui è prevista l'analisi in base alla matrice e all'argomento
        /// associati al campionamento
        /// </summary>
        public bool RimossoDaOperatore { get; set; }
        /// <summary>
        /// Indica che l'operatore ha aggiunto l'analita dall'elenco di quelli
        /// per cui è richiesta l'analisi
        /// </summary>
        public bool AggiuntoDaOperatore { get; set; }
        /// <summary>
        /// Ordinamento dell'analita all'interno del pacchetto cui appartiene
        /// </summary>
        public int Ordinamento { get; set; }
    }

    public class Argomento
    {
        public string Codice { get; set; }
        public string Denominazione { get; set; }
    }

    public class Cliente
    {
        public string Codice { get; set; }
        public string Denominazione { get; set; }
        public string CodiceFiscale { get; set; }
        public string PartitaIVA { get; set; }
    }

    public class Comune
    {
        public string CodiceIstat { get; set; }
        public string Denominazione { get; set; }
    }

    public class Contenitore
    {
        public string Tipo { get; set; }
        public int Quantita { get; set; }
    }

    public class Coordinate
    {
        public string Latitudine { get; set; }
        public string Longitudine { get; set; }
    }

    public class Ente
    {
        public string Codice { get; set; }
        public string Denominazione { get; set; }
    }

    public class Matrice
    {
        public string Codice { get; set; }
        public string Denominazione { get; set; }
    }

    public class MisuraInLoco
    {
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string CodiceMetodo { get; set; }
        public string DescrizioneMetodo { get; set; }
        public string UnitaMisura { get; set; }
        public string Valore { get; set; }
        public string NoteOperatore { get; set; }
    }

    public class NotaAccettazione
    {
        public string Testo { get; set; }
        public UltimoAggiornamento UltimoAggiornamento { get; set; }
    }

    public class Operatore
    {
        public string Codice { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string CodiceFiscale { get; set; }
    }

    public class Prelevatore
    {
        public string Codice { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
    }

    public class PuntoDiPrelievo
    {
        public string Codice { get; set; }
        public string Denominazione { get; set; }
        public string Indirizzo { get; set; }
        public Comune Comune { get; set; }
        public Coordinate Coordinate { get; set; }
    }

    public class UltimoAggiornamento
    {
        public DateTimeOffset DataOra { get; set; }
        public Operatore Operatore { get; set; }
    }

    public class Pacchetto
    {
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string Sede { get; set; }
    }
}
