﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HlidacStatu.Lib.Data;
using static HlidacStatu.Lib.Data.WatchDog;

namespace HlidacStatu.Lib.Watchdogs
{
    public class SingleEmailPerUserProcessor
    {

        public static void Send(IEnumerable<WatchDog> watchdogs, bool force = false, DateTime ? fromSpecificDate=null, DateTime? toSpecificDate = null)
        {
            bool saveWatchdogStatus = 
                force == false 
                && fromSpecificDate.HasValue == false 
                && toSpecificDate.HasValue == false;

            Dictionary<string, WatchDog[]> groupedByUserNoSpecContact = watchdogs
                .Where(w => w != null)
                .Where(m => string.IsNullOrEmpty(m.SpecificContact))
                .GroupBy(k => k.User().Id,
                        v => v,
                        (k, v) => new { key = k, val = v.ToArray() }
                        )
                .ToDictionary(k => k.key, v => v.val);

            foreach (var kv in groupedByUserNoSpecContact)
            {
                WatchDog[] wds = kv.Value;

                AspNetUser user = null;
                using (Lib.Data.DbEntities db = new DbEntities())
                {
                    user = db.AspNetUsers
                    .Where(m => m.Id == kv.Key)
                    .FirstOrDefault();
                }
                if (user == null)
                {
                    foreach (var wdtmp in wds)
                    {
                        if (wdtmp != null && wdtmp.StatusId != 0)
                        {
                            wdtmp.StatusId = 0;
                            wdtmp.Save();
                        }
                    }
                    continue;
                }
                if (user.EmailConfirmed == false)
                {
                    bool repeated = false;
                    foreach (var wdtmp in wds)
                    {
                        if (wdtmp != null && wdtmp.StatusId > 0)
                        {
                            wdtmp.DisableWDBySystem(DisabledBySystemReasons.NoConfirmedEmail, repeated);
                            repeated = true;
                        }
                    }
                    continue;
                } //user.EmailConfirmed == false
                string emailContact = user.Email;

                //process wds

                List<RenderedContent> parts = new List<RenderedContent>();
                foreach (var wd1 in wds)
                {
                    if (!(force || Tools.ReadyToRun(wd1.Period, wd1.LastSearched, DateTime.Now) ))
                        continue;


                    //specific Watchdog
                    List<IWatchdogProcessor> wdProcessorsForWD1 = wd1.GetWatchDogProcessors();

                    DateTime? fromDate = fromSpecificDate;
                    DateTime? toDate = toSpecificDate;
                    if (wd1.LatestRec.HasValue)
                        fromDate = new DateTime(wd1.LatestRec.Value.Ticks, DateTimeKind.Utc);
                    if (fromDate.HasValue == false) //because of first search (=> no .LastSearched)
                        fromDate = DateTime.Now.Date.AddMonths(-1); //from previous month


                    if (toDate.HasValue == false)
                        toDate = Tools.RoundWatchdogTime(wd1.Period, DateTime.Now);


                    List<RenderedContent> wdParts = new List<RenderedContent>();
                    foreach (var wdp in wdProcessorsForWD1)
                    {
                        wdParts.Add(wdp.RenderResults(wdp.GetResults(fromDate, toDate, 30), 5));
                    }
                    if (wdParts.Count() > 0)
                    {
                        //add watchdog header
                        RenderedContent wdtitle = new RenderedContent();
                        wdtitle.ContentHtml = "xxxx";
                        wdtitle.ContentText = "yyyy";
                        parts.Add(wdtitle);
                        parts.AddRange(wdParts);
                    }

                    if (saveWatchdogStatus)
                    {
                        wd1.LastSearched = toDate.Value;
                        wd1.Save();
                    }
                } //foreach wds

                if (parts.Count > 0)
                {
                    //send it
                    if (Email.SendEmail(emailContact, $"({DateTime.Now.ToShortDateString()}) Nové informace, co vás zajímají, na Hlídači státu", RenderedContent.Merge(parts)))
                    {
                        if (saveWatchdogStatus)
                        {
                            DateTime dt = DateTime.Now;
                            foreach (var wd in wds)
                            {
                                wd.LastSent = dt;
                                wd.Save();
                            }
                        }
                    }
                }


            } //foreach groupedByUserNoSpecContact

        }
    }
}
