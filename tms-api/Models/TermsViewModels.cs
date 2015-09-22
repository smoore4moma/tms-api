using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tms_api.Models
{
    /// <summary>
    /// Lists terms associated with objects.
    /// </summary>
    public class GetTermsViewModel
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
        public List<GetTermViewModel> Terms { get; set; }

    }

    /// <summary>
    /// The term, type, and count of objects 
    /// </summary>
    public class GetTermViewModel
    {

        public int TermID { get; set; }
        public string Term { get; set; }
        /// <summary>
        /// Currently limited to a select number of term types.
        /// </summary>
        public string TermType { get; set; }
        /// <summary>
        /// The number of objects associated with this term.
        /// </summary>
        public int TermCount { get; set; }


    }
}