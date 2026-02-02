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
	/// Dati di un campionamento completato
	/// </summary>
	public class InfoCampionamentoCompletato
	{
		public int IdCampionamento { get; set; }
		public string CodiceEnte { get; set; }
		public string DenominazioneEnte { get; set; }
        public string CodiceMatrice { get; set; }
        public string DenominazioneMatrice { get; set; }
		public string DenominazioneArgomento { get; set; }
		public string CodiceCliente { get; set; }
		public string DenominazioneCliente { get; set; }
		public DateOnly DataPrelievo { get; set; }
		public TimeOnly? OraPrelievo { get; set; }
		public string SiglaVerbale { get; set; }
		public string NumeroCampione { get; set; }
		public string UrlPDFVerbale { get; set; }
		public string UrlPDFRapportoDiProva { get; set; }
		public string StatoCampione { get; set; }
		public string FileVerbale { get; set; }
		public string FileRapportoDiProva { get; set; }
        public string UrlPDFVerbaleFirmato { get; set; }
        public string TemperaturaAccettazione { get; set; }		
	}
}
