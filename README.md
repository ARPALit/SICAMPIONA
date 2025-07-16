SICampiona

Sommario

[1\. SICampiona](#_Toc198734351)

[2\. Descrizione e finalità del software](#_Toc198734352)

[2.1. Struttura del repository](#_Toc198734353)

[2.2. Finalità del software](#_Toc198734354)

[2.3. Contesto di utilizzo e casi d’uso](#_Toc198734355)

[3\. Architettura del software](#_Toc198734358)

[3.1. Requisiti e dipendenze](#_Toc198734359)

[3.2. Descrizione architettura](#_Toc198734360)

[4\. Riuso ed installazione](#_Toc198734361)

[4.1. Installazione ambiente di sviluppo](#_Toc198734362)

[4.2. Compilazione software](#_Toc198734363)

[4.3. Installazione software](#_Toc198734364)

[5\. Modello dati](#_Toc198734365)

[6\. Descrizione API applicazione back end](#_Toc198734366)

[7\. Dati da servizi esterni](#_Toc198734367)

[Servizio anagrafiche esterne](#_Toc198734368)

[Servizio informazioni sui campioni](#_Toc198734369)

## SICampiona

Sistema per la registrazione dei dati di attività di campionamento

## Descrizione e finalità del software

### Struttura del repository

./base_dati contiene le istruzioni sql per la creazione della struttura del DB

./docs eventuale documentazione aggiuntiva (cartella contenente file integrati nel readme: immagini, diagrammi, ecc.)

./applicazione sorgenti dei moduli dell’applicazione

### Finalità del software

Il sistema SICampiona è stato sviluppato per consentire la memorizzazione delle attività di campionamento svolte da ARPAL o da esterni relativamente a campioni che vengono successivamente consegnati ad ARPAL per svolgere attività di analisi nei laboratori di quest'ultima.

### Contesto di utilizzo e casi d’uso

L'accesso al sistema è possibile mediante l'uso di credenziali digitali (SPID) ed è consentito sia agli operatori ARPAL sia a quegli enti esterni che periodicamente effettuano la raccolta di campioni da analizzare presso i laboratori ARPAL (ad esempio le ASL).

Il sistema si interfaccia con il sistema di gestione del laboratorio di ARPAL per il caricamento automatico delle informazioni di campionamento all'atto della consegna e dell'accettazione del campione in ARPAL.

### Status del progetto

Il sistema è in funzione presso ARPAL nella sua attuale versione 07.2.3 del 30 Maggio 2025

### Copyright e altre informazioni

Il detentore del copyright del sistema è ARPAL

Il sistema è concesso in licenza a norma di: **AGPL versione 3**;

E' possibile utilizzare l'opera unicamente nel rispetto della Licenza.

Una copia della Licenza è disponibile al seguente indirizzo: <https://www.gnu.org/licenses/agpl-3.0.txt>

In caso di riuso, in toto o in parte del sistema SICampiona, è richiesto di notificarne l’adozione in riuso ai seguenti indirizzi:

[sii@arpal.liguria.it](mailto:sii@arpal.liguria.it)

Eventuali segnalazioni inerenti la sicurezza devono essere inviate in modo confidenziale ai suddetti indirizzi.

## Architettura del software

### Requisiti e dipendenze

L'applicazione è composta di più componenti che richiedono:

Server:

- Sistema operativo Microsoft Windows Server 2012 R2 o successivo
- Microsoft IIS versione 8.5 o successiva
- Microsoft .NET 8
- Oracle Database v.11g R2 o successiva
- API per autenticazione e dati di servizio

Client:

- Browser Web
- Accesso con reverse proxy con autenticazione

### Descrizione architettura

L'architettura del sistema si basa su una struttura che integra diverse componenti front-end, back-end e di elaborazione in background per gestire il ciclo di vita dei campionamenti, la generazione dei PDF dei verbali di campionamento, la sincronizzazione dei dati e l'integrazione con sistemi esterni.

#### Applicazione Front-End

Interfaccia utente per l'inserimento, modifica, completamento e consultazione dei campionamenti.

L'applicazione utilizza per l'autenticazione un server di autenticazione OpenID Provider che rilascia un token di autenticazione che viene successivamente letto dall'applicazione.

L'applicazione ottiene il profilo utente con le relative autorizzazioni tramite API.

E’ accessibile agli operatori da qualsiasi dispositivo con un browser.

Funzionamento offline: l’applicazione memorizza temporaneamente i campionamenti in un archivio locale. Permette agli operatori di continuare il lavoro senza connessione grazie all’archivio locale del front-end e sincronizzare automaticamente i dati una volta ripristinata la rete.

Sincronizzazione automatica: si collega periodicamente o su richiesta al back-end per aggiornare o trasferire dati.

Comunicazione: utilizza API esposte dal back-end per inviare e ricevere dati.

#### Applicazione Back-End

Gestione centrale dei dati dei campionamenti. Espone endpoint per integrazioni esterne e fornisce supporto alla sincronizzazione con il front-end.

Espone API per:

- Creazione, aggiornamento e sincronizzazione dei campionamenti.
- Consultazione delle informazioni dei campionamenti da una sorgente dati esterna.
- Pubblicazione dei PDF generati.
- Ricezione e archiviazione dei file caricati.

SI integra con il servizio anagrafiche esterne tramite API RESTful.

#### Applicazione Background

Processo schedulato che si occupa di aggiornamento dei dati senza intervento manuale. Genera i PDF dei verbali e invio le relative email.

Si integra con il servizio informazioni sui campioni tramite API RESTful per l’acquisizione di numeri campione, stati e rapporti di prova.

#### Base dati Oracle

Archivio centrale per i dati strutturati dei campionamenti.

#### Cartella archivio file PDF

Conserva i verbali PDF e i rapporti di prova sul filesystem.

Ogni file è salvato con un prefisso (ID campione) e un suffisso generato casualmente per unicità. Accesso e gestione integrati nel back-end.

#### Servizio anagrafiche esterne

Sorgente di dati esterna all’applicazione utilizzata dall’applicazione tramite API RESTful. Fornisce endpoint per: dati anagrafici per compilazione (matrici, analiti, ecc.), profili operatori.

#### Servizio informazioni sui campioni

Sorgente di dati esterna all’applicazione utilizzata dall’applicazione tramite API RESTful. Fornisce endpoint per lo stato dei campioni.

#### Ciclo di vita di un campionamento

Creazione e modifica: inserimento dati da front-end. Sincronizzazione con il back-end per la registrazione. Alla creazione vengono richiesti alcuni dati obbligatori. La selezione dei pacchetti è effettuata in base a argomento, matrice e sede selezionati.

Completamento: per il completamento sono richiesti ora del prelievo e almeno un prelevatore.

Sincronizzazione: Allineamento periodico tra archivio locale e database centrale tramite API.

Integrazione con servizio informazioni sui campioni: acquisizione di: numero campione, stato campione, PDF rapporto di prova.

Generazione e Invio Verbali: PDF creati da applicazione background. Invio automatico via email agli indirizzi indicati.

#### Flusso comunicazioni tra componenti

Front-End/Back-End: comunicazione bidirezionale per sincronizzazione, caricamento e consultazione.

Back-End/Applicazione Background: operazioni batch per integrazione con sorgente dati esterna.

Back-End/Servizio anagrafiche esterne: comunicazione per dati anagrafici per compilazione (matrici, analiti, ecc.), profili operatori.

Back-End/ Servizio informazioni sui campioni: comunicazione per aggiornamenti sui campioni.

## Riuso ed installazione

### Installazione ambiente di sviluppo

L'ambiente di sviluppo è Microsoft Visual Studio 2022, da installare con strumenti di sviluppo C# e ASP.NET.

<https://visualstudio.microsoft.com/it/>

E' possibile utilizzare anche Microsoft Visual Studio Code

<https://code.visualstudio.com/>

E' richiesta l'installazione di Oracle Developer Tools per Visual Studio

<https://www.oracle.com/database/technologies/developer-tools/visual-studio/>

### Compilazione software

Il software comprende tre progetti di Visual Studio:

- applicazione front end per gli operatori compilabile come applicazione WebAssembly
- applicazione back end che espone API utilizzate dall’applicazione cliente
- applicazione background che acquisisce dati da altri sistemi e produce PDF dei verbali e inviare email con i verbali

I progetti sono compilabili per il rilascio mediante la funzione di Visual Studio (Menu Compilazione->Pubblica)

### Installazione software

#### Creazione database

Creare uno schema Oracle ed eseguire gli script forniti

#### Applicazione front end

Copiare la versione compilata in una cartella sul server Web.

Da interfaccia di IIS creare un application pool con nessun codice gestito.

Creare un sito Web che utilizza l'application pool al punto precedente e che punta alla relativa cartella sul server Web.

Configurare il sito per utilizzare una connessione protetta in https con certificato SSL.

Accedendo con il browser all’URL di pubblicazione, il browser propone l’installazione in locale dell’applicazione.

#### Applicazione back end

Copiare la versione compilata in una cartella sul server Web.

Da interfaccia di IIS creare un application pool versione .NET 4.0.

Creare un sito Web che utilizza l'application pool al punto precedente e che punta alla relativa cartella sul server Web.

Configurare il sito per utilizzare una connessione protetta in https con certificato SSL.

L’account che esegue IIS deve poter accedere in scrittura alle cartelle Log e File.

#### Applicazione background

Copiare la versione compilata in una cartella sul server Web.

Schedulare il processo tramite task scheduler di Windows.

L’account che esegue l’applicazione deve poter accedere in scrittura alle cartella Log.

#### Descrizione file di configurazione

###### Applicazione front end

La configurazione è contenuta nel file appsettings.json

| **Impostazione** | **Descrizione** |
| --- | --- |
| APISICampionaBaseUrl | URL delle API dell’applicazione back end |
| OidcClientId | ID Client per autenticazione in OpenID Connect |
| OidcAuthority | URL Authority OpenID |
| OidcRedirectUri | URL di reindirizzamento dell’autenticazione |
| UrlDocumentoAperturaCampioni | URL di un file di documentazione per l’apertura dei campioni |

###### Applicazione back end

La configurazione è contenuta nel file appsettings.json

| **Impostazione** | **Descrizione** |
| --- | --- |
| SwaggerEnabled | Abilita l'interfaccia Swagger per esplorare le API |
| APIAnagraficheBaseUrl | URL base per l'accesso alle API anagrafiche |
| APIAnagraficheAPIKey | Chiave per l’accesso alle API |
| APIAnagraficheEndpoint\* | Percorsi relativi dei singoli endpoint |
| StringaConnessioneDB | Stringa di connessione per l'accesso al database Oracle |
| APIKeyEndpointSenzaNumeroCampione | API Key dell’endpoint per campionamenti che non hanno il numero campione |
| URLApplicazione | URL principale dell'applicazione |
| VersioneFrontEnd | Versione del front-end utilizzata dall'applicazione |
| URLUserInfoIdentityServer | URL delle informazioni utente dell’OpenID Provider |

###### Applicazione background

| **Impostazione** | **Descrizione** |
| --- | --- |
| StringaConnessioneDB | Stringa di connessione per l'accesso al database Oracle |
| CartellaRadiceCampionamenti | Directory di salvataggio dei file relativi ai campionamenti |
| APICampioniBaseUrl | URL base per l'accesso alle API campionamenti |
| APICampionamentiAPIKey | API key per API campionamenti |
| APICampioni\* | Percorsi relativi ai singoli endpoint per numero campione, stato campione, rapporto di prova |
| CreazionePDFVerbaleAbilitata | Indica se creare il file PDF del verbale |
| LetturaNumeroCampioneAbilitata | Indica se leggere il numero del campione |
| LetturaStatoCampioneAbilitata | Indica se leggere lo stato del campione |
| LetturaRapportoDiProvaAbilitata | Indica se leggere il rapporto di prova |
| InvioEmailVerbaleAbilitato | Se true, invia il verbale per email all’indirizzo indicato nel campionamento |
| EmailSender | Mittente email |
| SMTPServerName | Server SMTP |
| SMTPServerPort | Porta server SMTP |
| SMTPServerEnableSSL | Se true, usa SSL per la connessione al server SMTP |
| SMTPServerUsername | Username per l’autenticazione al server SMTP |
| SMTPServerPassword | Password dello username |
| DestinationEmailOverride | Indirizzo email: se impostato, invia le email a questo indirizzo anziché a quelli inseriti nei campionamenti |

#### Procedura di verifica dell’installazione

Per testare l’installazione, completare un ciclo di campionamento per verificare che tutte le funzionalità (creazione, aggiornamento, sincronizzazione, completamento, generazione e invio verbali) siano operative.

Separatamente verificare che:

- l’applicazione front end sia raggiungibile tramite browser;
- l’applicazione back end sia raggiungibile dall’applicazione front end;
- la base dati sia raggiungibile dall’applicazione back end e dall’applicazione background;
- I servizi esterni (anagrafiche e informazioni sui campioni) siano raggiungibili dall’applicazione back end.

I log degli errori dell’applicazione front end sono mostrati nella console del browser; quelli delle applicazioni back end e background sono archiviati come file nelle rispettive sottocartelle Log. Il servizio usato per la scrittura e archiviazione dei log è NLog le cui impostazioni sono contenute nei file di configurazione delle singole applicazioni.

###### Scenari di test

Utilizzare l’applicazione Web.

##### 1\. Login

- Accedere alla pagina /login
- Effettuare il login con applicazione esterna.
- Verificare che, dopo l'autenticazione, si venga reindirizzati alla pagina principale dell'applicazione contenente l'elenco delle attività di campionamento e, in alto a destra, compaia il nominativo dell'utente.

Gli scenari seguenti assumono che l'utente abbia fatto login nell'applicazione e sia visualizzata la pagina principale.

##### 2\. Dettagli campionamento in sola lettura

- Fare clic sul titolo di uno dei campionamenti accanto cui compare il badge "Completato".
- E' visualizzata la pagina di dettaglio.
- Verificare che siano presenti le informazioni del campionamento senza la possibilità di modifica.

##### 3\. Aggiunta campionamento

- Fare clic sul pulsante "Nuovo".
- Inserire i dati del campionamento (tutti obbligatori), selezionare la sede di accettazione e quindi i pacchetti da includere.
- Fare clic sul pulsante "Crea".
- Dopo la creazione è visualizzata la pagina principale.
- Verificare che sia presente il nuovo campionamento.

1. Variante: non inserire uno o più dati e verificare che il campionamento non viene creato e che compare un messaggio di avviso sui dati mancanti.
2. Variante: non selezionare il punto di prelievo dall'anagrafica (pulsante "Ricerca in anagrafica") ma inserirne manualmente i dati facendo clic sul pulsante "Inserimento nuovo punto".

##### 4\. Modifica campionamento

- Fare clic sul titolo di uno dei campionamenti accanto cui non compare il badge "Completato".
- Modificare uno o più informazioni e fare clic sul pulsante "Chiudi" (i dati sono salvati automaticamente); in particolare rimuovere e aggiungere analiti e prelevatori e compilare i dati delle misure in loco.
- E' visualizzata la pagina principale; se i dati sono stati modificati è avviata automaticamente la sincronizzazione.
- Aprire la pagina di dettaglio del campionamento e verificare che i dati siano aggiornati.

##### 5\. Completamento campionamento

- Fare clic sul titolo di uno dei campionamenti accanto cui non compare il badge "Completato".
- Nella pagina di dettaglio del campionamento fare clic sul pulsante "Completa" e confermare la richiesta.
- E' visualizzata la pagina principale e avviata automaticamente la sincronizzazione.
- Verificare che accanto al campionamento sia comparso il badge "Completato".

1. Variante: nella pagina di dettaglio non inserire l'ora del prelievo o nessun prelevatore e verificare che il campionamento non venga completato e che compaia un messaggio di avviso sui dati mancanti.

##### 6\. Prelievo verbale

- Fare clic sul pulsante "Verbale" associato a un campionamento.
- Verificare che sia scaricato il file PDF del verbale contenente i dati del campionamento.
- Nota: il verbale è generato sul server da un processo in background asincrono e possono servire alcuni minuti prima che sia disponibile il link per il download che compare dopo una sincronizzazione (automatica o manuale).

##### 7\. Eliminazione campionamento

- Fare clic sul titolo di uno dei campionamenti accanto cui non compare il badge "Completato".
- Nella pagina di dettaglio del campionamento fare clic sul pulsante "Elimina" e confermare la richiesta.
- E' visualizzata la pagina principale e avviata automaticamente la sincronizzazione.
- Verificare che il campionamento non sia più presente.

##### 8\. Consultazione archivio campionamenti completati

- Fare clic sul pulsante "Campionamenti completati".
- E' visualizzata la pagina per la ricerca di campionamenti completati.
- Impostare vari filtri e verificare che i campionamenti elencati corrispondano a quanto cercato.

##### 9\. Prelievo rapporto di prova

- Fare clic sul pulsante "Campionamenti completati".
- Impostare i parametri di ricerca corrispondenti ad almeno un campionamento per cui in ALIMS è presente il rapporto di prova.
- Verificare che sia disponibile il link "Rapporto di prova" e che selezionandolo sia scaricato il PDF del rapporto.

Nota: il rapporto di prova è prelevato dal servizio esterno da un processo in background asincrono; il link per il download non è disponibile fino a quando il rapporto non è generato dal servizio esterno e prelevato.

##### 10\. Consultazione catalogo pacchetti

- Fare clic sul pulsante "Pacchetti ".
- Impostare diverse combinazioni di filtri per Matrice, Sede di accettazione e Pacchetto.
- Verificare che gli analiti e i contenitori risultanti dalla ricerca corrispondano a quanto richiesto.

##### 11\. Sincronizzazione manuale

La sincronizzazione col server avviene automaticamente quando sono modificati i dati dell'applicazione, quindi di norma l'esecuzione manuale non comporta modifica dei i dati tranne in due casi:

1. i dati sul server sono stati aggiornati da una altro operatore;
2. si sia utilizzata l'applicazione senza connettività.

- Fare clic sul pulsante "Sincronizza" e confermare la richiesta.
- Verificare che il processo venga eseguito (per un certo tempo è visibile una barra blu di avanzamento).

##### 12\. Funzionamento offline

L'applicazione è in grado di funzionare senza connessione Internet con funzionalità limitate. Quando rileva la mancanza di connessione disabilita le funzioni "Nuovo" (campionamento), "Sincronizza", "Campionamenti completati" e "Pacchetti".

Il rilevamento della mancata connessione è fatto dal browser in collaborazione con il sistema operativo.

- Mettere il dispositivo in condizione di assenza di connettività
- Verificare che i comandi corrispondenti alle suindicate funzioni non siano disponibili e che compaia il messaggio che indica l'assenza di connessione.

Nota

La mancanza di connessione non equivale all'impossibilità di raggiungere il server (in tal caso l'applicazione genera un errore quando tenta di comunicare).

Su PC e Laptop la rilevazione della mancanza di connessione può non funzionare in modo affidabile; è simulabile utilizzando gli strumenti di sviluppo inclusi nel browser.

## Modello dati

Di seguito le tabelle contenute nella base dati.

#### CAMPIONAMENTI

Dettagli dei campionamenti.

| **Campo** | **Descrizione** |
| --- | --- |
| ARGOMENTO_CODICE | Codice argomento |
| ARGOMENTO_DENOMINAZIONE | Denominazione argomento |
| CAMPIONAMENTO_COLLEGATO | Identificativo del campionamento collegato |
| CAMPIONE_BIANCO | Note su campione bianco |
| CF_OPERATORE_CREAZIONE | Codice fiscale dell’operatore che ha creato il campionamento |
| CLIENTE_CODICE | Identificativo del cliente |
| CLIENTE_CODICE_FISCALE | Codice fiscale del cliente |
| CLIENTE_DENOMINAZIONE | Denominazione del cliente |
| CLIENTE_PARTITA_IVA | Partita IVA del cliente |
| CODICE_SEDE_ACCETTAZIONE | Identificativo della sede di accettazione del campionamento |
| DATA_APERTURA_CAMPIONE | Data di apertura del campione |
| DATA_ORA_CHIUSURA | Data di chiusura del campionamento |
| DATA_ORA_CREAZIONE | Data di creazione del campionamento |
| DATA_PRELIEVO | Data del prelievo |
| DESCRIZIONE_ATTIVITA | Descrizione dell’attività |
| ELIMINATO | Indica se il campionamento è stato eliminato (cancellazione logica) |
| EMAIL_INVIO_VERBALE | Indirizzo email a cui inviare il verbale |
| ENTE_CODICE | Codice dell’ente |
| ENTE_DENOMINAZIONE | Denominazione dell’ente |
| FILE_RAPPORTO_DI_PROVA | Nome del file del rapporto di prova |
| FILE_VERBALE | Nome del file del verbale |
| FILE_VERBALE_FIRMATO | Nome del file del verbale firmato |
| ID_CAMPIONAMENTO | Identificativo univoco del campionamento (chiave primaria) |
| LUOGO_APERTURA_CAMPIONE | Note sul luogo di apertura campione |
| MATRICE_CODICE | Identificativo della matrice |
| MATRICE_DENOMINAZIONE | Denominazione della matrice |
| NOTA_ACCETTAZIONE | Note di accettazione |
| NOTA_ACCETTAZIONE_DATA_ORA | Data delle note di accettazione |
| NOTA_ACCETTAZIONE_OPER_CODICE | Identificativo dell’operatore autore delle note di accettazione |
| NOTA_ACCETTAZIONE_OPER_COGNOME | Cognome dell’operatore autore delle note di accettazione |
| NOTA_ACCETTAZIONE_OPER_NOME | Nome dell’operatore autore delle note di accettazione |
| NOTE | Note |
| NUMERO_CAMPIONE | Numero attribuito al campione |
| ORA_APERTURA_CAMPIONE | Ora di apertura del campione |
| ORA_PRELIEVO | Ora del prelievo |
| PEC_INVIO_VERBALE | Indirizzo PEC a cui inviare il verbale |
| PUNTO_ARPAL_CODICE | Identificativo del punto di campionamento secondo l’anagrafica dell’Agenzia |
| PUNTO_ARPAL_COMUNE_CODICE | Identificativo del Comune |
| PUNTO_ARPAL_COMUNE_DENOMINAZ | Denominazione del Comune |
| PUNTO_ARPAL_DENOMINAZIONE | Denominazione del punto |
| PUNTO_ARPAL_INDIRIZZO | Indirizzo del punto |
| PUNTO_ARPAL_LATITUDINE | Latitudine del punto |
| PUNTO_ARPAL_LONGITUDINE | Longitudine del punto |
| PUNTO_OPER_CODICE | Identificativo del punto se inserito dall’operatore |
| PUNTO_OPER_COMUNE_CODICE | Identificativo del Comune |
| PUNTO_OPER_COMUNE_DENOMINAZ | Denominazione del Comune |
| PUNTO_OPER_DENOMINAZ | Denominazione del punto |
| PUNTO_OPER_INDIRIZZO | Indirizzo del punto |
| PUNTO_OPER_LATITUDINE | Latitudine del punto |
| PUNTO_OPER_LONGITUDINE | Longitudine del punto |
| SIGLA_VERBALE | Sigla identificativa del verbale |
| STATO_CAMPIONE | Stato del campione |
| STATO_EMAIL_INVIO_VERBALE | Indica se l’email del verbale è stata inviata o è da inviare o l’invio ha ritornato un errore |
| SUFFISSO_CARTELLA | Suffisso della cartella del campionamento sul filesystem |
| TEMPERATURA_ACCETTAZIONE | Temperatura del campione all’accettazione |
| TIPO_CAMPIONAMENTO | Tipo di campionamento (indica un supplemento) |
| ULTIMO_AGG_DATA_ORA | Data di ultimo aggiornamento del record |
| ULTIMO_AGG_OPER_CF | Codice fiscale dell’operatore che ha effettuato l’ultimo aggiornamento |
| ULTIMO_AGG_OPER_CODICE | Identificativo dell’operatore che ha effettuato l’ultimo aggiornamento |
| ULTIMO_AGG_OPER_COGNOME | Cognome dell’operatore che ha effettuato l’ultimo aggiornamento |
| ULTIMO_AGG_OPER_NOME | Nome dell’operatore che ha effettuato l’ultimo aggiornamento |

#### ANALITI

Analiti associati ai campionamenti.

| **Campo** | **Descrizione** |
| --- | --- |
| AGGIUNTO_DA_OPER | Indica se l’analita è stato aggiunto dall’operatore rispetto a quelli presenti in configurazione |
| CODICE | Codice dell’analita |
| DESCRIZIONE | Descrizione dell’analita |
| ID_ANALITA | Identificativo dell’analita (chiave primaria) |
| ID_CAMPIONAMENTO | Identificativo del campionamento (chiave esterna) |
| METODO_CODICE | Identificativo del metodo |
| METODO_DESCRIZIONE | Descrizione del metodo |
| PACCHETTO_CODICE | Identificativo del pacchetto |
| PACCHETTO_DESCRIZIONE | Descrizione del pacchetto |
| RIMOSSO_DA_OPER | Indica se l’analita è stato rimosso dall’operatore rispetto a quelli presenti in configurazione |
| UNITA_MISURA | Unità di misura |
| VALORE_LIMITE | Valore limite accettabile |

#### CONTENITORI

Contenitori utilizzati per gli analiti.

| **Campo** | **Descrizione** |
| --- | --- |
| ID_ANALITA | Identificativo dell’analita (chiave esterna) |
| ID_CONTENITORE | Identificativo del contenitore (chiave primaria) |
| QUANTITA | Quantità dei contenitori |
| TIPO | Descrizione del contenitore |

#### PRELEVATORI/PRELEVATORI_SELEZIONABILI

Prelevatori associati a un campionamento o selezionabili.

| **Campo** | **Descrizione** |
| --- | --- |
| CODICE | Codice del prelevatore |
| COGNOME | Cognome del prelevatore |
| ID_CAMPIONAMENTO | Identificativo del campionamento (chiave esterna) |
| ID_PRELEVATORE | Identificativo del record prelevatore (chiave primaria) |
| NOME | Nome del prelevatore |

#### TIPI_CAMPIONAMENTO

Tipi di campionamento per matrice.

| **Campo** | **Descrizione** |
| --- | --- |
| DESCRIZIONE | Descrizione del tipo di campionamento |
| MATRICE_CODICE | Identificativo della matrice (chiave esterna) |
| NOTE | Note |
| TIPO_CAMPIONAMENTO | Identificativo del tipo di campionamento |

## Descrizione API applicazione back end

L’applicazione back end espone API utilizzate dall’applicazione front end.

La documentazione è consultabile in linea tramite l’interfaccia Swagger esposta dall’applicazione stessa all’indirizzo \[URL applicazione\]/swagger

| **Percorso relativo** | **Descrizione** |
| --- | --- |
| /analiti/ricerca | Ricerca analiti per matrice, argomento, pacchetto, descrizione e linea |
| /analiti/pacchetti | Ricerca pacchetto per matrice |
| /analiti/contenitori | Ricerca contenitori per pacchetto |
| /analiti/matrici | Matrici |
| /versionefrontend | Versione richiesta del front end dell’applicazione |
| /campionamenti/aperti | Elenco dei campionamenti aperti per l'operatore |
| /campionamenti/nuovo | Inserimento campionamento |
| /campionamenti/aggiornamento | Aggiornamento campionamenti |
| /campionamenti/completati | Campionamenti completati per l'operatore |
| /campionamenti/senzanumerocampione/{APIKey} | Dati dei campionamenti che non hanno il numero campione e per i quali esiste il file PDF del verbale. Richiede API key per utilizzo. |
| /campionamenti/completati/clienti/{CodiceEnte} | Clienti dei campionamenti completati per ente |
| /campionamenti/duplica | Duplicazione campionamento |
| /campionamenti/duplicacampionamentocompletato | Duplicazione campionamento completato |
| /campionamenti/tipi | Elenco tipi campionamento per codice matrice |
| /campionamenti/uploadverbalefirmato/{IdCampionamento} | Upload del file verbale firmato |
| /clienti/ricerca | Ricerca clienti per denominazione |
| /operatori/configurazione | Configurazione operatore in base al codice fiscale |
| /punti-di-prelievo/ricerca | Ricerca punto di prelievo per comune e denominazione |

## Dati da servizi esterni

L’applicazione utilizza il servizio anagrafiche esterne e il servizio informazioni sui campioni. Per l’autenticazione della chiamata utilizza una chiave verificata dal sistema esterno.

Di seguito i dati che l’applicazione si aspetta di ricevere per ogni chiamata. I tipi di attributi sono stringhe tranne dove indicato.

Il metodo è GET tranne dove indicato.

Il parametro della chiave è “apikey”.

### Servizio anagrafiche esterne

#### /analiti

Parametri in ingresso:

| **Parametro** | **Descrizione** |
| --- | --- |
| matrice | Codice della matrice |
| cod_argomento | Codice dell’argomento |
| cod_pack | Codice del pacchetto |
| analita_name | Nome dell’analita (anche sottostringa) |
| Linea | Nome della linea (“laboratorio” o “territorio”) |

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| pack_identity | Identificativo unico del pacchetto |
| pack_name | Descrizione del pacchetto |
| metodo_identity | Identificativo del metodo di analisi |
| metodo_name | Descrizione del metodo |
| analita_identity | Identificativo dell'analita |
| analita_name | Descrizione dell'analita |
| param_units | Unità di misura del parametro |
| valore_limite | Valore limite normativo del parametro |
| order_num | Numero di ordine dell’elemento (numero intero) |

#### /clienti

Anagrafica dei clienti

Parametri in ingresso:

| **Parametro** | **Descrizione** |
| --- | --- |
| denominazione | Denominazione del cliente (anche sottostringa) |
| cf_piva | Codice fiscale o partita IVA |

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| identity | Identificativo del cliente |
| company_name | Nome della società o ente |
| address1 | Indirizzo |
| address2 | Numero civico |
| address3 | Comune |
| address4 | Provincia |
| codice_fiscale | Codice fiscale |
| p_iva | Partita IVA |

#### /contenitori

Parametri in ingresso:

| **Parametro** | **Descrizione** |
| --- | --- |
| pack_identity | Codice del pacchetto |

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| bottle_type | Tipo di contenitore |
| Quantity | Quantità di contenitori richiesti |

#### /matrici

Elenco delle matrici ambientali

Parametri in ingresso: nessuno

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| identity | Identificativo della matrice |
| name | Descrizione della matrice |
| description | Descrizione di dettaglio della matrice |

#### /operatore

Configurazione operatore.

Parametri in ingresso:

| **Parametro** | **Descrizione** |
| --- | --- |
| cfpiva | Codice fiscale o partita IVA dell’operatore |

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| cod_ana | Identificativo dell’operatore |
| cognome |     |
| nome |     |
| cf  |     |
| cod_argomento |     |
| argomento |     |
| matrice_identity |     |
| matrice_name |     |
| cod_ana_ente |     |
| rag_soc_ente |     |
| id_alims_clienti | Codice cliente |

#### /pacchetto

Parametri in ingresso:

| **Parametro** | **Descrizione** |
| --- | --- |
| matrice | Codice della matrice |
| cod_argomento | Codice dell’argomento |
| sede | Codice della sede di accettazione |

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| pack_identity | Identificativo del pacchetto |
| pack_name | Descrizione del pacchetto |
| Sede | Codice della sede di accettazione |

#### /punti

Anagrafica dei punti di campionamento

Parametri in ingresso:

| **Parametro** | **Descrizione** |
| --- | --- |
| comune | Codice ISTAT del Comune |
| param | Codice o descrizione del parametro (sottostringa) |

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| identity | Identificativo del punto |
| description | Descrizione del punto |
| name | Nome del punto |
| indirizzo | Indirizzo |
| comune | Codice ISTAT del Comune |
| nome_comune | Nome del Comune |
| latitudine |     |
| longitudine |     |

#### /prelevatori

Anagrafica dei prelevatori

Parametri in ingresso:

| **Parametro** | **Descrizione** |
| --- | --- |
| ente | Identificativo dell’ente |
| cod_argomento | Identificativo dell’argomento |

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| cod_ana | Identificativo del prelevatore |
| cognome | Cognome |
| nome | Nome |
| cod_argomento | Identificativo dell’argomento |
| cod_ana_ente | Identificativo dell’ente |

#### /sede

Elenco delle sedi di accettazione

Parametri in ingresso: nessuno

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| sede | Identificativo della sede |
| descr_sede | Descrizione della sede |

### Servizio informazioni sui campioni

#### /campione_punti

Numero campione e dati punto

Parametri in ingresso:

| **Parametro** | **Descrizione** |
| --- | --- |
| verbale | Sigla del verbale |

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| id_text | Identificativo del campione |
| login_date |     |
| sigla_verbale | Identificativo del verbale |
| date_verbale |     |
| status |     |
| rdp_name | Identificativo del rapporto di prova (RDP) associato al campione |
| rdp_created_on |     |
| archived_data_protocollo |     |
| archived_n_protocollo |     |
| archived_on |     |
| archived_by |     |
| sampling_point |     |
| sampling_point_cap |     |
| sampling_point_comune |     |
| sampling_point_frazione |     |
| sampling_point_indirizzo |     |
| stato |     |
| description |     |
| temperatura |     |

#### /stato_campione

Parametri in ingresso:

| **Parametro** | **Descrizione** |
| --- | --- |
| id  | Identificativo del campione |

Output:

Lista di oggetti (items) contenenti i seguenti attributi:

| **Attributo** | **Descrizione** |
| --- | --- |
| id_text | Identificativo del campione |
| stato | Stato del campione (“Validato”, “In accettazione”, “In analisi”, “Cancellato”, “Completato”) |
| temperatura |     |

#### /RDP

File del rapporto di prova.

Parametri in ingresso:

| **Parametro** | **Descrizione** |
| --- | --- |
| camp | Identificativo del campione |

Output:

File del rapporto di prova
