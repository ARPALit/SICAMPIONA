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
	public class Pacchetto
	{
		public string Codice { get; set; }
		public string Descrizione { get; set; }
		public string DescrizioneCodice
		{
			get
			{
				// Aggiunta del separatore tra codice e descrizione solo se sono entrambi presenti
				if (string.IsNullOrEmpty(Codice) || string.IsNullOrEmpty(Descrizione))
					return string.Format("{0}{1}", Descrizione, Codice);
				else
					return string.Format("{0} - {1}", Descrizione, Codice);
			}
		}
	}
}
