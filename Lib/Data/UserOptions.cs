//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HlidacStatu.Lib.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserOptions
    {
        public int pk { get; set; }
        public string UserId { get; set; }
        public int OptionId { get; set; }
        public System.DateTime Created { get; set; }
        public string Value { get; protected set; }
        public Nullable<int> LanguageId { get; set; }
    }
}
