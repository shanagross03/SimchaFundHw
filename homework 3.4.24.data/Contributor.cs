using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace homework_3._4._24.data
{
    public class Contributor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CellNumber { get; set; }
        public bool AlwaysInclude { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Balance { get; set; }
        public decimal AmountContributedToSimcha { get; set; }
    }
}
