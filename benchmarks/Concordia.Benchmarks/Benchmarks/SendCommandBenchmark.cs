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
public class SendCommandBenchmark
{
    private Concordia.IMediator _concordiaMediator = null!;
    private Concordia.IMediator _concordiaGenMediator = null!;
    private MediatR.IMediator _mediatRMediator = null!;
    private MartinIMediator _martinMediator = null!;

    private readonly ConcordiaCommand _concordiaCommand = new() { Id = 1 };
    private readonly MediatRCommand _mediatRCommand = new() { Id = 1 };
    private readonly MartinCommand _martinCommand = new() { Id = 1 };

    [GlobalSetup]
    public void Setup()
    {
        // --- Concordia (wrapper-caching) setup ---
        var concordiaServices = new ServiceCollection();
        concordiaServices.AddSingleton<IMediator, Concordia.Mediator>();
        concordiaServices.AddSingleton<ISender, Concordia.Mediator>();
        concordiaServices.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
        concordiaServices.AddSingleton<IRequestHandler<ConcordiaCommand>, ConcordiaCommandHandler>();
        _concordiaMediator = concordiaServices.BuildServiceProvider().GetRequiredService<Concordia.IMediator>();

        // --- Concordia (source-generated) setup ---
        var genServices = new ServiceCollection();
        genServices.AddConcordiaHandlers();
        _concordiaGenMediator = genServices.BuildServiceProvider().GetRequiredService<Concordia.IMediator>();

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
    public Task Concordia_SendCommand()
        => _concordiaMediator.Send(_concordiaCommand);

    [Benchmark]
    public Task ConcordiaGen_SendCommand()
        => _concordiaGenMediator.Send(_concordiaCommand);

    [Benchmark]
    public ValueTask<global::Mediator.Unit> Martin_SendCommand()
        => _martinMediator.Send(_martinCommand);
}
