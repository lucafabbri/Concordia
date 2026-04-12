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
public class SendQueryBenchmark
{
    private Synaptrix.IMediator _synaptrixMediator = null!;
    private Synaptrix.IMediator _synaptrixGenMediator = null!;
    private MediatR.IMediator _mediatRMediator = null!;
    private MartinIMediator _martinMediator = null!;

    private readonly SynaptrixQuery _synaptrixQuery = new() { Id = 1 };
    private readonly MediatRQuery _mediatRQuery = new() { Id = 1 };
    private readonly MartinQuery _martinQuery = new() { Id = 1 };

    [GlobalSetup]
    public void Setup()
    {
        // --- Synaptrix (wrapper-caching) setup ---
        var synaptrixServices = new ServiceCollection();
        synaptrixServices.AddSingleton<IMediator, Synaptrix.Mediator>();
        synaptrixServices.AddSingleton<ISender, Synaptrix.Mediator>();
        synaptrixServices.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
        synaptrixServices.AddSingleton<IRequestHandler<SynaptrixQuery, string>, SynaptrixQueryHandler>();
        _synaptrixMediator = synaptrixServices.BuildServiceProvider().GetRequiredService<Synaptrix.IMediator>();

        // --- Synaptrix (source-generated) setup ---
        var genServices = new ServiceCollection();
        genServices.AddSynaptrixHandlers();
        _synaptrixGenMediator = genServices.BuildServiceProvider().GetRequiredService<Synaptrix.IMediator>();

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
    public ValueTask<string> Synaptrix_SendQuery()
        => _synaptrixMediator.Send(_synaptrixQuery);

    [Benchmark]
    public ValueTask<string> SynaptrixGen_SendQuery()
        => _synaptrixGenMediator.Send(_synaptrixQuery);

    [Benchmark]
    public ValueTask<string> Martin_SendQuery()
        => _martinMediator.Send(_martinQuery);
}
