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
  public class UserController : ControllerBase
  {
    private UpdatesContext DbContext { get; set; }
    private readonly ILogger<UserController> _logger;

    public UserController(UpdatesContext dbContext, ILogger<UserController> logger = null)
    {
      DbContext = dbContext;
      _logger = logger;
    }
  }
}