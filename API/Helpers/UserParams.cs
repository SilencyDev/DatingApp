using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class UserParams : Pagination
    {
		public string CurrentUsername { get; set; }
		public int MinAge { get; set; } = 18;
		public int MaxAge { get; set; } = 150;
		public string gender { get; set;}
		public string OrderBy { get; set; } = "lastActive";
    }
}
