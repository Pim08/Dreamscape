using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dreamscape.Data
{
    public class Trade
    {
        public int Id { get; set; }

        public int FromUserId { get; set; }
        public int ToUserId { get; set; }

        public int ItemOfferedId { get; set; }
        public int ItemRequestedId { get; set; }

        public string Status { get; set; } // Pending Accepted Rejected
    }
}
