using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using System.Linq.Expressions;
using Wolverine.Configuration;
using Wolverine.Persistence.Sagas;
using Wolverine.Runtime.Handlers;

namespace Wolverine.Persistence;

/// <summary>
/// Return side effect value that deletes all matching entities from the underlying persistence mechanism
/// </summary>
/// <param name="FilterFunction">Function that receives each entity and decides if it should be deleted or not</param>
/// <typeparam name="T"></typeparam>
public record BulkDelete<T>(Expression<Func<T, bool>> FilterFunction) : ISideEffectAware
{
    public static Frame BuildFrame(IChain chain, Variable variable, GenerationRules rules, IServiceContainer container)
    {
        if (rules.TryFindPersistenceFrameProvider(container, typeof(T), out var provider))
        {
            provider.ApplyTransactionSupport(chain, container, typeof(T));
            var value = new Variable(variable.VariableType.GetGenericArguments()[0], variable.Usage);
            return provider.DetermineBulkDeleteFrame(value, container).WrapIfNotNull(value);
        }

        throw new NoMatchingPersistenceProviderException(typeof(T));
    }
}
