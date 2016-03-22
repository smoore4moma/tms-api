using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tms_api.Models
{
    public class GetComponentsViewModel
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
        public List<GetComponentViewModel> Components { get; set; }

    }

    public class GetComponentViewModel
    {

        public int ComponentID { get; set; }
        public string ComponentNumber { get; set; }
        public string ComponentName { get; set; }
        public string PhysDesc { get; set; }
        public string StorageComments { get; set; }
        public string InstallComments { get; set; }
        public string PrepComments { get; set; }
        public string ComponentType { get; set; }
        public int CompCount { get; set; }
        public dynamic Dimensions { get; set; }
        public dynamic Attributes { get; set; }
        public dynamic TextEntries { get; set; }

    }

}