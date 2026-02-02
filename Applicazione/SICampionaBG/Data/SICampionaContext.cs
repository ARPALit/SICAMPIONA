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
using Microsoft.EntityFrameworkCore;


namespace SICampionaBG.Data;

public partial class SICampionaContext : DbContext
{
    public SICampionaContext()
    {
    }

    public SICampionaContext(DbContextOptions<SICampionaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Analita> ANALITI { get; set; }

    public virtual DbSet<CampionamentoDB> CAMPIONAMENTI { get; set; }

    public virtual DbSet<Contenitore> CONTENITORI { get; set; }

    public virtual DbSet<MisuraInLoco> MISURE_IN_LOCO { get; set; }

    public virtual DbSet<Prelevatore> PRELEVATORI { get; set; }

    public virtual DbSet<TipoCampionamento> TIPI_CAMPIONAMENTO { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("SICAMPIONA");

        modelBuilder.Entity<Analita>(entity =>
        {
            entity.HasKey(e => e.ID_ANALITA).HasName("ANALITI_PK");

            entity.Property(e => e.ID_ANALITA)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER");
            entity.Property(e => e.AGGIUNTO_DA_OPER)
                .IsRequired()
                .HasPrecision(1)
                .HasDefaultValueSql("0 ");
            entity.Property(e => e.CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DESCRIZIONE)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ID_CAMPIONAMENTO).HasColumnType("NUMBER");
            entity.Property(e => e.METODO_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.METODO_DESCRIZIONE)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PACCHETTO_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PACCHETTO_DESCRIZIONE)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.VALORE_LIMITE)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UNITA_MISURA)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RIMOSSO_DA_OPER)
                .IsRequired()
                .HasPrecision(1)
                .HasDefaultValueSql("0 ");

            entity.HasOne(d => d.ID_CAMPIONAMENTONavigation).WithMany(p => p.ANALITI)
                .HasForeignKey(d => d.ID_CAMPIONAMENTO)
                .HasConstraintName("ANALITI_FK1");
        });

        modelBuilder.Entity<CampionamentoDB>(entity =>
        {
            entity.HasKey(e => e.ID_CAMPIONAMENTO).HasName("CAMPIONAMENTI_PK");

            entity.HasIndex(e => e.NUMERO_CAMPIONE, "CAMPIONAMENTI_INDEX1");

            entity.Property(e => e.ID_CAMPIONAMENTO)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER");
            entity.Property(e => e.ARGOMENTO_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ARGOMENTO_DENOMINAZIONE)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CLIENTE_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CLIENTE_CODICE_FISCALE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CLIENTE_DENOMINAZIONE)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CLIENTE_PARTITA_IVA)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DATA_ORA_CHIUSURA).HasColumnType("DATE");
            entity.Property(e => e.DATA_ORA_CREAZIONE).HasColumnType("DATE");
            entity.Property(e => e.DATA_PRELIEVO).HasColumnType("DATE");
            entity.Property(e => e.DESCRIZIONE_ATTIVITA)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.ENTE_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ENTE_DENOMINAZIONE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MATRICE_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MATRICE_DENOMINAZIONE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NOTA_ACCETTAZIONE)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.NOTA_ACCETTAZIONE_DATA_ORA).HasColumnType("DATE");
            entity.Property(e => e.NOTA_ACCETTAZIONE_OPER_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NOTA_ACCETTAZIONE_OPER_COGNOME)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NOTA_ACCETTAZIONE_OPER_NOME)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NUMERO_CAMPIONE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ORA_PRELIEVO)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_ARPAL_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_ARPAL_COMUNE_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_ARPAL_COMUNE_DENOMINAZ)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_ARPAL_DENOMINAZIONE)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_ARPAL_INDIRIZZO)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_ARPAL_LATITUDINE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_ARPAL_LONGITUDINE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_OPER_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_OPER_COMUNE_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_OPER_COMUNE_DENOMINAZ)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_OPER_DENOMINAZ)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_OPER_INDIRIZZO)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_OPER_LATITUDINE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PUNTO_OPER_LONGITUDINE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SIGLA_VERBALE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FILE_VERBALE)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FILE_RAPPORTO_DI_PROVA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.STATO_CAMPIONE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ULTIMO_AGG_DATA_ORA).HasColumnType("DATE");
            entity.Property(e => e.ULTIMO_AGG_OPER_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ULTIMO_AGG_OPER_COGNOME)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ULTIMO_AGG_OPER_NOME)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SUFFISSO_CARTELLA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.STATO_EMAIL_INVIO_VERBALE)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TEMPERATURA_ACCETTAZIONE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CAMPIONE_BIANCO)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.LUOGO_APERTURA_CAMPIONE)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.NOTE)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Contenitore>(entity =>
        {
            entity.HasKey(e => e.ID_CONTENITORE).HasName("CONTENITORI_PK");

            entity.Property(e => e.ID_CONTENITORE)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER");
            entity.Property(e => e.ID_ANALITA).HasColumnType("NUMBER");
            entity.Property(e => e.QUANTITA).HasColumnType("NUMBER");
            entity.Property(e => e.TIPO)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.ID_ANALITANavigation).WithMany(p => p.CONTENITORI)
                .HasForeignKey(d => d.ID_ANALITA)
                .HasConstraintName("CONTENITORI_FK1");
        });

        modelBuilder.Entity<MisuraInLoco>(entity =>
        {
            entity.HasKey(e => e.ID_MISURA).HasName("MISURE_IN_LOCO_PK");

            entity.Property(e => e.ID_MISURA)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER");
            entity.Property(e => e.ANNOTAZIONI_OPERATORE)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DESCRIZIONE)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ID_CAMPIONAMENTO).HasColumnType("NUMBER");
            entity.Property(e => e.METODO_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.METODO_DESCRIZIONE)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.VALORE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UNITA_MISURA)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ID_CAMPIONAMENTONavigation).WithMany(p => p.MISURE_IN_LOCO)
                .HasForeignKey(d => d.ID_CAMPIONAMENTO)
                .HasConstraintName("MISURE_IN_LOCO_FK1");
        });

        modelBuilder.Entity<Prelevatore>(entity =>
        {
            entity.HasKey(e => e.ID_PRELEVATORE).HasName("PRELEVATORI_PK");

            entity.Property(e => e.ID_PRELEVATORE)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER");
            entity.Property(e => e.CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.COGNOME)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ID_CAMPIONAMENTO).HasColumnType("NUMBER");
            entity.Property(e => e.NOME)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.ID_CAMPIONAMENTONavigation).WithMany(p => p.PRELEVATORI)
                .HasForeignKey(d => d.ID_CAMPIONAMENTO)
                .HasConstraintName("PRELEVATORI_FK1");
        });

        modelBuilder.Entity<TipoCampionamento>(entity =>
        {
            entity.HasKey(e => new { e.TIPO_CAMPIONAMENTO, e.MATRICE_CODICE }).HasName("TIPI_CAMPIONAMENTO_PK");

            entity.Property(e => e.TIPO_CAMPIONAMENTO)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MATRICE_CODICE)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DESCRIZIONE)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NOTE)
                .IsRequired()
                .HasMaxLength(1000)
                .IsUnicode(false);
        });

        modelBuilder.HasSequence("ANALITI_SEQ");
        modelBuilder.HasSequence("CAMPIONAMENTI_SEQ");
        modelBuilder.HasSequence("CONTENITORI_SEQ");
        modelBuilder.HasSequence("MISURE_IN_LOCO_SEQ");
        modelBuilder.HasSequence("PRELEVATORI_SEQ");
    }
}
