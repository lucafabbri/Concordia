// Questo file contiene test di integrazione xUnit che verificano il corretto
// funzionamento di ContextualPipelineBehavior, concentrandosi unicamente sul
// contatore condiviso e sui casi d'errore che interrompono la pipeline.

using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Concordia;
using Concordia.Behaviors;
using System.Reflection;

// Nota: per eseguire questo test, assicurati di avere installato i seguenti pacchetti NuGet:
// - xunit
// - xunit.runner.visualstudio
// - Microsoft.NET.Test.Sdk
// - MediatR (per le interfacce di base)
// - Microsoft.Extensions.DependencyInjection
// - Microsoft.Extensions.DependencyInjection.Abstractions
// - Concordia.MediatR (per il metodo AddConcordiaCoreServices)

namespace Concordia.Core.Tests;

// ---------- Definizioni necessarie per i test ----------

// Una richiesta fittizia da usare nel test di successo.
/// <summary>
/// The test request
/// </summary>
public record TestRequest : IRequest<string>;

// Una richiesta fittizia da usare nel test d'errore.
/// <summary>
/// The error request
/// </summary>
public record ErrorRequest : IRequest<string>;

// Un handler per le richieste, che è l'ultimo anello della catena.
/// <summary>
/// The test request handler class
/// </summary>
/// <seealso cref="IRequestHandler{TestRequest, string}"/>
/// <seealso cref="IRequestHandler{ErrorRequest, string}"/>
public class TestRequestHandler : IRequestHandler<TestRequest, string>, IRequestHandler<ErrorRequest, string>
{
    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the string</returns>
    public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Success");
    }

    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the string</returns>
    public Task<string> Handle(ErrorRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Should not be called");
    }
}

// Un contesto di pipeline condiviso.
/// <summary>
/// The shared test context class
/// </summary>
/// <seealso cref="ICommandPipelineContext"/>
public class SharedTestContext : ICommandPipelineContext
{
    /// <summary>
    /// Gets or sets the value of the is success
    /// </summary>
    public bool IsSuccess { get; set; } = true;
    /// <summary>
    /// Gets or sets the value of the error code
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the value of the error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the counter
    /// </summary>
    public int Counter { get; set; }

    /// <summary>
    /// Gets or sets the value of the start time
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
}

// Un'interfaccia e un'implementazione per conservare il valore finale del contatore.
/// <summary>
/// The counter holder interface
/// </summary>
public interface ICounterHolder
{
    /// <summary>
    /// Gets or sets the value of the counter
    /// </summary>
    int Counter { get; set; }
}

/// <summary>
/// The counter holder class
/// </summary>
/// <seealso cref="ICounterHolder"/>
public class CounterHolder : ICounterHolder
{
    /// <summary>
    /// Gets or sets the value of the counter
    /// </summary>
    public int Counter { get; set; }
}

// Il primo comportamento contestuale. Incrementa il contatore.
/// <summary>
/// The first contextual behavior class
/// </summary>
/// <seealso cref="ContextualPipelineBehavior{TRequest, TResponse, SharedTestContext}"/>
public class FirstContextualBehavior<TRequest, TResponse> : ContextualPipelineBehavior<TRequest, TResponse, SharedTestContext>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Ons the inbound using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected override Task OnInbound(SharedTestContext context, TRequest request, CancellationToken cancellationToken)
    {
        context.Counter++;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ons the outbound using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="response">The response</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected override Task OnOutbound(SharedTestContext context, TResponse response, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

// Il secondo comportamento contestuale. Incrementa il contatore e memorizza il valore finale.
/// <summary>
/// The second contextual behavior class
/// </summary>
/// <seealso cref="ContextualPipelineBehavior{TRequest, TResponse, SharedTestContext}"/>
public class SecondContextualBehavior<TRequest, TResponse> : ContextualPipelineBehavior<TRequest, TResponse, SharedTestContext>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// The counter holder
    /// </summary>
    private readonly ICounterHolder _counterHolder;
    /// <summary>
    /// Initializes a new instance of the <see cref="SecondContextualBehavior{TRequest,TResponse}"/> class
    /// </summary>
    /// <param name="counterHolder">The counter holder</param>
    public SecondContextualBehavior(ICounterHolder counterHolder) => _counterHolder = counterHolder;

    /// <summary>
    /// Ons the inbound using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected override Task OnInbound(SharedTestContext context, TRequest request, CancellationToken cancellationToken)
    {
        context.Counter++;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ons the outbound using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="response">The response</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected override Task OnOutbound(SharedTestContext context, TResponse response, CancellationToken cancellationToken)
    {
        // Questo è l'ultimo punto in cui il contatore è aggiornato prima della pulizia.
        // Lo salviamo per poterlo verificare nel test.
        _counterHolder.Counter = context.Counter;
        return Task.CompletedTask;
    }
}

// Un comportamento che ferma la pipeline in caso di errore.
/// <summary>
/// The error behavior class
/// </summary>
/// <seealso cref="IPipelineBehavior{TRequest, TResponse}"/>
public class ErrorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// The counter holder
    /// </summary>
    private readonly ICounterHolder _counterHolder;
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorBehavior{TRequest,TResponse}"/> class
    /// </summary>
    /// <param name="counterHolder">The counter holder</param>
    public ErrorBehavior(ICounterHolder counterHolder) => _counterHolder = counterHolder;

    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="next">The next</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the response</returns>
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Se la richiesta è di tipo ErrorRequest, interrompiamo la pipeline.
        if (request is ErrorRequest)
        {
            // Resetta il contatore per verificare che i behavior successivi non siano eseguiti.
            _counterHolder.Counter = 0;
            // Restituisce un Task con una risposta di errore, senza chiamare 'next()'.
            return Task.FromResult((TResponse)(object)"Error occurred");
        }

        // Se non è un errore, continua la pipeline normalmente.
        return next(cancellationToken);
    }
}

// Un comportamento che induce un errore nel contesto e cortocircuita la pipeline.
/// <summary>
/// The error inducing behavior class
/// </summary>
/// <seealso cref="ContextualPipelineBehavior{TRequest, TResponse, SharedTestContext}"/>
public class ErrorInducingBehavior<TRequest, TResponse> : ContextualPipelineBehavior<TRequest, TResponse, SharedTestContext>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// The counter holder
    /// </summary>
    private readonly ICounterHolder _counterHolder;
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorInducingBehavior{TRequest,TResponse}"/> class
    /// </summary>
    /// <param name="counterHolder">The counter holder</param>
    public ErrorInducingBehavior(ICounterHolder counterHolder) => _counterHolder = counterHolder;

    /// <summary>
    /// Ons the inbound using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected override Task OnInbound(SharedTestContext context, TRequest request, CancellationToken cancellationToken)
    {
        // Qui simuliamo l'errore.
        context.IsSuccess = false;
        context.ErrorMessage = "Simulated error from ErrorInducingBehavior";

        // Incrementiamo il contatore per verificare che questo comportamento venga eseguito.
        context.Counter++;
        _counterHolder.Counter = context.Counter;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Ons the outbound using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="response">The response</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected override Task OnOutbound(SharedTestContext context, TResponse response, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

// Un comportamento che dovrebbe essere saltato a causa del cortocircuito.
/// <summary>
/// The follow up behavior class
/// </summary>
/// <seealso cref="ContextualPipelineBehavior{TRequest, TResponse, SharedTestContext}"/>
public class FollowUpBehavior<TRequest, TResponse> : ContextualPipelineBehavior<TRequest, TResponse, SharedTestContext>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Ons the inbound using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected override Task OnInbound(SharedTestContext context, TRequest request, CancellationToken cancellationToken)
    {
        // Questo contatore non dovrebbe mai essere incrementato se la pipeline si interrompe.
        context.Counter++;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ons the outbound using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="response">The response</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected override Task OnOutbound(SharedTestContext context, TResponse response, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

// ---------- La classe di test xUnit ----------
/// <summary>
/// The contextual pipeline integration tests class
/// </summary>
public class ContextualPipelineIntegrationTests
{
    /// <summary>
    /// Tests that send with multiple contextual behaviors should share context and clean up
    /// </summary>
    [Fact]
    public async Task Send_WithMultipleContextualBehaviors_ShouldShareContextAndCleanUp()
    {
        // Arrange
        var services = new ServiceCollection();

        // Registrazione del Mediator e dei suoi componenti in modo esplicito e granulare.
        services.AddConcordiaCoreServices();
        services.AddSingleton<ICounterHolder, CounterHolder>();

        // Registra esplicitamente tutti i comportamenti come tipi aperti
        // Nota: l'ordine di registrazione è importante per l'esecuzione.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SecondContextualBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FirstContextualBehavior<,>));

        // Registra l'handler
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddTransient<IRequestHandler<ErrorRequest, string>, TestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var counterHolder = serviceProvider.GetRequiredService<ICounterHolder>();
        var request = new TestRequest();

        // Act
        var response = await mediator.Send(request);

        // Assert
        // 1. Verifichiamo che la risposta finale sia corretta.
        Assert.Equal("Success", response);

        // 2. Verifichiamo che il contatore sia stato incrementato correttamente.
        // Ogni behavior contestuale incrementa il contatore di 1.
        Assert.Equal(2, counterHolder.Counter);
    }

    /// <summary>
    /// Tests that send with error behavior should stop pipeline and cleanup
    /// </summary>
    [Fact]
    public async Task Send_WithErrorBehavior_ShouldStopPipelineAndCleanup()
    {
        // Arrange
        var services = new ServiceCollection();

        // Registrazione del Mediator e dei suoi componenti
        services.AddConcordiaCoreServices();
        services.AddSingleton<ICounterHolder, CounterHolder>();

        // Registra i comportamenti, incluso quello che gestisce l'errore.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SecondContextualBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FirstContextualBehavior<,>));

        // Registra l'handler
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddTransient<IRequestHandler<ErrorRequest, string>, TestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var counterHolder = serviceProvider.GetRequiredService<ICounterHolder>();
        var request = new ErrorRequest();

        // Azzeriamo il contatore prima di iniziare per essere sicuri.
        counterHolder.Counter = 0;

        // Act
        var response = await mediator.Send(request);

        // Assert
        // 1. Verifichiamo che la risposta finale sia il messaggio di errore.
        Assert.Equal("Error occurred", response);

        // 2. Verifichiamo che il contatore non sia stato incrementato,
        // poiché i behavior contestuali non sono mai stati eseguiti.
        Assert.Equal(0, counterHolder.Counter);
    }


    /// <summary>
    /// Tests that send with error in context should stop pipeline and cleanup
    /// </summary>
    [Fact]
    public async Task Send_WithErrorInContext_ShouldStopPipelineAndCleanup()
    {
        // Arrange
        var services = new ServiceCollection();

        // Registrazione del Mediator e dei suoi componenti
        services.AddConcordiaCoreServices();
        services.AddSingleton<ICounterHolder, CounterHolder>();

        // Registriamo i comportamenti nell'ordine corretto
        // Prima il comportamento che induce l'errore...
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorInducingBehavior<,>));
        // ...poi il comportamento che ci aspettiamo venga saltato.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FollowUpBehavior<,>));

        // Registra l'handler, che non dovrebbe mai essere raggiunto
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var counterHolder = serviceProvider.GetRequiredService<ICounterHolder>();
        var request = new TestRequest();

        // Azzeriamo il contatore prima di iniziare.
        counterHolder.Counter = 0;

        // Act
        var response = await mediator.Send(request);

        // 2. Verifichiamo che il contatore sia stato incrementato solo una volta,
        // dal primo comportamento. Il secondo comportamento dovrebbe essere stato saltato.
        Assert.Equal(1, counterHolder.Counter);
    }
}
