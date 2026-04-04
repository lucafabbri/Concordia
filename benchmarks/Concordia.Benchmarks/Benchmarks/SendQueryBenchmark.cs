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
public class SendQueryBenchmark
{
    private Concordia.IMediator _concordiaMediator = null!;
    private Concordia.IMediator _concordiaGenMediator = null!;
    private MediatR.IMediator _mediatRMediator = null!;
    private MartinIMediator _martinMediator = null!;

    private readonly ConcordiaQuery _concordiaQuery = new() { Id = 1 };
    private readonly MediatRQuery _mediatRQuery = new() { Id = 1 };
    private readonly MartinQuery _martinQuery = new() { Id = 1 };

    [GlobalSetup]
    public void Setup()
    {
        // --- Concordia (wrapper-caching) setup ---
        var concordiaServices = new ServiceCollection();
        concordiaServices.AddSingleton<IMediator, Concordia.Mediator>();
        concordiaServices.AddSingleton<ISender, Concordia.Mediator>();
        concordiaServices.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
        concordiaServices.AddSingleton<IRequestHandler<ConcordiaQuery, string>, ConcordiaQueryHandler>();
        _concordiaMediator = concordiaServices.BuildServiceProvider().GetRequiredService<Concordia.IMediator>();

        // --- Concordia (source-generated) setup ---
        var genServices = new ServiceCollection();
        genServices.AddConcordiaHandlers();
        _concordiaGenMediator = genServices.BuildServiceProvider().GetRequiredService<Concordia.IMediator>();

        // --- MediatR setup ---
        var mediatRServices = new ServiceCollection();
        mediatRServices.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(MediatRQueryHandler).Assembly));
        _mediatRMediator = mediatRServices.BuildServiceProvider().GetRequiredService<MediatR.IMediator>();

        // --- martinothamar/Mediator setup ---
        var martinServices = new ServiceCollection();
        martinServices.AddMediator(opts => opts.ServiceLifetime = ServiceLifetime.Singleton);
        _martinMediator = martinServices.BuildServiceProvider().GetRequiredService<MartinIMediator>();
    }

    [Benchmark(Baseline = true)]
    public Task<string> MediatR_SendQuery()
        => _mediatRMediator.Send(_mediatRQuery);

    [Benchmark]
    public Task<string> Concordia_SendQuery()
        => _concordiaMediator.Send(_concordiaQuery);

    [Benchmark]
    public Task<string> ConcordiaGen_SendQuery()
        => _concordiaGenMediator.Send(_concordiaQuery);

    [Benchmark]
    public ValueTask<string> Martin_SendQuery()
        => _martinMediator.Send(_martinQuery);
}
