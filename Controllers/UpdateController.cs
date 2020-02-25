using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Levrum.Utils;

using UpdateProvider.Models;

namespace UpdateProvider.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class UpdateController : ControllerBase
  {
    private UpdatesContext DbContext { get; set; }
    private readonly ILogger<UpdateController> _logger;

    public UpdateController(UpdatesContext dbContext, ILogger<UpdateController> logger = null)
    {
      DbContext = dbContext;
      _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<JsonResult> Get(string app = "", string version = "", bool latest = false)
    {
      Version versionInfo = new Version(version);
      
      Update currentUpdate = await this.DbContext.Update.SingleOrDefaultAsync(
        u => u.Product.ToLower() == app.ToLower() &&
        u.Major == versionInfo.Major &&
        u.Minor == versionInfo.Minor &&
        u.Patch == versionInfo.Patch &&
        u.Build == versionInfo.Build);

      Update update = null;
      Update latestUpdate = await this.DbContext.Update.SingleOrDefaultAsync(u => u.Product.ToLower() == app.ToLower() && u.Latest == true);
      Update specificUpdate = await this.DbContext.Update.SingleOrDefaultAsync(
        u => u.Product.ToLower() == app.ToLower() &&
        u.PreviousMajor == versionInfo.Major &&
        u.PreviousMinor == versionInfo.Minor &&
        u.PreviousPatch == versionInfo.Patch &&
        u.PreviousBuild == versionInfo.Build
      );

      update = specificUpdate == null ? latestUpdate : specificUpdate;

      if (update != null)
      {
        if (update.Major == versionInfo.Major && update.Minor == versionInfo.Minor && update.Patch == versionInfo.Patch && update.Build == versionInfo.Build)
        {
          return new JsonResult(null);
        }
        else if (IsNewerVersion(versionInfo, update))
        {
          return new JsonResult(null);
        }

        UpdateInfo info = new UpdateInfo();
        info.Name = update.Name;
        info.Description = update.Description;
        info.FileName = update.File;
        info.URL = string.Format("https://updates.levrum.com/{0}", update.File);
        info.Version = string.Format("{0}.{1}.{2}.{3}", update.Major, update.Minor, update.Patch, update.Build);

        History entry = new History();
        entry.Address = Request.HttpContext.Connection.RemoteIpAddress.ToString();
        entry.Date = DateTime.Now;
        entry.DeliveredVersion = update.Id;
        if (currentUpdate != null) {
          entry.PreviousVersion = currentUpdate.Id;
        }
        
        await this.DbContext.History.AddAsync(entry);

        return new JsonResult(info);
      }

      return new JsonResult(null);
    }

    public class Version
    {
      public int Major { get; set; } = 0;
      public int Minor { get; set; } = 0;
      public int Patch { get; set; } = 0;
      public int Build { get; set; } = 0;

      public Version(int[] versionNumbers)
      {
        if (versionNumbers.Length > 0)
        {
          Major = versionNumbers[0];
        }

        if (versionNumbers.Length > 1)
        {
          Minor = versionNumbers[1];
        }

        if (versionNumbers.Length > 2)
        {
          Patch = versionNumbers[2];
        }

        if (versionNumbers.Length > 3)
        {
          Build = versionNumbers[3];
        }
      }

      public Version(string versionString = "")
      {
        int[] versionNumbers = new int[4];
        string[] versionStrings = versionString.Split('.');

        for (var i = 0; i < 4; i++)
        {
          if (i >= versionStrings.Length || !int.TryParse(versionStrings[i], out versionNumbers[i]))
          {
            versionNumbers[i] = 0;
          }
        }

        Major = versionNumbers[0];
        Minor = versionNumbers[1];
        Patch = versionNumbers[2];
        Build = versionNumbers[3];
      }
    }

    public static bool IsNewerVersion(Version version, Update thatUpdate)
    {
      if (version.Major > thatUpdate.Major)
      {
        return true;
      }

      if (version.Major == thatUpdate.Major && version.Minor > thatUpdate.Minor)
      {
        return true;
      }

      if (version.Major == thatUpdate.Major && version.Minor == thatUpdate.Minor && version.Patch > thatUpdate.Patch)
      {
        return true;
      }

      if (version.Major == thatUpdate.Major && version.Minor == thatUpdate.Minor && version.Patch == thatUpdate.Patch && version.Build > thatUpdate.Build)
      {
        return true;
      }

      return false;
    }
  }
}
