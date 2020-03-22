using System;    
    
namespace UpdateProvider.Models {
    public class HistoryInfo {
      public DateTime Time { get; set; }
      public string Address { get; set; }
      public OverviewInfo RequesterVersion { get; set; }
      public OverviewInfo DeliveredVersion { get; set; }
    }
}