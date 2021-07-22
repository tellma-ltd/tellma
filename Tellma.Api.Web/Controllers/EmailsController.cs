﻿using Microsoft.AspNetCore.Mvc;
using System;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/emails")]
    [ApplicationController]
    public class EmailsController : FactGetByIdControllerBase<EmailForQuery, int>
    {
        public const string BASE_ADDRESS = "emails";

        private readonly EmailsService _service;

        public EmailsController(EmailsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<EmailForQuery, int> GetFactGetByIdService()
        {
            return _service;
        }
    }
}
