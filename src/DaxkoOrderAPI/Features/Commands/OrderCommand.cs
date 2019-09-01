using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaxkoOrderAPI.Features.Commands
{
    public class OrderCommand
    {
        public int ItemID { get; set; }
        public int Quantity { get; set; }
    }
}
