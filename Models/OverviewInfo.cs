using System;
using System.IO;

namespace UpdateProvider.Models
{
  public class OverviewInfo
  {
    public string Product { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Version { get; set; } = "";
    public string Target { get; set; } = "";
    public string File { get; set; } = "";
    public long Size { get; set; } = 0;
    public string URI { get; set; } = "";
    public bool Latest { get; set; } = false;
    public bool Beta { get; set; } = false;
    public int Downloads { get; set; } = 0;

    public OverviewInfo(Update update, int downloads = 0)
    {
      Product = update.Product;
      Name = update.Name;
      Description = update.Description;
      Version = string.Format("{0}.{1}.{2}.{3}", update.Major, update.Minor, update.Build, update.Revision);
      Target = string.Format("{0}.{1}.{2}.{3}", update.PreviousMajor, update.PreviousMinor, update.PreviousBuild, update.PreviousRevision);
      File = update.File;
      FileInfo info = new FileInfo(string.Format("./files/{0}", File));
      if (info.Exists)
        Size = info.Length;

      URI = string.Format("https://updates.levrum.com/{0}", File);
      Latest = update.Latest;
      Beta = update.Beta;
      Downloads = downloads;
    }
  }
}