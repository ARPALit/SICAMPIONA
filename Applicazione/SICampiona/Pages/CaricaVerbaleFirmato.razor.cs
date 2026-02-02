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

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.FluentUI.AspNetCore.Components;
using SICampiona.Common;
using SICampiona.Model;
using SICampiona.Pages.Dialog;
using SICampiona.Services;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace SICampiona.Pages
{
    public partial class CaricaVerbaleFirmato
    {
        [Parameter]
        public int IdCampionamento { get; set; }

        private Campionamento campionamentoDaStorageLocale = null;

        private InfoCampionamentoCompletato infoCampionamentoCompletato = null;

        bool seFileValido = false;

        readonly int dimensioneMassimaFileInMB = 10;

        string informazioniFile = string.Empty;

        string messaggioFileNonValido = string.Empty;

        FluentInputFileEventArgs fileCaricato = null;

        bool seFileCaricato = false;

        bool seErroreCaricamentoFile = false;

        string erroreCaricamentoFile = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            var result = await campionamentiStorage.Dettagli(IdCampionamento);
            if (result.IsSuccess)
            {
                campionamentoDaStorageLocale = result.Data;
            }
            else
            {
                // Se i dati del campionamento non sono nello storage dei campionamenti
                // si cerca nel local storage i dati di un campionamento completato
                infoCampionamentoCompletato = await localStorage.GetItemAsync<InfoCampionamentoCompletato>("InfoCampionamentoCompletatoPerCaricamentoVerbale");
                if (infoCampionamentoCompletato != null && infoCampionamentoCompletato.IdCampionamento == IdCampionamento)
                {
                    IdCampionamento = infoCampionamentoCompletato.IdCampionamento;
                }
            }
        }

        private void Chiudi()
        {
            CancellaFileTemporanei();
            Navigation.NavigateTo("/");
        }

        void SelezioneFileOnCompleted(IEnumerable<FluentInputFileEventArgs> files)
        {
            // Se è stato caricato un file, verifica che sia valido
            fileCaricato = files.FirstOrDefault();

            seFileValido = false;

            if (fileCaricato == null)
            {
                return;
            }

            messaggioFileNonValido = string.Empty;

            // Verifica che l'estensione del file sia "PDF"
            if (!fileCaricato.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                messaggioFileNonValido = "Estensione file non valida (deve essere PDF)";
                return;
            }

            // Verifica che la dimensione del file non superi la dimensione massima consentita
            decimal dimensioneFileCaricatoInMB = Decimal.Divide(fileCaricato.Size, 1024 * 1024);
            informazioniFile = $"File selezionato: {fileCaricato.Name} ({dimensioneFileCaricatoInMB:0.0} MB)";
            if (dimensioneFileCaricatoInMB > dimensioneMassimaFileInMB)
            {
                messaggioFileNonValido = $"Dimensione file superiore alla massima consentita ({dimensioneMassimaFileInMB} MB)";
                return;
            }

            // Verifica che non ci siano stati errori durante il caricamento del file
            if (!string.IsNullOrEmpty(fileCaricato.ErrorMessage))
            {
                messaggioFileNonValido = $"Errore nel caricamento del file: {fileCaricato.ErrorMessage}";
                return;
            }

            seFileValido = true;
        }

        void CancellaFileTemporanei()
        {
            if (fileCaricato != null)
            {
                fileCaricato.LocalFile?.Delete();
            }
        }

        async Task RichiestaConfermaCaricaFile()
        {
            IDialogReference dialog = await DialogService.ShowDialogAsync<Conferma>($"Caricare il file '{fileCaricato?.Name}'?", new() { DismissTitle = "Annulla" });
            DialogResult result = await dialog.Result;
            if (!result.Cancelled)
            {
                await CaricaFile();
            }
        }

        async Task CaricaFile()
        {
            seFileCaricato = true;
            try
            {
                using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);

                var endpoint = $"campionamenti/uploadverbalefirmato/{IdCampionamento}";

                var content = new MultipartFormDataContent();
                using var fileStream = fileCaricato.LocalFile.OpenRead();

                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                content.Add(new StreamContent(fileStream, Convert.ToInt32(fileStream.Length)), "File", fileCaricato.Name);

                var response = await client.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseContent);
                    if (apiResponse.Success)
                    {
                        // Se il campionamento è stato letto dallo storage dei campionamenti, lo segna come modificato
                        if (campionamentoDaStorageLocale != null)
                        {
                            var result = await campionamentiStorage.SegnaComeModificato(IdCampionamento);
                            if (result.IsSuccess)
                            {
                                seErroreCaricamentoFile = false;
                            }
                            else
                            {
                                seErroreCaricamentoFile = true;
                                erroreCaricamentoFile = result.ErrorMessage;
                            }
                        }
                    }
                    else
                    {
                        seErroreCaricamentoFile = true;
                        logger.LogError("Errore chiamata API - Errore: {ErrorMessage} - Dettagli: {ErrorDetails}", apiResponse.ErrorMessage, apiResponse.ErrorDetails);
                        erroreCaricamentoFile = $"Errore chiamata API - Errore: {apiResponse.ErrorMessage} - Dettagli: {apiResponse.ErrorDetails}";
                    }
                }
                else
                {
                    seErroreCaricamentoFile = true;
                    logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                }
                CancellaFileTemporanei();
            }
            catch (Exception ex)
            {
                string dettagliEccezione = Utils.GetExceptionDetails(ex);
                logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
            }
        }
    }
}
