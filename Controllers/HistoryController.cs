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
  public class HistoryController : ControllerBase
  {
    private UpdatesContext DbContext { get; set; }
    private readonly ILogger<HistoryController> _logger;

    public HistoryController(UpdatesContext dbContext, ILogger<HistoryController> logger = null)
    {
      DbContext = dbContext;
      _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet()]
    public async Task<IActionResult> GetHistory(string product, string version)
    {
      try
      {
        Version updateVersion = null;
        try
        {
          updateVersion = new Version(version);
        }
        catch (Exception ex)
        {

        }

        List<History> histories;
        if (updateVersion == null)
        {
          List<Update> updates = await this.DbContext.Update.Where(u => u.Product.ToLower() == product.ToLower()).ToListAsync();
          HashSet<int> versions = new HashSet<int>((from u in updates
                                                    select u.Id).ToArray());

          histories = await this.DbContext.History.Where(h => versions.Contains(h.DeliveredVersion)).ToListAsync();
        }
        else
        {
          Update update = await this.DbContext.Update.SingleOrDefaultAsync(u => u.Product.ToLower() == product.ToLower());
          if (update == null)
          {
            histories = new List<History>();
          }
          else
          {
            histories = await this.DbContext.History.Where(h => h.DeliveredVersion == update.Id).ToListAsync();
          }
        }

        return Ok(histories);
      }
      catch (Exception ex)
      {
        return StatusCode(500);
      }
    }

    [AllowAnonymous]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllHistory()
    {
      try
      {
        return Ok();
      }
      catch (Exception ex)
      {
        return StatusCode(500);
      }
    }
  }
}