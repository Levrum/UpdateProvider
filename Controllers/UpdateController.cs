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
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Levrum.Utils;

namespace UpdateProvider.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class UpdateController : ControllerBase
  {
    private readonly ILogger<UpdateController> _logger;

    public UpdateController(ILogger<UpdateController> logger = null)
    {
      _logger = logger;
    }

    [HttpGet]
    public JsonResult Get(string app, string version)
    {

      if (app == "databridge" && version != "0.0.3.100")
      {
        UpdateInfo info = new UpdateInfo();
        info.Description = "Moo";
        info.FileName = "databridge_0.0.3.99_setup.exe";
        info.URL = "http://updates.levrum.com/databridge_0.0.3.99_setup.exe";
        info.Name = "Moo";
        info.Version = "0.0.3.99";
        return new JsonResult(info);
      }
      return new JsonResult(null);
    }
  }
}
