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
	public class NuovoCampionamento
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
		public string TipoCampionamento { get; set; }
	}
}
