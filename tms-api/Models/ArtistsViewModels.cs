using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tms_api.Models
{

    public class GetArtistsViewModel
    {
        /// <summary>
        /// Your Museum Name Here
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Use 2-digit ISO code.
        /// </summary>
        public string Language { get; set; }
        public int ResultsCount { get; set; }
        
        public List<GetArtistViewModel> Artists { get; set; }


    }

    /// <summary>
    /// Artist information returned from search
    /// </summary>
    public class GetArtistViewModel
    {

        public int ArtistID { get; set; }
        public string AlphaSort { get; set; }
        public string DisplayName { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public string DisplayDate { get; set; }
        public string Sex { get; set; }
        public string Nationality { get; set; }
        /// <summary>
        /// Number of objects by artist.
        /// </summary>
        public int ObjectCount { get; set; }

    }

    /// <summary>
    /// Artist information including objects.
    /// </summary>
    public class GetArtistObjectsViewModel
    {

        public int ArtistID { get; set; }
        public string AlphaSort { get; set; }
        public string DisplayName { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public string DisplayDate { get; set; }
        public string Sex { get; set; }
        public string Nationality { get; set; }
        /// <summary>
        /// Number of objects by artist.
        /// </summary>
        public int ObjectCount { get; set; }
        public List<GetAltObjectViewModel> Objects { get; set; }

    }
}