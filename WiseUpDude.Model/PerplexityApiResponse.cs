using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiseUpDude.Model
{
    public class PerplexityApiResponse
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long Created { get; set; }
        public string? Model { get; set; }
        public List<Choice>? Choices { get; set; }
        public Usage? Usage { get; set; }
    }
}



