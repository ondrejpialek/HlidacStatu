﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HlidacStatu.Lib.Db.Insolvence
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class InsolvenceEntities : DbContext
    {
        public InsolvenceEntities()
            : base("name=InsolvenceEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Dluznici> Dluznici { get; set; }
        public virtual DbSet<Dokumenty> Dokumenty { get; set; }
        public virtual DbSet<Rizeni> Rizeni { get; set; }
        public virtual DbSet<Spravci> Spravci { get; set; }
        public virtual DbSet<Veritele> Veritele { get; set; }
    }
}
