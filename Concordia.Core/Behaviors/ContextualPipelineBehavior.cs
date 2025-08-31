// This file contains the abstract ContextualPipelineBehavior class,
// which provides a base implementation for pipeline behaviors that
// manage a shared context. The comments have been updated to be
// in English for broader accessibility.

using Concordia;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Behaviors
{
    /// <summary>
    /// An abstract base class for pipeline behaviors that operate with a context.
    /// This class manages the creation and lifecycle of a context for each
    /// individual pipeline execution, providing a common implementation for
    /// the IPipelineBehavior's Handle method.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TContext">The type of the pipeline context, which must
    /// implement <see cref="ICommandPipelineContext"/> and have a parameterless constructor.</typeparam>
    public abstract class ContextualPipelineBehavior<TRequest, TResponse, TContext> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TContext : ICommandPipelineContext, new()
    {
        // AsyncLocal is the ideal choice in environments like ASP.NET Core because it guarantees
        // that the value is isolated for each asynchronous execution flow, such as the one
        // generated for a single HTTP request. It's not a single application-wide context,
        // but a context for each pipeline instance.
        private static readonly AsyncLocal<ICommandPipelineContext> _currentContext = new AsyncLocal<ICommandPipelineContext>();

        /// <summary>
        /// The central method that orchestrates the entire pipeline.
        /// It creates the context if it doesn't exist (i.e., if it's the first behavior in the chain),
        /// otherwise it uses the existing one. It ensures the context is cleaned up at the end.
        /// </summary>
        /// <param name="request">The request instance.</param>
        /// <param name="next">The delegate to the next step in the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response instance.</returns>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            bool isContextCreator = _currentContext.Value == null;
            TResponse response = default;

            if (isContextCreator)
            {
                // If this is the first behavior in the chain, create the context.
                _currentContext.Value = new TContext();
                _currentContext.Value.IsSuccess = true; // Assume success by default.

                try
                {
                    // Execute the inbound logic of the behavior and then the rest of the pipeline.
                    await OnInbound((TContext)_currentContext.Value, request, cancellationToken);
                    response = await next(cancellationToken);
                }
                catch (Exception ex)
                {
                    // In case of an exception, update the context with the failure state.
                    _currentContext.Value.IsSuccess = false;
                    _currentContext.Value.ErrorMessage = ex.Message;
                    // Re-throw the exception, the finally block will ensure cleanup.
                    throw;
                }
                finally
                {
                    // Execute the outbound logic with the final state of the context.
                    await OnOutbound((TContext)_currentContext.Value, response, cancellationToken);

                    // Crucial step: clean up the context to prevent it from being "leaked"
                    // and affecting other requests.
                    _currentContext.Value = null;
                }
            }
            else
            {
                // If it's a subsequent behavior, simply use the existing context.
                await OnInbound((TContext)_currentContext.Value, request, cancellationToken);
                response = await next(cancellationToken);
                await OnOutbound((TContext)_currentContext.Value, response, cancellationToken);
            }

            return response;
        }

        /// <summary>
        /// The logic to be implemented by the derived class, executed BEFORE the request
        /// is passed to the handler (inbound). The context is provided as a parameter.
        /// </summary>
        /// <param name="context">The pipeline context.</param>
        /// <param name="request">The request instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected abstract Task OnInbound(TContext context, TRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// The logic to be implemented by the derived class, executed AFTER the handler
        /// has completed (outbound). It is called regardless of success or failure.
        /// </summary>
        /// <param name="context">The pipeline context, with the final state.</param>
        /// <param name="response">The response instance (may be null in case of an exception).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected abstract Task OnOutbound(TContext context, TResponse response, CancellationToken cancellationToken);
    }
}
