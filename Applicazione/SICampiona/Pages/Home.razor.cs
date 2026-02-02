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
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.FluentUI.AspNetCore.Components;
using SICampiona.Common;
using SICampiona.Model;
using SICampiona.Pages.Dialog;
using System.Reflection;

namespace SICampiona.Pages
{
    public partial class Home
    {
        #region Gestione errore
        private string messaggioErrore = null;
        private string dettagliErrore = null;
        private bool visualizzaComandoRicarica = false;
        private bool visualizzaComandoChiudi = false;
        private bool visualizzaComandoTornaHomePage = false;
        #endregion

        private bool aggiornamentoInCorso = true;
        private bool necessariaSincronizzazione = false;
        private string messaggioAggiornamentoVersione = null;
        private string urlDocumentoAperturaCampioni = null;

        IQueryable<InformazioniCampionamento> campionamenti;

        protected override async Task OnInitializedAsync()
        {
            await AggiornaAccessTokenClientHttp();
            await CaricaConfigurazioneOperatore();
            await VerificaSeNecessariaSincronizzazione();
            await AggiornaElenco();
            await VerificaVersioneRichiesta();
            aggiornamentoInCorso = false;
            urlDocumentoAperturaCampioni = Configuration["UrlDocumentoAperturaCampioni"];
        }

        private async Task VerificaSeNecessariaSincronizzazione()
        {
            var result = await campionamentiStorage.SeModificati();

            if (result.IsSuccess)
            {
                necessariaSincronizzazione = result.Data;
                if (necessariaSincronizzazione)
                    await Sincronizzazione(false);
            }
            else
            {
                messaggioErrore = "Non è stato possibile verificare lo stato di sincronizzazione";
                dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
                visualizzaComandoRicarica = true;
                visualizzaComandoChiudi = false;
                visualizzaComandoTornaHomePage = false;
            }
        }

        #region Comandi

        private void Nuovo()
        {
            Navigation.NavigateTo("nuovo");
        }

        private void Duplica()
        {
            Navigation.NavigateTo("duplica");
        }

        private void Logout()
        {
            Navigation.NavigateToLogout("authentication/logout");
        }

        private void Login()
        {
            Navigation.NavigateToLogout("authentication/login");
        }

        private void ArchivioCompletati()
        {
            Navigation.NavigateTo("archivio");
        }

        private void Pacchetti()
        {
            Navigation.NavigateTo("pacchetti");
        }

        #endregion Comandi

        private async Task Sincronizzazione(bool richiestaOperatore)
        {
            bool sincronizza = true;
            if (richiestaOperatore)
            {
                IDialogReference dialog = await DialogService.ShowDialogAsync<Conferma>("Sincronizzare i dati con il server?", new() { DismissTitle = "Annulla" });
                DialogResult result = await dialog.Result;
                sincronizza = !result.Cancelled;
            }
            if (sincronizza)
            {
                aggiornamentoInCorso = true;
                StateHasChanged();
                var resultSincronizzazione = await campionamentiStorage.Sincronizzazione();
                aggiornamentoInCorso = false;
                if (resultSincronizzazione.IsSuccess)
                {
                    await AggiornaElenco();
                    necessariaSincronizzazione = false;
                }
                else
                {
                    if (richiestaOperatore)
                    {
                        messaggioErrore = "Non è stato possibile sincronizzare i campionamenti";
                        dettagliErrore = $"{resultSincronizzazione.ErrorMessage} {resultSincronizzazione.ErrorDetails}";
                        visualizzaComandoRicarica = false;
                        visualizzaComandoChiudi = true;
                        visualizzaComandoTornaHomePage = false;
                    }
                    else
                    {
                        logger.LogError("Errore sincronizzazione :{ErrorMessage} - {ErrorDetails}", resultSincronizzazione.ErrorMessage, resultSincronizzazione.ErrorDetails);
                    }
                }
            }
        }

        private async Task AggiornaElenco()
        {
            aggiornamentoInCorso = true;

            var result = await campionamentiStorage.Elenco();
            StateHasChanged();

            if (result.IsSuccess)
            {
                campionamenti = result.Data.AsQueryable();
            }
            else
            {
                messaggioErrore = "Non è stato possibile recuperare l'elenco dei campionamenti";
                dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
                visualizzaComandoRicarica = true;
                visualizzaComandoChiudi = false;
                visualizzaComandoTornaHomePage = false;
            }
            aggiornamentoInCorso = false;
        }

        private async Task CaricaConfigurazioneOperatore()
        {
            // Se la configurazione è già disponibile non è necessario fare nulla
            if (infoOperatore.ConfigurazioneDisponibile)
                return;

            aggiornamentoInCorso = true;

            // Se è disponibile il token di accesso, la configurazione operatore è prelevata
            // dal server (e salvata in localstorage)
            if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
            {
                var result = await infoOperatore.ScaricaConfigurazioneDaServer();
                if (!result.IsSuccess)
                {
                    messaggioErrore = "Non è stato possibile caricare la configurazione operatore";
                    dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
                    visualizzaComandoRicarica = true;
                    visualizzaComandoChiudi = false;
                    visualizzaComandoTornaHomePage = false;
                    aggiornamentoInCorso = false;
                    return;
                }
            }

            // Carica la configurazione da local storage
            await infoOperatore.CaricaConfigurazioneDaLocalStorage();

            aggiornamentoInCorso = false;

            // Se la configurazione non è disponibile nel locale storage
            // non è possibile utilizzare l'applicazione e viene visualizzata la pagina di 
            // spiegazione
            if (!infoOperatore.ConfigurazioneDisponibile)
            {
                infoOperatore.Autorizzato = false;
                Navigation.NavigateTo("nessuna-configurazione");
                return;
            }

            // Se la configurazione non contiene enti significa che l'operatore non è autorizzato
            // e viene visualizzata la pagina di spiegazione
            if (infoOperatore.ConfigurazioneSenzaEnti)
            {
                infoOperatore.Autorizzato = false;
                Navigation.NavigateTo("non-autorizzato");
                return;
            }

            infoOperatore.Autorizzato = !string.IsNullOrEmpty(infoOperatore.AccessToken);

            if(infoOperatore.Autorizzato)
            {
                await Sincronizzazione(false);                
            }
        }

        /// <summary>
        /// Verifica se la versione dell'applicazione coincide con quella richiesta dal back end
        /// </summary>
        /// <returns></returns>
        private async Task VerificaVersioneRichiesta()
        {
            try
            {
                if(infoOperatore.VersioneApplicazioneVerificata)
                    return;

                using var client = clientFactory.CreateClient("APISICampiona");
                if (!string.IsNullOrEmpty(infoOperatore.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", infoOperatore.AccessToken);
                else
                    return;

                var endpoint = $"versionefrontend";

                var response = await client.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string versioneCorrente = Utils.Version();
                    string versioneRichiesta = await response.Content.ReadAsStringAsync();
                    logger.LogDebug("Versione corrente: {VersioneCorrente} - Versione richiesta: {VersioneRichiesta}", versioneCorrente, versioneRichiesta);
                    if ((versioneCorrente != versioneRichiesta))
                    {
                        messaggioAggiornamentoVersione = $"E' disponibile una nuova versione dell'applicazione ({versioneRichiesta}). " +
                            "Riavviare per aggiornare o premere CTRL+F5";
                    }
                    else
                        messaggioAggiornamentoVersione = null;
                    infoOperatore.VersioneApplicazioneVerificata = true;
                }
                else
                {
                    logger.LogError("Errore chiamata client http - Risposta: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                }

            }
            catch (Exception ex)
            {
                string dettagliEccezione = Utils.GetExceptionDetails(ex);
                logger.LogError("{Metodo} - Errore: {Errore}", MethodBase.GetCurrentMethod().Name, dettagliEccezione);
            }
        }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationState { get; set; }

        private async Task AggiornaAccessTokenClientHttp()
        {
            var state = await AuthenticationState;
            if (state.User.Identity.IsAuthenticated)
            {
                var accessTokenResult = await AuthorizationService.RequestAccessToken();

                if (!accessTokenResult.TryGetToken(out var token))
                {
                    messaggioErrore = "Non è stato possibile acquisire il token di accesso.";
                    visualizzaComandoRicarica = false;
                    visualizzaComandoChiudi = true;
                    visualizzaComandoTornaHomePage = false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(token.Value))
                    {
                        infoOperatore.AccessToken = token.Value;
                    }
                }
            }
        }
    }
}
