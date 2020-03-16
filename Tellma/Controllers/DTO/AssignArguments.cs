using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// Arguments for the API of reassigning the document to another user
    /// </summary>
    public class AssignArguments : ActionArguments
    {
        /// <summary>
        /// The new assignee
        /// </summary>
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        public int AssigneeId { get; set; }

        /// <summary>
        /// Comment for the new assignee
        /// </summary>
        public string Comment { get; set; }
    }
}
