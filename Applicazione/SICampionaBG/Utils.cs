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
using System.Reflection;
using System.Text;

namespace SICampionaBG;
internal static class Utils
{
	/// <summary>
	/// Estrazione di tutti i dettagli di un'eccezione
	/// </summary>
	/// <param name="ex"></param>
	/// <returns></returns>
	public static string GetExceptionDetails(Exception ex)
	{
		var stringBuilder = new StringBuilder();
		while (ex != null)
		{
			stringBuilder.AppendLine(ex.Message);
			if (ex.StackTrace != null)
				stringBuilder.AppendLine(ex.StackTrace);
			ex = ex.InnerException;
		}

		return Environment.NewLine + stringBuilder.ToString();
	}

	/// <summary>
	/// Versione dell'applicazione letta dall'assembly
	/// </summary>
	/// <returns></returns>
	public static string Version()
	{
		Assembly assembly = Assembly.GetEntryAssembly()!;
		AssemblyInformationalVersionAttribute versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!;
		return versionAttribute.InformationalVersion;
	}

	// Funzione che elimina gli spazi in testa e coda di una stringa e restituisce null se la stringa è vuota
	public static string TrimAndNull(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return null;
		else
			return value.Trim();
	}

    /// <summary>
    /// Sostituisce i caratteri non validi per i nomi di file in Windows con "_"
    /// </summary>
    /// <param name="nomeFile"></param>
    /// <returns></returns>
    public static string SostituisceCaratteriNonValidiPerNomeFile(string nomeFile)
    {
        // Ottieni i caratteri non validi per i nomi di file in Windows
        char[] caratteriNonValidi = Path.GetInvalidFileNameChars();

        // Sostituisci i caratteri non validi con "_"
        string nuovoNomeFile = new(nomeFile.Select(c => caratteriNonValidi.Contains(c) ? '_' : c).ToArray());

        return nuovoNomeFile;
    }
}
