using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;

namespace Wolverine.EntityFrameworkCore.Codegen;

internal class DbContextBulkOperationFrame : AsyncFrame
{
    private readonly Type _dbContextType;
    private readonly string _methodName;
    private readonly Variable _saga;
    private Variable? _context;
    private Variable? _cancellation;

    public DbContextBulkOperationFrame(Type dbContextType, Variable saga, string methodName)
    {
        _dbContextType = dbContextType;
        _saga = saga;
        _methodName = methodName;
    }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _context = chain.FindVariable(_dbContextType);
        yield return _context;

        _cancellation = chain.FindVariable(typeof(CancellationToken));
        yield return _cancellation;
    }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.WriteLine("");
        writer.WriteComment("Registering the bulk entity operation");
        writer.Write($"await System.Linq.Queryable.Where({_context!.Usage}.Set<{_saga.VariableType}>(), {_saga.Usage}.FilterFunction).{_methodName}({_cancellation!.Usage}).ConfigureAwait(false);");
        Next?.GenerateCode(method, writer);
    }
}
