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

namespace SICampionaAPI.Features
{
    public class ResultDto<T>
    {
        public bool Succeeded { get; }
        public bool Failed => !Succeeded;

        public T Data { get; }
        public Error Error { get; }

        internal ResultDto(T data) => (Succeeded, Data) = (true, data);
        internal ResultDto(Error error) => Error = error;
    }
    public class Error(string message, string details = null, ErrorType type = ErrorType.BadRequest)
    {
        public string Message { get; } = message;
        public string Details { get; } = details;
        public ErrorType Type { get; } = type;
    }

    public enum ErrorType
    {
        BadRequest, // Dati della richiesta non validi
        Application // Errore dell'applicazione
    }
}

