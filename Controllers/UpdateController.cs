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
      Version currentVersion;
      try {
        currentVersion = new Version(version);
      } catch (Exception ex) {
        currentVersion = new Version(0,0,0,0);
        latest = true;
      }
      
      Update currentUpdate = await this.DbContext.Update.SingleOrDefaultAsync(
        u => u.Product.ToLower() == app.ToLower() && 
        u.Major == currentVersion.Major &&
        u.Minor == currentVersion.Minor &&
        u.Build == currentVersion.Build &&
        u.Revision == currentVersion.Revision);

      Update update = null;
      Update latestUpdate = await this.DbContext.Update.SingleOrDefaultAsync(u => u.Product.ToLower() == app.ToLower() && u.Latest == true);
      Update specificUpdate = await this.DbContext.Update.SingleOrDefaultAsync(
        u => u.Product.ToLower() == app.ToLower() && 
        u.PreviousMajor == currentVersion.Major &&
        u.PreviousMinor == currentVersion.Minor &&
        u.Build == currentVersion.Build &&
        u.Revision == currentVersion.Revision);

      update = specificUpdate == null ? latestUpdate : specificUpdate;

      if (update != null)
      {
        Version updateVersion = new Version(update.Major, update.Minor, update.Build, update.Revision);
        if (updateVersion <= currentVersion) {
          return new JsonResult(null);
        }

        UpdateInfo info = new UpdateInfo();
        info.Name = update.Name;
        info.Description = update.Description;
        info.FileName = update.File;
        info.URL = string.Format("https://updates.levrum.com/{0}", update.File);
        info.Version = string.Format("{0}.{1}.{2}.{3}", update.Major, update.Minor, update.Build, update.Revision);

        History entry = new History();
        entry.Address = Request.HttpContext.Connection.RemoteIpAddress.ToString();
        entry.Date = DateTime.Now;
        entry.DeliveredVersion = update.Id;
        if (currentUpdate != null) {
          entry.PreviousVersion = currentUpdate.Id;
        }
        
        await this.DbContext.History.AddAsync(entry);
        await this.DbContext.SaveChangesAsync();

        return new JsonResult(info);
      }

      return new JsonResult(null);
    }

    [AllowAnonymous]
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview() {
      try {
        List<History> histories = await this.DbContext.History.ToListAsync();
        Dictionary<int, int> downloadCounts = new Dictionary<int, int>();
        foreach (History history in histories) {
          if (!downloadCounts.ContainsKey(history.DeliveredVersion)) {
            downloadCounts[history.DeliveredVersion] = 1;
          } else {
            downloadCounts[history.DeliveredVersion] += 1;
          }
        }

        var sortedHistories = from pair in downloadCounts
                              orderby pair.Value descending
                              select pair;

        List<OverviewInfo> output = new List<OverviewInfo>();
        foreach (KeyValuePair<int, int> pair in sortedHistories) {
          if (output.Count >= 10) {
            break;
          }

          Update update = await this.DbContext.Update.SingleOrDefaultAsync(u => u.Id == pair.Key);
          OverviewInfo info = new OverviewInfo(update, pair.Value);
          output.Add(info);
        }

        return Ok(output);
      } catch (Exception ex) {
        return StatusCode(500);
      }
    }

    [AllowAnonymous]
    [HttpGet("list")]
    public async Task<IActionResult> GetList() {
      try {
        List<Update> updates = await this.DbContext.Update.ToListAsync();
        List<OverviewInfo> output = new List<OverviewInfo>();

        foreach (Update update in updates) {
          output.Add(new OverviewInfo(update, -1));
        }

        return Ok(output);
      } catch (Exception ex) {
        return StatusCode(500);
      }
    }

    [AllowAnonymous]
    [HttpPost("add")]
    [RequestSizeLimit(1_000_000_000)]
    public async Task<IActionResult> AddUpdate(IFormFile file, string product, string name, string description, string fileName, string targetVersion, string newVersion, bool latest, bool beta) {
      try {
        if (file.Length == 0) {
          throw new Exception("File length is 0");
        }

        fileName = string.IsNullOrWhiteSpace(fileName) ? file.Name : fileName;

        using (var fs = new FileStream(string.Format("./file/{0}", fileName), FileMode.Create)) {
          await file.CopyToAsync(fs);
        }

        Update update = new Update();
        update.Product = product;
        update.Name = name;
        update.Description = description;
        update.File = fileName;
        
        try {
          Version target = new Version(targetVersion);
          update.PreviousMajor = target.Major;
          update.PreviousMinor = target.Minor;
          update.PreviousBuild = target.Build;
          update.PreviousRevision = target.Revision;
        } catch (Exception ex) {
          update.PreviousMajor = 0;
          update.PreviousMinor = 0;
          update.PreviousBuild = 0;
          update.PreviousRevision = 0;
        }

        try {
          Version version = new Version(newVersion);
          update.Major = version.Major;
          update.Minor = version.Minor;
          update.Build = version.Build;
          update.Revision = version.Revision;
        } catch (Exception ex) {
          return StatusCode(500);
        }

        update.Latest = latest;
        update.Beta = beta;
        
        await this.DbContext.Update.AddAsync(update);
        await this.DbContext.SaveChangesAsync();

        return Ok();
      } catch (Exception ex) {
        return StatusCode(500);
      }
    }
  }
}
