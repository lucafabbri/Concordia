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
public class SendCommandBenchmark
{
    private Synaptrix.IMediator _synaptrixMediator = null!;
    private Synaptrix.IMediator _synaptrixGenMediator = null!;
    private MediatR.IMediator _mediatRMediator = null!;
    private MartinIMediator _martinMediator = null!;

    private readonly SynaptrixCommand _synaptrixCommand = new() { Id = 1 };
    private readonly MediatRCommand _mediatRCommand = new() { Id = 1 };
    private readonly MartinCommand _martinCommand = new() { Id = 1 };

    [GlobalSetup]
    public void Setup()
    {
        // --- Synaptrix (wrapper-caching) setup ---
        var synaptrixServices = new ServiceCollection();
        synaptrixServices.AddSingleton<IMediator, Synaptrix.Mediator>();
        synaptrixServices.AddSingleton<ISender, Synaptrix.Mediator>();
        synaptrixServices.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
        synaptrixServices.AddSingleton<IRequestHandler<SynaptrixCommand>, SynaptrixCommandHandler>();
        _synaptrixMediator = synaptrixServices.BuildServiceProvider().GetRequiredService<Synaptrix.IMediator>();

        // --- Synaptrix (source-generated) setup ---
        var genServices = new ServiceCollection();
        genServices.AddSynaptrixHandlers();
        _synaptrixGenMediator = genServices.BuildServiceProvider().GetRequiredService<Synaptrix.IMediator>();

        // --- MediatR setup ---
        var mediatRServices = new ServiceCollection();
        mediatRServices.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(MediatRCommandHandler).Assembly));
        _mediatRMediator = mediatRServices.BuildServiceProvider().GetRequiredService<MediatR.IMediator>();

        // --- martinothamar/Mediator setup ---
        var martinServices = new ServiceCollection();
        martinServices.AddMediator(opts => opts.ServiceLifetime = ServiceLifetime.Singleton);
        _martinMediator = martinServices.BuildServiceProvider().GetRequiredService<MartinIMediator>();
    }

    [Benchmark(Baseline = true)]
    public Task MediatR_SendCommand()
        => _mediatRMediator.Send(_mediatRCommand);

    [Benchmark]
    public ValueTask Synaptrix_SendCommand()
        => _synaptrixMediator.Send(_synaptrixCommand);

    [Benchmark]
    public ValueTask SynaptrixGen_SendCommand()
        => _synaptrixGenMediator.Send(_synaptrixCommand);

    [Benchmark]
    public ValueTask<global::Mediator.Unit> Martin_SendCommand()
        => _martinMediator.Send(_martinCommand);
}
