using System;
using System.Collections.Generic;

namespace UpdateProvider.Models
{
    public partial class Update
    {
        public Update()
        {
            HistoryDeliveredVersionNavigation = new HashSet<History>();
            HistoryPreviousVersionNavigation = new HashSet<History>();
        }

        public int Id { get; set; }
        public string Product { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PreviousMajor { get; set; }
        public int PreviousMinor { get; set; }
        public int PreviousPatch { get; set; }
        public int PreviousBuild { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public int Build { get; set; }
        public string File { get; set; }
        public bool Latest { get; set; }
        public bool Beta { get; set; }

        public virtual ICollection<History> HistoryDeliveredVersionNavigation { get; set; }
        public virtual ICollection<History> HistoryPreviousVersionNavigation { get; set; }
    }
}
