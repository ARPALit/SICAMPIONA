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

using SICampiona.Model;

namespace SICampiona.Services
{
	public class RichiestaDuplicazioneCampionamentoCompletato
	{
		public int IdCampionamentoDaDuplicare { get; set; }
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
}
