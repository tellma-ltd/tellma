using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Model.Sharding
{
    /// <summary>
    /// A record specifies that tenant X lives in Database Y
    /// </summary>
    public class Tenant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int ShardId { get; set; }

        public Shard Shard { get; set; }
    }
}
