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
	public interface IAnagrafiche
	{
		public Task<Result<List<Analita>>> Analiti(string codiceMatrice, string codiceArgomento = null, string codicePacchetto = null, string parteDelNome = null, string linea = null);
		public Task<Result<List<PuntoDiPrelievo>>> PuntiDiPrelievo(string codiceIstatComune, string parteDenominazioneCodice);
		public Task<Result<List<ClienteConIndirizzo>>> Clienti(string parteDenominazioneCodice);
		public Task<Result<List<Comune>>> Comuni(string parteDellaDenominazione);
		public Task<Result<List<Pacchetto>>> Pacchetti(string codiceMatrice, string codiceArgomento = null, string codiceSedeAccettazione = null);
		public Task<Result<List<Contenitore>>> Contenitori(string codicePacchetto);
		public Task<Result<List<SedeAccettazione>>> SediAccettazione();
		public Task<Result<List<MatriceNomeDescrizione>>> Matrici();
		public Task<Result<List<string>>> TipiCampionamento(string codiceMatrice);
	}
}
