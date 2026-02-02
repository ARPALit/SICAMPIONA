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
namespace SICampionaBG.Data;

public partial class MisuraInLoco
{
	public decimal ID_MISURA { get; set; }

	public decimal ID_CAMPIONAMENTO { get; set; }

	public string CODICE { get; set; } = null!;

	public string DESCRIZIONE { get; set; } = null!;

	public string METODO_CODICE { get; set; } = null!;

	public string METODO_DESCRIZIONE { get; set; } = null!;

	public string VALORE { get; set; }

	public string ANNOTAZIONI_OPERATORE { get; set; }

	public string UNITA_MISURA { get; set; }

	public virtual CampionamentoDB ID_CAMPIONAMENTONavigation { get; set; } = null!;
}
