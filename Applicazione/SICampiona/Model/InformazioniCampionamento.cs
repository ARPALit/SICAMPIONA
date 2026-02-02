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

namespace SICampiona.Model
{
	/// <summary>
	/// Dati minimi di un campionamento
	/// </summary>
	public class InformazioniCampionamento
	{
		/// <summary>
		/// Identificativo univoco del campionamento
		/// </summary>
		public int IdCampionamento { get; set; }
		/// <summary>
		/// Matrice del campionamento
		/// </summary>
		public string Matrice { get; set; }
        /// <summary>
        /// Codice matrice del campionamento
        /// </summary>
        public string CodiceMatrice { get; set; }
        /// <summary>
        /// Argomento del campionamento
        /// </summary>
        public string Argomento { get; set; }
		public string Cliente { get; set; }
		/// <summary>
		/// Dati del punto di prelievo
		/// </summary>
		public string PuntoDiPrelievo { get; set; }
		public string Stato
		{
			get
			{
				if (Eliminato)
					return "Eliminato";

				if (DataOraChiusuraCampionamento.HasValue)
					return "Completato";
				else
					return "Da completare";
			}
		}
		public string UrlPDFVerbale { get; set; }

		/// <summary>
		/// Indica se un campionamento nello storage è stato modificato e non ancora inviato al server
		/// </summary>
		public bool SeModificato { get; set; }
		public DateTimeOffset? DataOraChiusuraCampionamento { get; set; }
		public bool Eliminato { get; set; }

		public DateOnly DataPrelievo { get; set; }
	}
}
