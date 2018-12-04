﻿using System.Collections.Generic;
using AElf.Management.Interfaces;
using AElf.Management.Models;
using AElf.Management.Website.Models;
using Microsoft.AspNetCore.Mvc;

namespace AElf.Management.Website.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    public class LauncherController : ControllerBase
    {
        private readonly ILauncherService _launcherService;

        public LauncherController(ILauncherService auncherService)
        {
            _launcherService = auncherService;
        }

        [HttpGet]
        [Route("list/{chainId}")]
        public ApiResult<List<LauncherResult>> List(string chainId)
        {
            var result = _launcherService.GetAllLaunchers(chainId);
            return new ApiResult<List<LauncherResult>>(result);
        }
    }
}