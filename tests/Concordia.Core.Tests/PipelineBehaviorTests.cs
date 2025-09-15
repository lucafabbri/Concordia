using Xunit;
using Concordia;
using Concordia.Behaviors;
using Concordia.Core.Behaviors;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Concordia.Core.Tests;

/// <summary>
/// Tests for LoggingBehavior
/// </summary>
public class LoggingBehaviorTests
{
    /// <summary>
    /// Test request for pipeline behavior tests
    /// </summary>
    public class TestRequest : IRequest<string>
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tests that LoggingBehavior logs request handling and calls next delegate
    /// </summary>
    [Fact]
    public async Task Handle_ShouldLogRequestAndCallNext()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, string>();
        var request = new TestRequest { Message = "Test" };
        var expectedResponse = "Test Response";
        var nextCalled = false;
        
        RequestHandlerDelegate<string> next = ct =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Capture console output
        var stringWriter = new StringWriter();
        var originalConsoleOut = Console.Out;
        Console.SetOut(stringWriter);

        try
        {
            // Act
            var response = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Equal(expectedResponse, response);
            Assert.True(nextCalled);
            
            var output = stringWriter.ToString();
            Assert.Contains("--- Handling Request: TestRequest ---", output);
            Assert.Contains("--- Handled Request: TestRequest - Response Type: String ---", output);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    /// <summary>
    /// Tests that LoggingBehavior propagates exceptions from next delegate
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPropagateExceptionsFromNext()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, string>();
        var request = new TestRequest { Message = "Test" };
        var expectedException = new InvalidOperationException("Test exception");
        
        RequestHandlerDelegate<string> next = ct => throw expectedException;

        // Capture console output
        var stringWriter = new StringWriter();
        var originalConsoleOut = Console.Out;
        Console.SetOut(stringWriter);

        try
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                behavior.Handle(request, next, CancellationToken.None));
            
            Assert.Same(expectedException, exception);
            
            var output = stringWriter.ToString();
            Assert.Contains("--- Handling Request: TestRequest ---", output);
            // Should not contain the "Handled" message since an exception was thrown
            Assert.DoesNotContain("--- Handled Request: TestRequest - Response Type: String ---", output);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    /// <summary>
    /// Tests that LoggingBehavior passes cancellation token correctly
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToNext()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, string>();
        var request = new TestRequest { Message = "Test" };
        var cancellationToken = new CancellationToken();
        CancellationToken receivedToken = default;
        
        RequestHandlerDelegate<string> next = ct =>
        {
            receivedToken = ct;
            return Task.FromResult("Response");
        };

        // Capture console output to avoid cluttering test output
        var stringWriter = new StringWriter();
        var originalConsoleOut = Console.Out;
        Console.SetOut(stringWriter);

        try
        {
            // Act
            await behavior.Handle(request, next, cancellationToken);

            // Assert
            Assert.Equal(cancellationToken, receivedToken);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }
}

/// <summary>
/// Tests for RequestPreProcessorBehavior
/// </summary>
public class RequestPreProcessorBehaviorTests
{
    /// <summary>
    /// Test request for pre-processor behavior tests
    /// </summary>
    public class TestRequest : IRequest<string>
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test pre-processor implementation
    /// </summary>
    public class TestPreProcessor : IRequestPreProcessor<TestRequest>
    {
        public List<TestRequest> ProcessedRequests { get; } = new();
        public List<CancellationToken> ReceivedTokens { get; } = new();

        public Task Process(TestRequest request, CancellationToken cancellationToken)
        {
            ProcessedRequests.Add(request);
            ReceivedTokens.Add(cancellationToken);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Tests that RequestPreProcessorBehavior executes all pre-processors and calls next
    /// </summary>
    [Fact]
    public async Task Handle_ShouldExecuteAllPreProcessorsAndCallNext()
    {
        // Arrange
        var preProcessor1 = new TestPreProcessor();
        var preProcessor2 = new TestPreProcessor();
        var preProcessors = new List<IRequestPreProcessor<TestRequest>> { preProcessor1, preProcessor2 };
        
        var behavior = new RequestPreProcessorBehavior<TestRequest, string>(preProcessors);
        var request = new TestRequest { Message = "Test" };
        var expectedResponse = "Test Response";
        var nextCalled = false;
        
        RequestHandlerDelegate<string> next = ct =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var response = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, response);
        Assert.True(nextCalled);
        
        // Verify both pre-processors were called
        Assert.Single(preProcessor1.ProcessedRequests);
        Assert.Same(request, preProcessor1.ProcessedRequests[0]);
        Assert.Single(preProcessor2.ProcessedRequests);
        Assert.Same(request, preProcessor2.ProcessedRequests[0]);
    }

    /// <summary>
    /// Tests that RequestPreProcessorBehavior works with no pre-processors
    /// </summary>
    [Fact]
    public async Task Handle_WithNoPreProcessors_ShouldCallNext()
    {
        // Arrange
        var preProcessors = new List<IRequestPreProcessor<TestRequest>>();
        var behavior = new RequestPreProcessorBehavior<TestRequest, string>(preProcessors);
        var request = new TestRequest { Message = "Test" };
        var expectedResponse = "Test Response";
        var nextCalled = false;
        
        RequestHandlerDelegate<string> next = ct =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var response = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, response);
        Assert.True(nextCalled);
    }

    /// <summary>
    /// Tests that RequestPreProcessorBehavior propagates pre-processor exceptions
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPropagatePreProcessorExceptions()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var failingPreProcessor = new FailingPreProcessor(expectedException);
        var preProcessors = new List<IRequestPreProcessor<TestRequest>> { failingPreProcessor };
        
        var behavior = new RequestPreProcessorBehavior<TestRequest, string>(preProcessors);
        var request = new TestRequest { Message = "Test" };
        
        RequestHandlerDelegate<string> next = ct => Task.FromResult("Should not be called");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(request, next, CancellationToken.None));
        
        Assert.Same(expectedException, exception);
    }

    /// <summary>
    /// Tests that RequestPreProcessorBehavior passes cancellation token to pre-processors
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToPreProcessors()
    {
        // Arrange
        var preProcessor = new TestPreProcessor();
        var preProcessors = new List<IRequestPreProcessor<TestRequest>> { preProcessor };
        var behavior = new RequestPreProcessorBehavior<TestRequest, string>(preProcessors);
        var request = new TestRequest { Message = "Test" };
        var cancellationToken = new CancellationToken();
        
        RequestHandlerDelegate<string> next = ct => Task.FromResult("Response");

        // Act
        await behavior.Handle(request, next, cancellationToken);

        // Assert
        Assert.Single(preProcessor.ReceivedTokens);
        Assert.Equal(cancellationToken, preProcessor.ReceivedTokens[0]);
    }

    /// <summary>
    /// Failing pre-processor for testing exception scenarios
    /// </summary>
    public class FailingPreProcessor : IRequestPreProcessor<TestRequest>
    {
        private readonly Exception _exception;

        public FailingPreProcessor(Exception exception)
        {
            _exception = exception;
        }

        public Task Process(TestRequest request, CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }
}

/// <summary>
/// Tests for RequestPostProcessorBehavior
/// </summary>
public class RequestPostProcessorBehaviorTests
{
    /// <summary>
    /// Test request for post-processor behavior tests
    /// </summary>
    public class TestRequest : IRequest<string>
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test post-processor implementation
    /// </summary>
    public class TestPostProcessor : IRequestPostProcessor<TestRequest, string>
    {
        public List<(TestRequest Request, string Response)> ProcessedItems { get; } = new();
        public List<CancellationToken> ReceivedTokens { get; } = new();

        public Task Process(TestRequest request, string response, CancellationToken cancellationToken)
        {
            ProcessedItems.Add((request, response));
            ReceivedTokens.Add(cancellationToken);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Tests that RequestPostProcessorBehavior calls next first, then executes all post-processors
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCallNextThenExecuteAllPostProcessors()
    {
        // Arrange
        var postProcessor1 = new TestPostProcessor();
        var postProcessor2 = new TestPostProcessor();
        var postProcessors = new List<IRequestPostProcessor<TestRequest, string>> { postProcessor1, postProcessor2 };
        
        var behavior = new RequestPostProcessorBehavior<TestRequest, string>(postProcessors);
        var request = new TestRequest { Message = "Test" };
        var expectedResponse = "Test Response";
        var nextCalled = false;
        
        RequestHandlerDelegate<string> next = ct =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var response = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, response);
        Assert.True(nextCalled);
        
        // Verify both post-processors were called with correct parameters
        Assert.Single(postProcessor1.ProcessedItems);
        Assert.Equal(request, postProcessor1.ProcessedItems[0].Request);
        Assert.Equal(expectedResponse, postProcessor1.ProcessedItems[0].Response);
        
        Assert.Single(postProcessor2.ProcessedItems);
        Assert.Equal(request, postProcessor2.ProcessedItems[0].Request);
        Assert.Equal(expectedResponse, postProcessor2.ProcessedItems[0].Response);
    }

    /// <summary>
    /// Tests that RequestPostProcessorBehavior works with no post-processors
    /// </summary>
    [Fact]
    public async Task Handle_WithNoPostProcessors_ShouldCallNext()
    {
        // Arrange
        var postProcessors = new List<IRequestPostProcessor<TestRequest, string>>();
        var behavior = new RequestPostProcessorBehavior<TestRequest, string>(postProcessors);
        var request = new TestRequest { Message = "Test" };
        var expectedResponse = "Test Response";
        var nextCalled = false;
        
        RequestHandlerDelegate<string> next = ct =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var response = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, response);
        Assert.True(nextCalled);
    }

    /// <summary>
    /// Tests that RequestPostProcessorBehavior propagates next delegate exceptions (post-processors not called)
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPropagateNextExceptions()
    {
        // Arrange
        var postProcessor = new TestPostProcessor();
        var postProcessors = new List<IRequestPostProcessor<TestRequest, string>> { postProcessor };
        var behavior = new RequestPostProcessorBehavior<TestRequest, string>(postProcessors);
        var request = new TestRequest { Message = "Test" };
        var expectedException = new InvalidOperationException("Test exception");
        
        RequestHandlerDelegate<string> next = ct => throw expectedException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(request, next, CancellationToken.None));
        
        Assert.Same(expectedException, exception);
        Assert.Empty(postProcessor.ProcessedItems); // Post-processors should not be called
    }

    /// <summary>
    /// Tests that RequestPostProcessorBehavior propagates post-processor exceptions
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPropagatePostProcessorExceptions()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var failingPostProcessor = new FailingPostProcessor(expectedException);
        var postProcessors = new List<IRequestPostProcessor<TestRequest, string>> { failingPostProcessor };
        
        var behavior = new RequestPostProcessorBehavior<TestRequest, string>(postProcessors);
        var request = new TestRequest { Message = "Test" };
        
        RequestHandlerDelegate<string> next = ct => Task.FromResult("Response");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(request, next, CancellationToken.None));
        
        Assert.Same(expectedException, exception);
    }

    /// <summary>
    /// Tests that RequestPostProcessorBehavior passes cancellation token to post-processors
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToPostProcessors()
    {
        // Arrange
        var postProcessor = new TestPostProcessor();
        var postProcessors = new List<IRequestPostProcessor<TestRequest, string>> { postProcessor };
        var behavior = new RequestPostProcessorBehavior<TestRequest, string>(postProcessors);
        var request = new TestRequest { Message = "Test" };
        var cancellationToken = new CancellationToken();
        
        RequestHandlerDelegate<string> next = ct => Task.FromResult("Response");

        // Act
        await behavior.Handle(request, next, cancellationToken);

        // Assert
        Assert.Single(postProcessor.ReceivedTokens);
        Assert.Equal(cancellationToken, postProcessor.ReceivedTokens[0]);
    }

    /// <summary>
    /// Failing post-processor for testing exception scenarios
    /// </summary>
    public class FailingPostProcessor : IRequestPostProcessor<TestRequest, string>
    {
        private readonly Exception _exception;

        public FailingPostProcessor(Exception exception)
        {
            _exception = exception;
        }

        public Task Process(TestRequest request, string response, CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }
}