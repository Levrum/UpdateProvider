using System;
using System.Collections.Generic;

namespace UpdateProvider.Models
{
    public partial class History
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Address { get; set; }
        public int? PreviousVersion { get; set; }
        public int DeliveredVersion { get; set; }

        public virtual Update DeliveredVersionNavigation { get; set; }
        public virtual Update PreviousVersionNavigation { get; set; }
    }
}
