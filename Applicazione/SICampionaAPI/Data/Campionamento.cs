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
*/namespace SICampionaAPI.Data;

public partial class CampionamentoDB
{
    public decimal ID_CAMPIONAMENTO { get; set; }

    public string ENTE_CODICE { get; set; } = null!;

    public string ENTE_DENOMINAZIONE { get; set; } = null!;

    public string MATRICE_CODICE { get; set; } = null!;

    public string ARGOMENTO_CODICE { get; set; } = null!;

    public string ARGOMENTO_DENOMINAZIONE { get; set; } = null!;

    public string CLIENTE_CODICE { get; set; } = null!;

    public string CLIENTE_DENOMINAZIONE { get; set; } = null!;

    public string CLIENTE_CODICE_FISCALE { get; set; } = null!;

    public string CLIENTE_PARTITA_IVA { get; set; } = null!;

    public string PUNTO_ARPAL_CODICE { get; set; } = null!;

    public string PUNTO_ARPAL_DENOMINAZIONE { get; set; } = null!;

    public string PUNTO_ARPAL_INDIRIZZO { get; set; } = null!;

    public string PUNTO_ARPAL_COMUNE_CODICE { get; set; } = null!;

    public string PUNTO_ARPAL_COMUNE_DENOMINAZ { get; set; } = null!;

    public string PUNTO_ARPAL_LATITUDINE { get; set; }

    public string PUNTO_ARPAL_LONGITUDINE { get; set; }

    public string PUNTO_OPER_CODICE { get; set; }

    public string PUNTO_OPER_DENOMINAZ { get; set; }

    public string PUNTO_OPER_INDIRIZZO { get; set; }

    public string PUNTO_OPER_COMUNE_CODICE { get; set; }

    public string PUNTO_OPER_COMUNE_DENOMINAZ { get; set; }

    public string PUNTO_OPER_LATITUDINE { get; set; }

    public string PUNTO_OPER_LONGITUDINE { get; set; }

    public string DESCRIZIONE_ATTIVITA { get; set; }

    public DateTime DATA_ORA_CREAZIONE { get; set; }

    public DateTime DATA_PRELIEVO { get; set; }

    public string ORA_PRELIEVO { get; set; }

    public DateTime? DATA_ORA_CHIUSURA { get; set; }

    public DateTime ULTIMO_AGG_DATA_ORA { get; set; }

    public string ULTIMO_AGG_OPER_CODICE { get; set; } = null!;

    public string ULTIMO_AGG_OPER_COGNOME { get; set; } = null!;

    public string ULTIMO_AGG_OPER_NOME { get; set; } = null!;

    public string SIGLA_VERBALE { get; set; }

    public string NOTA_ACCETTAZIONE { get; set; }

    public string NUMERO_CAMPIONE { get; set; }

    public string MATRICE_DENOMINAZIONE { get; set; } = null!;

    public DateTime? NOTA_ACCETTAZIONE_DATA_ORA { get; set; }

    public string NOTA_ACCETTAZIONE_OPER_CODICE { get; set; }

    public string NOTA_ACCETTAZIONE_OPER_COGNOME { get; set; }

    public string NOTA_ACCETTAZIONE_OPER_NOME { get; set; }

    public string SUFFISSO_CARTELLA { get; set; }

    public bool ELIMINATO { get; set; }

    public string FILE_VERBALE { get; set; }

    public string FILE_RAPPORTO_DI_PROVA { get; set; }

    public string STATO_CAMPIONE { get; set; }

    public string CF_OPERATORE_CREAZIONE { get; set; }

    public string ULTIMO_AGG_OPER_CF { get; set; }

    public string CODICE_SEDE_ACCETTAZIONE { get; set; }

    public DateTime? DATA_APERTURA_CAMPIONE { get; set; }

    public string ORA_APERTURA_CAMPIONE { get; set; }

    public string EMAIL_INVIO_VERBALE { get; set; }

    public string PEC_INVIO_VERBALE { get; set; }

    public string CAMPIONAMENTO_COLLEGATO { get; set; }

    public string TIPO_CAMPIONAMENTO { get; set; }

    public string FILE_VERBALE_FIRMATO { get; set; }

    public string CAMPIONE_BIANCO { get; set; }

    public string TEMPERATURA_ACCETTAZIONE { get; set; }
    
    public string LUOGO_APERTURA_CAMPIONE { get; set; }
    
    public string NOTE { get; set; }

    public virtual ICollection<Analita> ANALITI { get; set; } = new List<Analita>();

    public virtual ICollection<MisuraInLoco> MISURE_IN_LOCO { get; set; } = new List<MisuraInLoco>();

    public virtual ICollection<Prelevatore> PRELEVATORI { get; set; } = new List<Prelevatore>();

    public virtual ICollection<PrelevatoreSelezionabile> PRELEVATORI_SELEZIONABILI { get; set; } = new List<PrelevatoreSelezionabile>();
}
