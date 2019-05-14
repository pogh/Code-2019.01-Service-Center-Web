using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class ArtikelstammSuchanbieter
    {
        public ArtikelstammSuchanbieter()
        {
            AffiliateArtikelstammAlternativPreis = new HashSet<AffiliateArtikelstammAlternativPreis>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string SrcParameter { get; set; }
        public int? GroupId { get; set; }

        public virtual ICollection<AffiliateArtikelstammAlternativPreis> AffiliateArtikelstammAlternativPreis { get; set; }
    }
}
