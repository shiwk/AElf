﻿using System.Collections.Generic;
using AElf.Management.Interfaces;
using AElf.Management.Models;
using AElf.Management.Website.Models;
using Microsoft.AspNetCore.Mvc;

namespace AElf.Management.Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        private readonly IWorkerService _workerService;

        public WorkerController(IWorkerService workerService)
        {
            _workerService = workerService;
        }

        [HttpGet]
        [Route("list/{chainId}")]
        public ApiResult<List<WorkerResult>> List(string chainId)
        {
            var result = _workerService.GetAllWorkers(chainId);
            return new ApiResult<List<WorkerResult>>(result);
        }
    }
}