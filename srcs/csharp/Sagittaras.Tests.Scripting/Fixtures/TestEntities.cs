using Sagittaras.Scripting.Entities;

namespace Sagittaras.Tests.Scripting.Fixtures;

/// <summary>
///     Entity targeted by the scripts shared between all of its instances.
/// </summary>
public class TestEntity(string id) : IScriptedEntity
{
    public string Id { get; } = id;
}

/// <summary>
///     Entity whose every instance is served by its own script.
/// </summary>
public class TestTransientEntity(string id, int instanceId) : ITransientScriptedEntity
{
    public string Id { get; } = id;

    public int InstanceId { get; } = instanceId;

    public event EventHandler<ITransientScriptedEntity>? Released;

    /// <summary>
    ///     Announces the end of the lifecycle, as a pooled entity returning to its pool would.
    /// </summary>
    public void Release()
    {
        Released?.Invoke(this, this);
    }

    /// <summary>
    ///     Number of handlers currently subscribed to <see cref="Released"/>.
    /// </summary>
    /// <remarks>
    ///     Lets the tests verify that the registry unsubscribes, which is otherwise invisible from outside.
    /// </remarks>
    public int SubscriberCount => Released?.GetInvocationList().Length ?? 0;
}