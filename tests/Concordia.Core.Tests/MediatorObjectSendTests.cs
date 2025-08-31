using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using System;
using Concordia;
using Concordia;
using System.Reflection; // Added for TargetInvocationException

namespace Concordia.Core.Tests
{
    // Definizioni di richieste e handler di test per il metodo Send(object request)
    // Queste classi dovrebbero idealmente essere in file separati o in una cartella "TestHelpers"
    // all'interno del progetto di test, ma sono incluse qui per completezza dello snippet.

    public class TestObjectRequest : IRequest<string>
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestObjectRequestHandler : IRequestHandler<TestObjectRequest, string>
    {
        public Task<string> Handle(TestObjectRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {request.Message}");
        }
    }

    public class TestObjectCommand : IRequest
    {
        public string CommandMessage { get; set; } = string.Empty;
    }

    public class TestObjectCommandHandler : IRequestHandler<TestObjectCommand>
    {
        public bool WasHandled { get; private set; }
        public Task Handle(TestObjectCommand request, CancellationToken cancellationToken)
        {
            WasHandled = true;
            Console.WriteLine($"Command Handled: {request.CommandMessage}");
            return Task.CompletedTask;
        }
    }

    // Handler di supporto per testare la propagazione del CancellationToken
    public class TestObjectRequestHandlerWithCancellation : IRequestHandler<TestObjectRequest, string>
    {
        public Task<string> Handle(TestObjectRequest request, CancellationToken cancellationToken)
        {
            // Lancia un'eccezione se il token di cancellazione è stato richiesto
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult($"Handled: {request.Message}");
        }
    }


    public class MediatorObjectSendTests
    {
        [Fact]
        public async Task Send_ObjectRequestWithResponse_ShouldReturnCorrectResponse()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<IMediator, Mediator>();
            services.AddTransient<IRequestHandler<TestObjectRequest, string>, TestObjectRequestHandler>();
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            // Crea la richiesta come object
            object request = new TestObjectRequest { Message = "Hello World" };

            // Act
            var response = await mediator.Send(request);

            // Assert
            Assert.NotNull(response);
            Assert.IsType<string>(response);
            Assert.Equal("Handled: Hello World", response);
        }

        [Fact]
        public async Task Send_ObjectRequestWithoutResponse_ShouldExecuteHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<IMediator, Mediator>();
            // Usiamo un'istanza singleton dell'handler per poter verificare il suo stato dopo l'invocazione
            var handlerInstance = new TestObjectCommandHandler();
            services.AddSingleton<IRequestHandler<TestObjectCommand>, TestObjectCommandHandler>(provider => handlerInstance);
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            // Crea la richiesta come object
            object request = new TestObjectCommand { CommandMessage = "Execute this command" };

            // Act
            await mediator.Send(request);

            // Assert
            Assert.True(handlerInstance.WasHandled);
            // Verifica che il metodo restituisca null per le richieste senza risposta
            var response = await mediator.Send(request); // Rinvio per controllare il valore di ritorno
            Assert.Null(response);
        }

        [Fact]
        public async Task Send_UnregisteredObjectRequest_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<IMediator, Mediator>();
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            // Crea una richiesta per cui non è registrato alcun handler
            object request = new TestObjectRequest { Message = "This handler is not registered" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request));
            Assert.Contains("No handler found for object request of type TestObjectRequest", exception.Message);
        }

        [Fact]
        public async Task Send_ObjectRequestWithCancellationToken_ShouldPropagateToken()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<IMediator, Mediator>();
            // Registra un handler che controlla e lancia un'eccezione se il token è cancellato
            services.AddTransient<IRequestHandler<TestObjectRequest, string>, TestObjectRequestHandlerWithCancellation>();
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            object request = new TestObjectRequest { Message = "Test Cancellation" };
            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancella immediatamente il token

            // Act & Assert
            // Quando si invoca un metodo tramite reflection e questo lancia un'eccezione,
            // l'eccezione viene incapsulata in TargetInvocationException.
            // Dobbiamo quindi asserire su TargetInvocationException e poi controllare la sua InnerException.
            var targetInvocationException = await Assert.ThrowsAsync<TargetInvocationException>(() => mediator.Send(request, cts.Token));
            Assert.NotNull(targetInvocationException.InnerException);
            Assert.IsType<OperationCanceledException>(targetInvocationException.InnerException);
            Assert.Contains("operation was canceled.", targetInvocationException.InnerException.Message);
        }
    }
}
