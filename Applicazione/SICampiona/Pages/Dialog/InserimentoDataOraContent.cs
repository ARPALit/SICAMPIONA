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

namespace SICampiona.Pages.Dialog
{
	/// <summary>
	/// Classe per argomento per la finestra di dialogo InserimentoDataOra
	/// </summary>
	public class InserimentoDataOraContent
	{
		public string Etichetta { get; set; }
		public bool VisualizzaData { get; set; } = true;
		public bool VisualizzaOra { get; set; } = true;
		public DateTime? ValoreDataInserito { get; set; }
		public DateTime? ValoreOraInserito { get; set; }
		public bool Obbligatorio { get; set; } = false;
	}
}
