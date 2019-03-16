﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Devmasters.Core;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using HlidacStatu.Util;

namespace HlidacStatu.Lib.Data
{
    public partial class Osoba
    {

        public JSON ToJsonEditor()
        {
            var t = this;
            var r = new JSON();
            
            r.Gender =  (t.Pohlavi == "f") ? JSON.gender.Žena : JSON.gender.Muž;
            r.NameId = t.NameId;
            r.Jmeno = t.Jmeno;
            r.Narozeni = t.Narozeni?.ToString("yyyy-MM-dd") ?? "";
            r.Umrti = t.Umrti?.ToString("yyyy-MM-dd") ?? "";
            r.Prijmeni = t.Prijmeni;
            r.Status = (Osoba.StatusOsobyEnum)t.Status;
            r.Event = t.Events().Select(m =>
                new JSON.ev()
                {
                    pk = m.pk,
                    AddInfo = m.AddInfo,
                    DatumOd = m.DatumOd?.ToString("yyyy-MM-dd") ?? "",
                    DatumDo = m.DatumDo?.ToString("yyyy-MM-dd") ?? "",
                    Title = m.Title,
                    Description = m.Description,
                    Typ = (OsobaEvent.Types)m.Type,
                    AddInfoNum = m.AddInfoNum,
                    Zdroj = m.Zdroj
                }
                ).ToArray();
            //throw new NotImplementedException();
            string[] angazovanostDesc = Enums.EnumToEnumerable(typeof(Firma.RelationSimpleEnum))
                .Where(m => Convert.ToInt32(m.Value) < 0)
                .Select(m => m.Key).ToArray();
            

            r.Vazbyfirmy = Lib.Data.Graph.VsechnyDcerineVazby(this)
                .Where(m => angazovanostDesc.Contains(m.Descr))
                .Select(m =>
                    new JSON.vazba()
                    {
                        DatumOd = m.RelFrom?.ToString("yyyy-MM-dd") ?? "",
                        DatumDo = m.RelTo?.ToString("yyyy-MM-dd") ?? "",
                        Popis = m.Descr,
                        TypVazby = (JSON.typVazby) Enum.Parse(typeof(JSON.typVazby),m.Descr, true) ,
                        VazbaKIco = m.To.Id,
                        VazbaKOsoba = m.To.PrintName() 
                        //Zdroj = m.
                    })
                .ToArray();

            return r;
        }



        public partial class JSON 
        {

            public Osoba Save(string user)
            {
                Osoba o = null;
                var t = this;

                DateTime? narozeni = ParseTools.ToDateTime(t.Narozeni, "yyyy-MM-dd");
                //find existing
                if (this.Id > 0)
                    o = Osoby.GetById.Get(this.Id);
                else if (narozeni.HasValue == false)
                    return null;
                if (o==null)
                    o = Osoba.GetByName(t.Jmeno, t.Prijmeni, narozeni.Value);
                if (o == null)
                    o = Osoba.GetByNameAscii(t.Jmeno, t.Prijmeni, narozeni.Value);

                if (o == null)
                    o = new Data.Osoba();

                o.Pohlavi = (t.Gender == gender.Žena) ? "f" : "m";
                o.Jmeno = t.Jmeno;
                o.Narozeni = narozeni.Value;
                o.Umrti = ParseTools.ToDateTime(t.Umrti, "yyyy-MM-dd");
                o.Prijmeni = t.Prijmeni;
                o.Status = (int)t.Status;
                o.NameId = t.NameId;
                o.Save();

                foreach (var e in t.Event)
                {
                    OsobaEvent ev = new OsobaEvent();
                    ev.pk = e.pk;
                    ev.AddInfo = e.AddInfo;
                    ev.AddInfoNum = e.AddInfoNum;
                    ev.DatumOd = ParseTools.ToDateTime(e.DatumOd, "yyyy-MM-dd");
                    ev.DatumDo = ParseTools.ToDateTime(e.DatumDo, "yyyy-MM-dd");
                    ev.Description = e.Description;
                    ev.Title = e.Title;
                    ev.Type = (int)e.Typ;
                    ev.Zdroj = e.Zdroj;
                    ev.OsobaId = o.InternalId;                    
                    o.AddOrUpdateEvent(ev,user);
                }

                foreach (var v in t.Vazbyfirmy)
                {
                    if (!string.IsNullOrEmpty(v.VazbaKOsoba))
                    {
                        var os = Osoby.GetByNameId.Get(v.VazbaKOsoba);
                        if (os != null)
                        {
                            OsobaVazby.AddOrUpdate(o.InternalId, os.InternalId, (int)v.TypVazby, v.Popis, 0, ParseTools.ToDateTime(v.DatumOd, "yyyy-MM-dd"), ParseTools.ToDateTime(v.DatumDo, "yyyy-MM-dd"), v.Zdroj);
                        }
                    }
                    else
                    {
                        OsobaVazby.AddOrUpdate(o.InternalId, v.VazbaKIco, (int)v.TypVazby, v.Popis, 0, ParseTools.ToDateTime(v.DatumOd, "yyyy-MM-dd"), ParseTools.ToDateTime(v.DatumDo, "yyyy-MM-dd"), v.Zdroj);
                    }
                }
                return o;
            }


            [JsonProperty("id", Required = Required.Default)]
            public int Id
            {
                get; set;
            }
            [JsonProperty("nameId", Required = Required.Default)]
            public string NameId
            {
                get; set;
            }

            [JsonProperty("jmeno", Required = Required.Default)]
            public string Jmeno
            {
                get; set;
            }

            [JsonProperty("prijmeni", Required = Required.Default)]
            public string Prijmeni
            {
                get; set;
            }

            /// <summary>ve formátu 1982-03-23 (rok-mesic-den)</summary>
            [JsonProperty("narozeni", Required = Required.Default)]
            public string Narozeni
            {
                get; set;
            }

            [JsonProperty("umrti", Required = Required.Default)]
            public string Umrti
            {
                get; set;
            }



            [JsonProperty("gender", Required = Required.Default)]
            [JsonConverter(typeof(StringEnumConverter))]
            public gender Gender
            {
                get; set;
            }

            [JsonProperty("status", Required = Required.Default)]
            [JsonConverter(typeof(StringEnumConverter))]
            public Osoba.StatusOsobyEnum Status
            {
                get; set;
            }

            /// <summary>ve formátu 1982-03-23 (rok-mesic-den)</summary>
            [JsonProperty("autor", Required = Required.Default)]
            public string Autor
            {
                get; set;
            }

            [JsonProperty("event", Required = Required.Default)]
            public ev[] Event
            {
                get; set;
            }

            [JsonProperty("vazbyfirmy", Required = Required.Default)]
            public vazba[] Vazbyfirmy
            {
                get; set;
            }




            public enum gender
            {
                Muž = 0,
                Žena = 1,
            }

         

            public enum typVazby
            {
                OsobniVztah = -3,
                Vliv = -2,
                Kontrola = -1,
            }

            public partial class ev
            {

                [JsonProperty("pk", Required = Required.Default)]
                public int pk { get; set; } = 0;

                [JsonProperty("title", Required = Required.Default)]
                public string Title
                {
                    get; set;
                }
                [JsonProperty("description", Required = Required.Default)]
                public string Description
                {
                    get; set;
                }


                /// <summary>Pokud politik, tak za jakou stranu</summary>
                [JsonProperty("addInfo", Required = Required.Default)]
                public string AddInfo
                {
                    get; set;
                }
                /// <summary>Pokud politik, tak za jakou stranu</summary>
                [JsonProperty("addInfoNum", Required = Required.Default)]
                public decimal? AddInfoNum
                {
                    get; set;
                }


                [JsonProperty("typ", Required = Required.Default)]
                [JsonConverter(typeof(StringEnumConverter))]
                public OsobaEvent.Types Typ
                {
                    get; set;
                }

                /// <summary>Ve formatu yyyy-MM-dd (1983-03-26)</summary>
                [JsonProperty("datumOd", Required = Required.Default)]
                public string DatumOd
                {
                    get; set;
                }

                /// <summary>Ve formatu yyyy-MM-dd (1983-03-26)</summary>
                [JsonProperty("datumDo", Required = Required.Default)]
                public string DatumDo
                {
                    get; set;
                }
                [JsonProperty("zdroj", Required = Required.Default)]
                public string Zdroj
                {
                    get; set;
                }

            }

            public partial class vazba
            {


                [JsonProperty("vazbaKIco", Required = Required.Default)]
                public string VazbaKIco
                {
                    get; set;
                }
                [JsonProperty("vazbaKOsoba", Required = Required.Default)]
                public string VazbaKOsoba
                {
                    get; set;
                }

                /// <summary>Formalni vztahy automaticky nacteme z Obchodniho rejstriku</summary>
                [JsonProperty("typVazby", Required = Required.Default)]
                [JsonConverter(typeof(StringEnumConverter))]
                public typVazby TypVazby
                {
                    get; set;
                }

                [JsonProperty("popis", Required = Required.Default)]
                public string Popis
                {
                    get; set;
                }

                /// <summary>Ve formatu yyyy-MM-dd (1983-03-26)</summary>
                [JsonProperty("datumOd", Required = Required.Default)]
                public string DatumOd
                {
                    get; set;
                }

                /// <summary>Ve formatu yyyy-MM-dd (1983-03-26)</summary>
                [JsonProperty("datumDo", Required = Required.Default)]
                public string DatumDo
                {
                    get; set;
                }

                /// <summary>Zdroj (URL], ktery tuto vazbu popisuje/dokazuje</summary>
                [JsonProperty("zdroj", Required = Required.Default)]
                public string Zdroj
                {
                    get; set;
                }



            }
        }




    }

}
