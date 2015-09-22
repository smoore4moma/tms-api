using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tms_api.Models
{

    public class GetExhibitionsViewModel
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
        public List<GetExhibitionViewModel> Exhibitions { get; set; }

    }

    public class GetExhibitionsObjectsViewModel
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
        public List<GetExhibitionObjectsViewModel> Exhibitions { get; set; }

    }

    public class GetExhibitionViewModel
    {
        public int ExhibitionID { get; set; }
        public string ProjectNumber { get; set; }
        public string ExhibitionTitle { get; set; }
        public string Department { get; set; }
        public string ExhibitionDisplayDate { get; set; }
        public string ExhibitionBeginDate { get; set; }
        public string ExhibitionEndDate { get; set; }
        /// <summary>
        /// Number of objects in exhibition.
        /// </summary>
        public int ObjectCount { get; set; }

    }

    public class GetExhibitionObjectsViewModel
    {
        public int ExhibitionID { get; set; }
        public string ProjectNumber { get; set; }
        public string ExhibitionTitle { get; set; }
        public string Department { get; set; }
        public string ExhibitionDisplayDate { get; set; }
        public string ExhibitionBeginDate { get; set; }
        public string ExhibitionEndDate { get; set; }
        /// <summary>
        /// Number of objects in exhibition.
        /// </summary>
        public int ObjectCount { get; set; }
        public List<GetAltObjectViewModel> Objects { get; set; }

    }
}