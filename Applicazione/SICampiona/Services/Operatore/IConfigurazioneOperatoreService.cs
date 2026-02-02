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
	public interface IConfigurazioneOperatoreService
	{
		public string CodiceFiscale();
		public Task<Result> ScaricaConfigurazioneDaServer();
		public Task CaricaConfigurazioneDaLocalStorage();
		public string NomeCognome();
		public List<ConfigurazioneOperatore.Ente> Enti();
		public List<ConfigurazioneOperatore.Ente.Matrice.Argomento> Argomenti(string codiceEnte, string codiceMatrice);
		public List<ConfigurazioneOperatore.Ente.Matrice> Matrici(string codiceEnte);
		public bool ConfigurazioneDisponibile { get; }
		public void CancellaConfigurazione();
		public string AccessToken { get; set; }
		public bool Autorizzato { get; set; }
		public bool ConfigurazioneSenzaEnti { get; }
		public bool VersioneApplicazioneVerificata { get; set; }
    }
}
