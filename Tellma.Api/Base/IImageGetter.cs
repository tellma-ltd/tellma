using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Api.Base
{
    public interface IImageGetter
    {
        public Task<ImageResult> GetImage(int id, CancellationToken cancellation);
    }
}
