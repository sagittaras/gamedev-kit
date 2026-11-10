using System;
using System.Collections.Generic;
using Sagittaras.Scripting.Attributes;
using Sagittaras.Scripting.Entities;
using Sagittaras.Scripting.Registries;

namespace Sagittaras.Scripting
{
    /// <summary>
    ///     Represents a global entrypoint for calling and invoking instances of <see cref="IGameScript"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The manager is the only place where the core module meets the extending ones. The core reaches
    ///         its own hook-point by calling <c>Call</c> with the abstract base script as the type argument;
    ///         the manager resolves the registered implementations and invokes the passed delegate on them.
    ///         A hook-point with no implementation is not an error - the call simply does nothing and the
    ///         result-returning overloads fall back to the default value.
    ///     </para>
    ///     <para>
    ///         Overloads differ only in what the script is bound to: no target for
    ///         <see cref="IGlobalScript"/>, an entity identity for <see cref="IEntityScript"/>, and a concrete
    ///         entity instance for <see cref="ITransientEntityScript"/>. The manager itself only dispatches -
    ///         the scripts are held by <see cref="IGlobalScriptRegistry"/>,
    ///         <see cref="IEntityScriptRegistry"/> and <see cref="ITransientEntityScriptRegistry"/>.
    ///     </para>
    /// </remarks>
    public interface IScriptManager
    {
        /// <summary>
        ///     Creates the transient script serving the given entity instance under the <typeparamref name="TGameScript"/> entrypoint.
        /// </summary>
        /// <remarks>
        ///     The created instance lives alongside the <paramref name="entity"/> until it raises
        ///     <see cref="ITransientScriptedEntity.Released"/>; calling the method again for an entity that
        ///     has been released starts a fresh, freshly initialized script instance.
        /// </remarks>
        /// <param name="entity">Scripted entity instance for which the script is created.</param>
        /// <param name="args">Arguments of the spawn, as offered by the entrypoint.</param>
        /// <typeparam name="TGameScript">Entrypoint to create the script under.</typeparam>
        /// <typeparam name="TArgs">Arguments offered by the entrypoint.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when the entity instance already holds a script reached through one of the entrypoints of this one.</exception>
        void Instantiate<TGameScript, TArgs>(ITransientScriptedEntity entity, in TArgs args) where TGameScript : class, ITransientEntityScript<TArgs>;

        /// <summary>
        ///     Creates the transient script serving the given entity instance under an entrypoint offering no arguments.
        /// </summary>
        /// <param name="entity">Scripted entity instance for which the script is created.</param>
        /// <typeparam name="TGameScript">Entrypoint to create the script under.</typeparam>
        void Instantiate<TGameScript>(ITransientScriptedEntity entity) where TGameScript : class, ITransientEntityScript<NoArgs>;

        /// <summary>
        ///     Executes the <paramref name="hook"/> action for the <typeparamref name="TGameScript"/> instance.
        /// </summary>
        /// <remarks>
        ///     The action is executed for every registered implementation of <typeparamref name="TGameScript"/>,
        ///     ordered by <see cref="ScriptPriorityAttribute"/>, so multiple extending modules may react to
        ///     the same hook-point.
        /// </remarks>
        /// <param name="hook">Action to execute.</param>
        /// <typeparam name="TGameScript">Type of <see cref="IGlobalScript"/> to execute the <paramref name="hook"/> action for.</typeparam>
        void Call<TGameScript>(Action<TGameScript> hook) where TGameScript : class, IGlobalScript;

        /// <summary>
        ///     Executes the <paramref name="hook"/> action for the <typeparamref name="TGameScript"/> instance and returns the result.
        /// </summary>
        /// <remarks>
        ///     Method iterates all possible instances of <typeparamref name="TGameScript"/> until the first
        ///     result differing from the default of <typeparamref name="TResult"/> is found. The first script
        ///     that answers therefore wins, and the remaining ones are not asked at all - which is what
        ///     <see cref="ScriptPriorityAttribute"/> is there to decide.
        /// </remarks>
        /// <param name="hook">Action to execute.</param>
        /// <param name="default">
        ///     Value returned when none of the scripts answered. It says nothing about what counts as an
        ///     answer - a script never sees it, so passing the question on stays tied to the default of
        ///     <typeparamref name="TResult"/>.
        /// </param>
        /// <typeparam name="TGameScript">Type of <see cref="IGlobalScript"/> to execute the <paramref name="hook"/> action for.</typeparam>
        /// <typeparam name="TResult">Type of the result returned by the <paramref name="hook"/> action.</typeparam>
        /// <returns>The result of the <paramref name="hook"/> action, or <paramref name="default"/> when no script answered.</returns>
        TResult? Call<TGameScript, TResult>(Func<TGameScript, TResult> hook, TResult? @default = default) where TGameScript : class, IGlobalScript;

        /// <summary>
        ///     Executes the <paramref name="hook"/> action for the <typeparamref name="TGameScript"/> instance
        ///     registered for the <paramref name="entity"/>.
        /// </summary>
        /// <remarks>
        ///     The script is looked up by <see cref="IScriptedEntity.Id"/> and there is at most one under the
        ///     entrypoint. Entities without a script for the given hook-point keep the default behavior of
        ///     the core module.
        /// </remarks>
        /// <param name="entity">Scripted entity whose script is the target of the call.</param>
        /// <param name="hook">Action to execute.</param>
        /// <typeparam name="TGameScript">Type of <see cref="IEntityScript"/> to execute the <paramref name="hook"/> action for.</typeparam>
        void Call<TGameScript>(IScriptedEntity entity, Action<TGameScript> hook) where TGameScript : class, IEntityScript;

        /// <summary>
        ///     Executes the <paramref name="hook"/> action for the <typeparamref name="TGameScript"/> instance
        ///     registered for the <paramref name="entity"/> and returns the result.
        /// </summary>
        /// <remarks>
        ///     Lets the script of the entity override a value the core module would otherwise compute on its own.
        ///     An entity holds at most one script per entrypoint, so whatever it answers is final - even the
        ///     default of <typeparamref name="TResult"/>. Only the absence of a script yields
        ///     <paramref name="default"/>.
        /// </remarks>
        /// <param name="entity">Scripted entity whose script is the target of the call.</param>
        /// <param name="hook">Action to execute.</param>
        /// <param name="default">Value returned when the entity has no script under the entrypoint.</param>
        /// <typeparam name="TGameScript">Type of <see cref="IEntityScript"/> to execute the <paramref name="hook"/> action for.</typeparam>
        /// <typeparam name="TResult">Type of the result returned by the <paramref name="hook"/> action.</typeparam>
        /// <returns>The result of the <paramref name="hook"/> action, or <paramref name="default"/> when the entity has no script.</returns>
        TResult? Call<TGameScript, TResult>(IScriptedEntity entity, Func<TGameScript, TResult> hook, TResult? @default = default) where TGameScript : class, IEntityScript;

        /// <summary>
        ///     Executes the <paramref name="hook"/> action for the <typeparamref name="TGameScript"/> instance
        ///     belonging to this single <paramref name="entity"/> instance.
        /// </summary>
        /// <remarks>
        ///     Targets the script instance created by <see cref="Instantiate{TGameScript,TArgs}"/>, so the
        ///     hook-point may rely on the state the script has accumulated over the lifecycle of that very
        ///     entity instance. Nothing happens before the script is created, or after the entity has been
        ///     released.
        /// </remarks>
        /// <param name="entity">Scripted entity instance whose script is the target of the call.</param>
        /// <param name="hook">Action to execute.</param>
        /// <typeparam name="TGameScript">Type of <see cref="ITransientEntityScript"/> to execute the <paramref name="hook"/> action for.</typeparam>
        void Call<TGameScript>(ITransientScriptedEntity entity, Action<TGameScript> hook) where TGameScript : class, ITransientEntityScript;

        /// <summary>
        ///     Executes the <paramref name="hook"/> action for the <typeparamref name="TGameScript"/> instance
        ///     belonging to this single <paramref name="entity"/> instance and returns the result.
        /// </summary>
        /// <remarks>
        ///     Lets the script of this particular entity instance override a value the core module would
        ///     otherwise compute on its own. Whatever the script answers is final - only the absence of one,
        ///     before it is created or after the entity has been released, yields <paramref name="default"/>.
        /// </remarks>
        /// <param name="entity">Scripted entity instance whose script is the target of the call.</param>
        /// <param name="hook">Action to execute.</param>
        /// <param name="default">Value returned when the entity instance has no script under the entrypoint.</param>
        /// <typeparam name="TGameScript">Type of <see cref="ITransientEntityScript"/> to execute the <paramref name="hook"/> action for.</typeparam>
        /// <typeparam name="TResult">Type of the result returned by the <paramref name="hook"/> action.</typeparam>
        /// <returns>The result of the <paramref name="hook"/> action, or <paramref name="default"/> when the entity instance has no script.</returns>
        TResult? Call<TGameScript, TResult>(ITransientScriptedEntity entity, Func<TGameScript, TResult> hook, TResult? @default = default) where TGameScript : class, ITransientEntityScript;

        /// <summary>
        ///     Tells whether any script extends the <typeparamref name="TGameScript"/> hook-points.
        /// </summary>
        /// <remarks>
        ///     Worth asking before work that only pays off once a script is there to see it - gathering a
        ///     context, building a snapshot. Reaching a hook-point nobody extends is cheap on its own and
        ///     needs no guarding.
        /// </remarks>
        /// <typeparam name="TGameScript">Entrypoint to ask about.</typeparam>
        /// <returns>Returns true when at least one script is registered under the entrypoint; otherwise false.</returns>
        bool HasScript<TGameScript>() where TGameScript : class, IGlobalScript;

        /// <summary>
        ///     Gets every script extending the <typeparamref name="TGameScript"/> hook-points.
        /// </summary>
        /// <remarks>
        ///     Plural because a global hook-point is a broadcast - the entrypoint may be extended by several
        ///     modules at once. Scripts come ordered by <see cref="ScriptPriorityAttribute"/>, the same order
        ///     <c>Call</c> reaches them in.
        /// </remarks>
        /// <typeparam name="TGameScript">Entrypoint to get the scripts for.</typeparam>
        /// <returns>Returns the registered scripts, or an empty list when there is none.</returns>
        IReadOnlyList<TGameScript> GetScripts<TGameScript>() where TGameScript : class, IGlobalScript;

        /// <summary>
        ///     Tells whether the <paramref name="entity"/> is served by a script under the <typeparamref name="TGameScript"/> entrypoint.
        /// </summary>
        /// <param name="entity">Scripted entity to ask about.</param>
        /// <typeparam name="TGameScript">Entrypoint to ask about.</typeparam>
        /// <returns>Returns true when the entity has a script under the entrypoint; otherwise false.</returns>
        bool HasScript<TGameScript>(IScriptedEntity entity) where TGameScript : class, IEntityScript;

        /// <summary>
        ///     Gets the script serving the <paramref name="entity"/> under the <typeparamref name="TGameScript"/> entrypoint.
        /// </summary>
        /// <remarks>
        ///     Singular because an entity holds at most one script per entrypoint. Reach for it to hold the
        ///     script across several calls; a single hook-point is better reached by <c>Call</c>, which needs
        ///     no null check of its own.
        /// </remarks>
        /// <param name="entity">Scripted entity whose script is looked up.</param>
        /// <typeparam name="TGameScript">Entrypoint to get the script for.</typeparam>
        /// <returns>Returns the script serving the entity, or null when it has none.</returns>
        TGameScript? GetScript<TGameScript>(IScriptedEntity entity) where TGameScript : class, IEntityScript;

        /// <summary>
        ///     Tells whether this single <paramref name="entity"/> instance is served by a script under the
        ///     <typeparamref name="TGameScript"/> entrypoint.
        /// </summary>
        /// <remarks>
        ///     False until the script is created by <see cref="Instantiate{TGameScript,TArgs}"/>, and again
        ///     once the entity has been released.
        /// </remarks>
        /// <param name="entity">Scripted entity instance to ask about.</param>
        /// <typeparam name="TGameScript">Entrypoint to ask about.</typeparam>
        /// <returns>Returns true when the entity instance has a script under the entrypoint; otherwise false.</returns>
        bool HasScript<TGameScript>(ITransientScriptedEntity entity) where TGameScript : class, ITransientEntityScript;

        /// <summary>
        ///     Gets the script bound to this single <paramref name="entity"/> instance under the
        ///     <typeparamref name="TGameScript"/> entrypoint.
        /// </summary>
        /// <param name="entity">Scripted entity instance whose script is looked up.</param>
        /// <typeparam name="TGameScript">Entrypoint to get the script for.</typeparam>
        /// <returns>Returns the script bound to the entity instance, or null when it has none.</returns>
        TGameScript? GetScript<TGameScript>(ITransientScriptedEntity entity) where TGameScript : class, ITransientEntityScript;
    }
}
