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

namespace SICampiona.Services
{
	public class Result<T>(T data, bool isSuccess, string errorMessage, string errorDetails)
	{
		public bool IsSuccess { get; set; } = isSuccess;
		public T Data { get; set; } = data;
		public string ErrorMessage { get; set; } = errorMessage;
		public string ErrorDetails { get; set; } = errorDetails;
	}

	public class Result(bool isSuccess, string errorMessage, string errorDetails)
	{
		public bool IsSuccess { get; set; } = isSuccess;
		public string ErrorMessage { get; set; } = errorMessage;
		public string ErrorDetails { get; set; } = errorDetails;
	}

	public static class ResultFactory
	{
		public static Result<DataType> Success<DataType>(DataType data) => new(data, true, null, null);
		public static Result Success() => new(true, null, null);
		public static Result<DataType> Failure<DataType>(string errorMessage, string errorDetails = null) => new(default, false, errorMessage, errorDetails);		
		public static Result Failure(string errorMessage, string errorDetails = null) => new(false, errorMessage, errorDetails);
	}
}
