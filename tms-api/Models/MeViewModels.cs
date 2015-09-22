using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tms_api.Models
{
    // Models returned by MeController actions.
    public class GetViewModel
    {
        public string Token { get; set; }
    }
}