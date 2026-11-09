using System.Diagnostics;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("Arzenal.Store.Api.TestIntegration.Infrastructure.IntegrationTestFramework", "Arzenal.Store.Api.TestIntegration")]

namespace Arzenal.Store.Api.TestIntegration.Infrastructure;

// xUnit 2 n'a pas de fixture d'assembly. Ce hook encadre la suite sans sérialiser ses classes.
public sealed class IntegrationTestFramework(IMessageSink messages) : XunitTestFramework(messages)
{
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) =>
        new Executor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);

    private sealed class Executor(AssemblyName name, ISourceInformationProvider sources, IMessageSink diagnostics)
        : XunitTestFrameworkExecutor(name, sources, diagnostics)
    {
        protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases,
            IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            using var runner = new AssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);
            await runner.RunAsync();
        }
    }

    private sealed class AssemblyRunner(ITestAssembly assembly, IEnumerable<IXunitTestCase> cases,
        IMessageSink diagnostics, IMessageSink messages, ITestFrameworkExecutionOptions options)
        : XunitTestAssemblyRunner(assembly, cases, diagnostics, messages, options)
    {
        protected override async Task AfterTestAssemblyStartingAsync()
        {
            await base.AfterTestAssemblyStartingAsync();
            // Préparer Docker hors des constructeurs synchrones des deux premiers tests.
            await Aggregator.RunAsync(async () =>
            {
                DiagnosticMessageSink.OnMessage(new DiagnosticMessage("Préparation de MySQL : attente d'une connexion depuis l'hôte..."));
                var stopwatch = Stopwatch.StartNew();
                await SharedMySqlServer.GetAsync();
                DiagnosticMessageSink.OnMessage(new DiagnosticMessage("MySQL joignable depuis l'hôte après {0:N1} s. Démarrage des tests.", stopwatch.Elapsed.TotalSeconds));
            });
        }

        protected override async Task BeforeTestAssemblyFinishedAsync()
        {
            // Les fixtures ont déjà libéré leurs bases. Nettoyer avant de notifier la fin à Visual Studio.
            await base.BeforeTestAssemblyFinishedAsync();
            await Aggregator.RunAsync(SharedMySqlServer.StopAsync);
        }
    }
}
