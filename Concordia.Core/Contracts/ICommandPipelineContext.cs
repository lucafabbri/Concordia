using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concordia.Contracts;

/// <summary>
/// The base interface for a command pipeline context, now including
/// properties for tracking the success or failure of the pipeline.
/// </summary>
public interface ICommandPipelineContext
{
    /// <summary>
    /// Gets the timestamp when the pipeline started.
    /// </summary>
    DateTime StartTime { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the pipeline execution was successful.
    /// </summary>
    bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets an optional error code in case of failure.
    /// </summary>
    string ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets an optional error message in case of failure.
    /// </summary>
    string ErrorMessage { get; set; }
}
