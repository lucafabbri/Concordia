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

    /// <summary>
    /// The test object request class
    /// </summary>
    /// <seealso cref="IRequest{string}"/>
    public class TestObjectRequest : IRequest<string>
    {
        /// <summary>
        /// Gets or sets the value of the message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// The test object request handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{TestObjectRequest, string}"/>
    public class TestObjectRequestHandler : IRequestHandler<TestObjectRequest, string>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the string</returns>
        public Task<string> Handle(TestObjectRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {request.Message}");
        }
    }

    /// <summary>
    /// The test object command class
    /// </summary>
    /// <seealso cref="IRequest"/>
    public class TestObjectCommand : IRequest
    {
        /// <summary>
        /// Gets or sets the value of the command message
        /// </summary>
        public string CommandMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// The test object command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{TestObjectCommand}"/>
    public class TestObjectCommandHandler : IRequestHandler<TestObjectCommand>
    {
        /// <summary>
        /// Gets or sets the value of the was handled
        /// </summary>
        public bool WasHandled { get; private set; }
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(TestObjectCommand request, CancellationToken cancellationToken)
        {
            WasHandled = true;
            Console.WriteLine($"Command Handled: {request.CommandMessage}");
            return Task.CompletedTask;
        }
    }

    // Handler di supporto per testare la propagazione del CancellationToken
    /// <summary>
    /// The test object request handler with cancellation class
    /// </summary>
    /// <seealso cref="IRequestHandler{TestObjectRequest, string}"/>
    public class TestObjectRequestHandlerWithCancellation : IRequestHandler<TestObjectRequest, string>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the string</returns>
        public Task<string> Handle(TestObjectRequest request, CancellationToken cancellationToken)
        {
            // Lancia un'eccezione se il token di cancellazione è stato richiesto
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult($"Handled: {request.Message}");
        }
    }


    /// <summary>
    /// The mediator object send tests class
    /// </summary>
    public class MediatorObjectSendTests
    {
        /// <summary>
        /// Tests that send object request with response should return correct response
        /// </summary>
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

        /// <summary>
        /// Tests that send object request without response should execute handler
        /// </summary>
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

        /// <summary>
        /// Tests that send unregistered object request should throw invalid operation exception
        /// </summary>
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

        /// <summary>
        /// Tests that send object request with cancellation token should propagate token
        /// </summary>
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
