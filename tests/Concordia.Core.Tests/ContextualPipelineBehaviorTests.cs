// Questo file contiene test di integrazione xUnit che verificano il corretto
// funzionamento di ContextualPipelineBehavior, concentrandosi unicamente sul
// contatore condiviso e sui casi d'errore che interrompono la pipeline.

using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Concordia.Contracts;
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
public record TestRequest : IRequest<string>;

// Una richiesta fittizia da usare nel test d'errore.
public record ErrorRequest : IRequest<string>;

// Un handler per le richieste, che è l'ultimo anello della catena.
public class TestRequestHandler : IRequestHandler<TestRequest, string>, IRequestHandler<ErrorRequest, string>
{
    public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Success");
    }

    public Task<string> Handle(ErrorRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Should not be called");
    }
}

// Un contesto di pipeline condiviso.
public class SharedTestContext : ICommandPipelineContext
{
    public bool IsSuccess { get; set; } = true;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public int Counter { get; set; }

    public DateTime StartTime { get; set; } = DateTime.UtcNow;
}

// Un'interfaccia e un'implementazione per conservare il valore finale del contatore.
public interface ICounterHolder
{
    int Counter { get; set; }
}

public class CounterHolder : ICounterHolder
{
    public int Counter { get; set; }
}

// Il primo comportamento contestuale. Incrementa il contatore.
public class FirstContextualBehavior<TRequest, TResponse> : ContextualPipelineBehavior<TRequest, TResponse, SharedTestContext>
    where TRequest : IRequest<TResponse>
{
    protected override Task OnInbound(SharedTestContext context, TRequest request, CancellationToken cancellationToken)
    {
        context.Counter++;
        return Task.CompletedTask;
    }

    protected override Task OnOutbound(SharedTestContext context, TResponse response, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

// Il secondo comportamento contestuale. Incrementa il contatore e memorizza il valore finale.
public class SecondContextualBehavior<TRequest, TResponse> : ContextualPipelineBehavior<TRequest, TResponse, SharedTestContext>
    where TRequest : IRequest<TResponse>
{
    private readonly ICounterHolder _counterHolder;
    public SecondContextualBehavior(ICounterHolder counterHolder) => _counterHolder = counterHolder;

    protected override Task OnInbound(SharedTestContext context, TRequest request, CancellationToken cancellationToken)
    {
        context.Counter++;
        return Task.CompletedTask;
    }

    protected override Task OnOutbound(SharedTestContext context, TResponse response, CancellationToken cancellationToken)
    {
        // Questo è l'ultimo punto in cui il contatore è aggiornato prima della pulizia.
        // Lo salviamo per poterlo verificare nel test.
        _counterHolder.Counter = context.Counter;
        return Task.CompletedTask;
    }
}

// Un comportamento che ferma la pipeline in caso di errore.
public class ErrorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICounterHolder _counterHolder;
    public ErrorBehavior(ICounterHolder counterHolder) => _counterHolder = counterHolder;

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
public class ErrorInducingBehavior<TRequest, TResponse> : ContextualPipelineBehavior<TRequest, TResponse, SharedTestContext>
    where TRequest : IRequest<TResponse>
{
    private readonly ICounterHolder _counterHolder;
    public ErrorInducingBehavior(ICounterHolder counterHolder) => _counterHolder = counterHolder;

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

    protected override Task OnOutbound(SharedTestContext context, TResponse response, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

// Un comportamento che dovrebbe essere saltato a causa del cortocircuito.
public class FollowUpBehavior<TRequest, TResponse> : ContextualPipelineBehavior<TRequest, TResponse, SharedTestContext>
    where TRequest : IRequest<TResponse>
{
    protected override Task OnInbound(SharedTestContext context, TRequest request, CancellationToken cancellationToken)
    {
        // Questo contatore non dovrebbe mai essere incrementato se la pipeline si interrompe.
        context.Counter++;
        return Task.CompletedTask;
    }

    protected override Task OnOutbound(SharedTestContext context, TResponse response, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

// ---------- La classe di test xUnit ----------
public class ContextualPipelineIntegrationTests
{
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
