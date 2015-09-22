using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tms_api.Models
{

    public class GetObjectsViewModel
    {
        /// <summary>
        /// The Museum of Modern Art (MoMA)
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Use 2-digit ISO code.
        /// </summary>
        public string Language { get; set; }

        public int ResultsCount { get; set; }

        public List<GetObjectViewModel> Objects { get; set; }

    }

    public class GetObjectViewModel
    {
        public string ObjectNumber { get; set; }
        public int ObjectID { get; set; }
        public string Title { get; set; }
        public string DisplayName { get; set; }
        public string AlphaSort { get; set; }
        public int ArtistID { get; set; }
        public string DisplayDate { get; set; }
        public string Dated { get; set; }
        public int DateBegin { get; set; }
        public int DateEnd { get; set; }
        public string Medium { get; set; }
        public string Dimensions { get; set; }
        public string Department { get; set; }
        public string Classification { get; set; }
        public int OnView { get; set; }
        public string Provenance { get; set; }
        public string Description { get; set; }
        public int ObjectStatusID { get; set; }
        public string CreditLine { get; set; }
        public string ImageID { get; set; }
        public string Thumbnail { get; set; }
        public string FullImage { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public GetExhibitionsViewModel Exhibitions { get; set; }
        public GetTermsViewModel Terms { get; set; }
    }

    // This is set up this way to avoid circular references, which causes errors on the help pages
    public class GetAltObjectViewModel
    {
        public string ObjectNumber { get; set; }
        public int ObjectID { get; set; }
        public string Title { get; set; }
        public string DisplayName { get; set; }
        public string AlphaSort { get; set; }
        public int ArtistID { get; set; }
        public string DisplayDate { get; set; }
        public string Dated { get; set; }
        public int DateBegin { get; set; }
        public int DateEnd { get; set; }
        public string Medium { get; set; }
        public string Dimensions { get; set; }
        public string Department { get; set; }
        public string Classification { get; set; }
        public int OnView { get; set; }
        public string Provenance { get; set; }
        public string Description { get; set; }
        public int ObjectStatusID { get; set; }
        public string CreditLine { get; set; }
        public string ImageID { get; set; }
        public string Thumbnail { get; set; }
        public string FullImage { get; set; }
        public DateTime LastModifiedDate { get; set; }

    }

}