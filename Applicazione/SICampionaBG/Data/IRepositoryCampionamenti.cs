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
using SICampionaBG.Model;

namespace SICampionaBG.Data
{
	internal interface IRepositoryCampionamenti
	{
		/// <summary>
		/// Elenco dei campionamenti senza verbale
		/// </summary>
		/// <returns></returns>
		List<Campionamento> CampionamentiChiusiSenzaVerbale();

		/// <summary>
		/// Aggiorna il nome del file verbale
		/// </summary>
		/// <param name="idCampionamento"></param>
		/// <param name="nomeFileVerbale"></param>
		void AggiornaFileVerbale(decimal idCampionamento, string nomeFileVerbale);

		/// <summary>
		/// Elenco di campionamenti di cui prelevare il numero campione.
		/// Sono considerati solo i campionamenti chiusi, senza numero campione e per cui il PDF del verbale è stato creato.
		/// </summary>
		/// <returns></returns>
		List<Campionamento> CampionamentiSenzaNumeroCampione();

		/// <summary>
		/// Elenco di campionamenti di cui prelevare lo stato
		/// Sono considerati solo i campionamenti chiusi, con numero campione valorizzato e senza rapporto di prova
		/// (lo stato può cambiare fino alla creazione del rapporto di prova)
		/// </summary>
		/// <returns></returns>
		List<Campionamento> CampionamentiConStatoCampioneDaAggiornare();

		void AggiornaNumeroCampione(decimal idCampionamento, string numeroCampione);

		void AggiornaStatoCampione(decimal idCampionamento, string statoCampione);

		/// <summary>
		/// Elenco campionamenti senza rapporto di prova
		/// Sono considerati solo i campionamenti chiusi, con numero campione valorizzato e senza rapporto di prova
		/// </summary>
		/// <returns></returns>
		List<Campionamento> CampionamentiSenzaRapportoDiProva();

		void AggiornaFileRapportoDiProva(decimal idCampionamento, string nomeFileRapportoDiProva);

		/// <summary>
		/// Carica la descrizione del tipo di campionamento associato alla matrice
		/// </summary>
		/// <param name="codiceMatrice"></param>
		/// <param name="tipoCampionamento"></param>
		/// <returns></returns>
		(string Descrizione, string Note) CaricaInformazioniTipoCampionamento(string codiceMatrice, string tipoCampionamento);

        /// <summary>
        /// Aggiorna lo stato di invio dell'email del verbale
        /// </summary>
        /// <param name="idCampionamento"></param>
        /// <param name="statoEmailInvioVerbale"></param>
        void AggiornaStatoEmailInvioVerbale(decimal idCampionamento, string statoEmailInvioVerbale);

        /// <summary>
        /// Elenco di campionamenti di cui inviare il verbale per email in base al valore del campo STATO_EMAIL_INVIO_VERBALE
        /// </summary>
        /// <returns></returns>
        List<Campionamento> CampionamentiConVerbaleDaInviarePerEmail();

        /// <summary>
        /// Elenco di campionamenti senza temperatura di accettazione
        /// </summary>
        /// <returns></returns>
        List<Campionamento> CampionamentiSenzaTemperaturaAccettazione();
        void AggiornaTemperaturaAccettazioneCampione(decimal idCampionamento, string temperaturaAccettazione);
    }
}
