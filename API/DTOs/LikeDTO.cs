using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class LikeDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PhotoUrl { get; set; }
        public string Pseudo { get; set; }
        public string City { get; set; }
        public int Age { get; set; }
    }
}