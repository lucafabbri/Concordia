using BenchmarkDotNet.Attributes;
using Concordia.Benchmarks.Generated;
using Concordia.Benchmarks.Handlers;
using Concordia.Benchmarks.Requests;
using MediatR;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using MartinIMediator = Mediator.IMediator;

namespace Concordia.Benchmarks;

[MemoryDiagnoser]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class PublishNotificationBenchmark
{
    private Concordia.IMediator _concordiaMediator = null!;
    private Concordia.IMediator _concordiaGenMediator = null!;
    private MediatR.IMediator _mediatRMediator = null!;
    private MartinIMediator _martinMediator = null!;

    private readonly ConcordiaNotification _concordiaNotification = new() { Id = 1 };
    private readonly MediatRNotification _mediatRNotification = new() { Id = 1 };
    private readonly MartinNotification _martinNotification = new() { Id = 1 };

    [GlobalSetup]
    public void Setup()
    {
        // --- Concordia (wrapper-caching) setup ---
        var concordiaServices = new ServiceCollection();
        concordiaServices.AddSingleton<IMediator, Concordia.Mediator>();
        concordiaServices.AddSingleton<ISender, Concordia.Mediator>();
        concordiaServices.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
        concordiaServices.AddSingleton<INotificationHandler<ConcordiaNotification>, ConcordiaNotificationHandler1>();
        concordiaServices.AddSingleton<INotificationHandler<ConcordiaNotification>, ConcordiaNotificationHandler2>();
        _concordiaMediator = concordiaServices.BuildServiceProvider().GetRequiredService<Concordia.IMediator>();

        // --- Concordia (source-generated) setup ---
        var genServices = new ServiceCollection();
        genServices.AddConcordiaHandlers();
        _concordiaGenMediator = genServices.BuildServiceProvider().GetRequiredService<Concordia.IMediator>();

        // --- MediatR setup ---
        var mediatRServices = new ServiceCollection();
        mediatRServices.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(MediatRNotificationHandler1).Assembly));
        _mediatRMediator = mediatRServices.BuildServiceProvider().GetRequiredService<MediatR.IMediator>();

        // --- martinothamar/Mediator setup ---
        var martinServices = new ServiceCollection();
        martinServices.AddMediator(opts => opts.ServiceLifetime = ServiceLifetime.Singleton);
        _martinMediator = martinServices.BuildServiceProvider().GetRequiredService<MartinIMediator>();
    }

    [Benchmark(Baseline = true)]
    public Task MediatR_PublishNotification()
        => _mediatRMediator.Publish(_mediatRNotification);

    [Benchmark]
    public Task Concordia_PublishNotification()
        => _concordiaMediator.Publish(_concordiaNotification);

    [Benchmark]
    public Task ConcordiaGen_PublishNotification()
        => _concordiaGenMediator.Publish(_concordiaNotification);

    [Benchmark]
    public ValueTask Martin_PublishNotification()
        => _martinMediator.Publish(_martinNotification);
}
