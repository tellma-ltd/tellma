using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Entities.Descriptors;

namespace Tellma.Controllers
{
    [Route("api/ping")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public ActionResult Ping()
        {
            // Used by clients to check if they are online
            return Ok();
        }

        [HttpGet("sql")]
        public ActionResult Sql([FromQuery] GetArguments args)
        {
            StringBuilder output = new StringBuilder();

            try
            {
                // Get the parameters
                var selectExps = QueryexBase.Parse(args.Select);
                var orderByExps = QueryexBase.Parse(args.OrderBy, true);
                var filterExps = QueryexBase.Parse(args.Filter);

                // Extract all paths
                var allPaths = selectExps.SelectMany(e => e?.ColumnAccesses() ?? new List<QueryexColumnAccess>()).Select(e => e.Path)
                    .Concat(orderByExps.SelectMany(e => e?.ColumnAccesses() ?? new List<QueryexColumnAccess>()).Select(e => e.Path))
                    .Concat(filterExps.SelectMany(e => e?.ColumnAccesses() ?? new List<QueryexColumnAccess>()).Select(e => e.Path));

                // Prepare CTX
                var joinTrie = JoinTrie.Make(TypeDescriptor.Get<DetailsEntry>(), allPaths);
                var ps = new SqlStatementParameters();
                var vars = new SqlStatementVariables();
                var userId = 25;
                var today = DateTime.Today;
                var ctx = new QxCompilationContext(joinTrie, ApplicationRepository.Sources, vars, ps, today, userId);


                output.AppendLine("[Select]");
                foreach (var exp in selectExps.Where(e => e!= null))
                    output.AppendLine(exp.CompileToNonBoolean(ctx));
                output.AppendLine();

                output.AppendLine("[Join]");
                output.AppendLine(joinTrie.GetSql(ApplicationRepository.Sources, null));
                output.AppendLine();

                output.AppendLine("[OrderBy]");
                foreach (var exp in orderByExps.Where(e => e != null))
                    output.AppendLine(exp.CompileToNonBoolean(ctx) + (exp.Direction == QxDirection.None ? "" : " " + exp.Direction.ToString().ToUpper()));
                output.AppendLine();

                output.AppendLine("[Filter]");
                foreach (var exp in filterExps.Where(e => e != null))
                    output.AppendLine(exp.CompileToBoolean(ctx));
                output.AppendLine();


                output.AppendLine("----------------------------------");
                output.AppendLine("[Parameters]");
                foreach (var v in ctx.Parameters)
                    output.AppendLine($"@{v.ParameterName} = {v.Value};");
                output.AppendLine();

                output.AppendLine("[Variables]");
                foreach (var v in ctx.Variables)
                    output.AppendLine($"DECLARE @{v.Name} {v.Type} = {v.Definition};");
            }
            catch (QueryException ex)
            {
                output.Append("QueryException: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Catastrophic
                output.Append("Exception: " + ex.Message);
            }

            return Content(output.ToString());
        }
    }
}
