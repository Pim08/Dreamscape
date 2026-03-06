using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dreamscape.Data
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        public string Naam { get; set; }

        public string Beschrijving { get; set; }

        public string Type { get; set; }

        public string Zeldzaamheid { get; set; }

        public int Kracht { get; set; }

        public int Snelheid { get; set; }

        public int Duurzaamheid { get; set; }

        public string MagischeEigenschap { get; set; }
    }
}