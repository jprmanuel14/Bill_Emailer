using BillColl_Main.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.AppDbContext
{
    public class myDBContext : DbContext
    {
        public myDBContext(DbContextOptions<myDBContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // The database was migrated from SQL Server; all tables live in the
            // "dbo" schema. Without this, EF Core targets "public" and every query
            // fails with 42P01 relation "<Table>" does not exist.
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<BankPHEntity>().HasKey(bp => new { bp.BankId, bp.PHEntityId });
            modelBuilder.Entity<BankPHEntity>()
                .HasOne<Bank>(bp => bp.Bank)
                .WithMany(b => b.BankPHEntity)
                .HasForeignKey(bp => bp.BankId);
            modelBuilder.Entity<BankPHEntity>()
                .HasOne<PHEntity>(bp => bp.PHEntity)
                .WithMany(p => p.BankPHEntity)
                .HasForeignKey(bp => bp.PHEntityId);

            // The imported column is "timestamp without time zone". Npgsql maps
            // DateTime to "timestamp with time zone" by default, which rejects the
            // Kind=Unspecified values read back from a PostgreSQL date column.
            modelBuilder.Entity<ClientRemarks>()
                .Property(c => c.StatementDate)
                .HasColumnType("timestamp without time zone");



            modelBuilder.Entity<Bank>().HasData(
                        new Bank()
                        {
                            Id = 1,
                            AccountNumber = "3691-0000-27 - Current account-Peso <br><br> 3694-0001-25(Dollar account)",
                            AccountName = "Isla Lipana and Co.",
                            BankName = "Bank of the Philippine Islands",
                            Branch = "Malate, Metro Manila, Philippines",
                            SwiftCode = "BOPIPHMM"
                        },
                         new Bank()
                         {
                             Id = 2,
                             AccountNumber = "001688052218 - Current Account Peso",
                             AccountName = "Isla Lipana and Co.",
                             BankName = "BDO Unibank, Inc.",
                             Branch = "BDO Philam Tower- Valero Branch",
                             SwiftCode = "BNORPHMM"
                         },
                        new Bank()
                        {
                            Id = 3,
                            AccountNumber = "99196741877 - Current account-Peso <br><br> 9010196927767 - SA-(Dollar account)",
                            AccountName = "PricewaterhouseCoopers Consulting Services Philippines Co.Ltd..",
                            BankName = "Standard Chartered Bank",
                            Branch = "Ayala, Makati City",
                            SwiftCode = "SCBLPHMM"
                        },
                        new Bank()
                        {
                            Id = 4,
                            AccountNumber = "0031-0681-69 - Current account-Peso <br><br> 0034-0280-95 - (Dollar Account)",
                            AccountName = "Cabrera & Company",
                            BankName = "Bank of the Philippine Islands",
                            Branch = "Paseo de Roxas, Makati City",
                            SwiftCode = "BOPIPHMM"
                        },

                             new Bank()
                             {
                                 Id = 8,
                                 AccountCode= "040",
                                 AccountNumber = "PHP (C/A)   0031-0744-01 <br><br> USD (S/A)	0034-0547-62",
                                 AccountName = "PRICEWATERHOUSECOOPERS WMS PTE. LTD.",
                                 BankName = "BANK OF THE PHILIPPINE ISLANDS",
                                 Branch = "AYALA-PASEO",
                                 SwiftCode = "BOPIPHMM"
                             },
                             new Bank()
                             {
                                 Id = 9,
                                 AccountCode = "090",
                                 AccountNumber = "8074000968-Current account - Peso <br><br> 8071000371 (Dollar account)",
                                 AccountName = "PricewaterhouseCoopers Business Services Philippines Co., Ltd.  ",
                                 BankName = "Bank of the Philippine Islands",
                                 Branch = "BPI St. Francis Square",
                                 SwiftCode = "BOPIPHMM"
                             }
                             ,
                                new Bank()
                                {
                                    Id = 10,
                                    AccountCode = "060",
                                    AccountNumber = "00-001-01-0048872",
                                    AccountName = "PRICEWATERHOUSECOOPERS SERVICES",
                                    BankName = "Bank Islam Brunei Darussalam",
                                    Branch = "",
                                    SwiftCode = "BIBDBNBB"
                                }



            );

            modelBuilder.Entity<PHEntity>().HasData(
                new PHEntity() { 
                    Id=1,
                    EntityCode="",
                    EntityName= "Isla Lipana & Co.",
                    ReportHierarchy=1
                },
                new PHEntity()
                {
                    Id = 2,
                    EntityCode = "",
                    EntityName = "Cabrera & Company",
                     ReportHierarchy = 2
                },
                new PHEntity()
                {
                    Id = 3,
                    EntityCode = "",
                    EntityName = "PwC WMS Pte Ltd.",
                    ReportHierarchy = 5
                }
                ,
                new PHEntity()
                {
                    Id = 4,
                    EntityCode = "",
                    EntityName = "PwC CONS Ltd.",
                    ReportHierarchy = 3
                }
                 ,
                new PHEntity()
                {
                    Id = 5,
                    EntityCode = "",
                    EntityName = "PwC Services",
                    ReportHierarchy = 6
                }
                 ,
                new PHEntity()
                {
                    Id = 6,
                    EntityCode = "",
                    EntityName = "PwC BSP Co. Ltd.",
                    ReportHierarchy = 4
                }
            );

            modelBuilder.Entity<BankPHEntity>().HasData(
                new BankPHEntity()
                {
                    BankId=1,
                    PHEntityId=1
                },
                 new BankPHEntity()
                 {
                     BankId = 2,
                     PHEntityId = 1
                 },
                  new BankPHEntity()
                  {
                      BankId = 3,
                      PHEntityId = 4
                  },
                   new BankPHEntity()
                   {
                       BankId = 4,
                       PHEntityId = 2
                   }
           );

            modelBuilder.Entity<ContactsLimit>().HasData(
                new ContactsLimit()
                {
                    Id=1,
                    Limit=5
                }
            );

            modelBuilder.Entity<User>().HasData(

                new User() { 
                    Id=1,
                    GUID="",
                    Email="sancho.james.inocencio@pwc.com"
                },


                new User()
                {
                    Id = 2,
                    GUID = "",
                    Email = "renie.rose.ann.manaois@pwc.com"
                },


                new User()
                {
                    Id = 3,
                    GUID = "",
                    Email = "enjelo.marrius.balane@pwc.com"
                },

                new User()
                {
                    Id = 4,
                    GUID = "",
                    Email = "john.patrick.manuel@pwc.com"
                },

                new User()
                {
                    Id = 5,
                    GUID = "",
                    Email = "jherremy.ivan.roldan@pwc.com"
                },

                new User()
                {
                    Id = 6,
                    GUID = "",
                    Email = "edna.c.brimon@pwc.com"
                },


                new User()
                {
                    Id = 7,
                    GUID = "",
                    Email = "genpros.sanidad@pwc.com"
                },

                new User()
                {
                    Id = 8,
                    GUID = "",
                    Email = "monalisa.m.mamasao@pwc.com"
                },

                new User()
                {
                    Id = 9,
                    GUID = "",
                    Email = "carla.v.tanieca@pwc.com"
                },

                new User()
                {
                    Id = 10,
                    GUID = "",
                    Email = "kyle.christopher.quiogue@pwc.com"
                },

                new User()
                {
                    Id = 11,
                    GUID = "",
                    Email = "christine.maneja@pwc.com"
                },

                new User()
                {
                    Id = 12,
                    GUID = "",
                    Email = "alvin.p.mendoza@pwc.com"
                },

                new User()
                {
                    Id = 13,
                    GUID = "",
                    Email = "carol.a.barandon@pwc.com"
                },
                
                new User()
                {
                    Id = 14,
                    GUID = "",
                    Email = "sandro.mark.laguerta@pwc.com"
                },
                new User()
                {
                    Id = 15,
                    GUID = "",
                    Email = "kenneth.aquino@pwc.com"
                }
                

            
            );

        }

        public DbSet<Contacts> Contacts { get; set; }
        public DbSet<EngagementTeam> EngagementTeams { get; set; }
        public DbSet<ClientRemarks> ClientRemarks { get; set; }
        public DbSet<Exceptions> Exceptions { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<PHEntity> PHEntity { get; set; }
        public DbSet<BankPHEntity> BankPHEntity { get; set; }
        public DbSet<ContactsLimit> ContactsLimit { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
