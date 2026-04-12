using BenchmarkDotNet.Attributes;
using Synaptrix.Benchmarks.Generated;
using Synaptrix.Benchmarks.Handlers;
using Synaptrix.Benchmarks.Requests;
using MediatR;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using MartinIMediator = Mediator.IMediator;

namespace Synaptrix.Benchmarks;

[MemoryDiagnoser]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class PublishNotificationBenchmark
{
    private Synaptrix.IMediator _synaptrixMediator = null!;
    private Synaptrix.IMediator _synaptrixGenMediator = null!;
    private MediatR.IMediator _mediatRMediator = null!;
    private MartinIMediator _martinMediator = null!;

    private readonly SynaptrixNotification _synaptrixNotification = new() { Id = 1 };
    private readonly MediatRNotification _mediatRNotification = new() { Id = 1 };
    private readonly MartinNotification _martinNotification = new() { Id = 1 };

    [GlobalSetup]
    public void Setup()
    {
        // --- Synaptrix (wrapper-caching) setup ---
        var synaptrixServices = new ServiceCollection();
        synaptrixServices.AddSingleton<IMediator, Synaptrix.Mediator>();
        synaptrixServices.AddSingleton<ISender, Synaptrix.Mediator>();
        synaptrixServices.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
        synaptrixServices.AddSingleton<INotificationHandler<SynaptrixNotification>, SynaptrixNotificationHandler1>();
        synaptrixServices.AddSingleton<INotificationHandler<SynaptrixNotification>, SynaptrixNotificationHandler2>();
        _synaptrixMediator = synaptrixServices.BuildServiceProvider().GetRequiredService<Synaptrix.IMediator>();

        // --- Synaptrix (source-generated) setup ---
        var genServices = new ServiceCollection();
        genServices.AddSynaptrixHandlers();
        _synaptrixGenMediator = genServices.BuildServiceProvider().GetRequiredService<Synaptrix.IMediator>();

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
    public ValueTask Synaptrix_PublishNotification()
        => _synaptrixMediator.Publish(_synaptrixNotification);

    [Benchmark]
    public ValueTask SynaptrixGen_PublishNotification()
        => _synaptrixGenMediator.Publish(_synaptrixNotification);

    [Benchmark]
    public ValueTask Martin_PublishNotification()
        => _martinMediator.Publish(_martinNotification);
}
