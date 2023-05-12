using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Helpful.Framework.Config;
using HelpfulUtilities.Discord.Commands.Readers;
using HelpfulUtilities.Discord.Listeners;
using HelpfulUtilities.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Helpful.Framework
{
    /// <summary>A base framework bot for Discord bots employing the Discord.NET API wrapper</summary>
    public abstract partial class FrameworkBot<TConfig, TGuild, TUser, TCommandContext>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild
        where TUser : class, IConfigUser
        where TCommandContext : class, ICommandContext
    {
        /// <summary>A base bot config</summary>
        public FrameworkBotConfig BotConfig;

        /// <summary>The Socket Client used to connect to the Discord gateway</summary>
        public BaseSocketClient SocketClient { get; protected set; }
        /// <summary>The Command Service used for handling commands</summary>
        public CommandService CommandService { get; protected set; }
        /// <summary>The Listener Service used for handling other messages</summary>
        public ListenerService ListenerService { get; protected set; }
        /// <summary>The service provider used by <see cref="CommandService"/> and <see cref="ListenerService"/></summary>
        public IServiceProvider ServiceProvider { get; protected set; }
        /// <summary>The configuration provider to be used</summary>
        public TConfig Configuration { get; protected set; }

        /// <summary>A list of service types and whether they have disconnected</summary>
        public ConcurrentDictionary<Type, bool> DisconnectList { get; protected set; } = new ConcurrentDictionary<Type, bool>();
        /// <summary>A list of services</summary>
        protected internal IList<IService<TConfig, TGuild, TUser, TCommandContext>> ServiceList { get; protected set; } = new List<IService<TConfig, TGuild, TUser, TCommandContext>>();
        /// <summary>A list of type readers</summary>
        protected Dictionary<Type, TypeReader> TypeReaders { get; } = new Dictionary<Type, TypeReader>
        {
            { typeof(TimeSpan), new TimeSpanTypeReader() },
            { typeof(IEmote), new IEmoteTypeReader() },
            { typeof(Emoji), new EmojiTypeReader() },
            { typeof(Emote), new EmoteTypeReader() }
        };

        /// <summary>Readies base features</summary>
        protected FrameworkBot()
        {
            Task.Run(() => StartConsole());
        }

        /// <summary>Starts the bot</summary>
        public abstract Task StartAsync();
        /// <summary>Stops the bot</summary>
        /// <param name="graceful">Whether the bot should gracefully shutdown or not.</param>
        /// <param name="timeout">How long to wait before the bot proceeds</param>
        public abstract Task StopAsync(bool graceful = true, TimeSpan? timeout = null);
        /// <summary>Restarts the bot</summary>
        /// <param name="graceful">Whether the bot should gracefully restart or not.</param>
        /// <param name="timeout">How long to wait before the bot proceeds</param>
        public abstract Task RestartAsync(bool graceful = true, TimeSpan? timeout = null);

        /// <summary>Loads the bot</summary>
        public abstract Task LoadAsync();
        /// <summary>Unloads the bot</summary>
        public abstract Task UnloadAsync();

        /// <summary>Constructs a context given a <see cref="IUserMessage"/></summary>
        public abstract TCommandContext CreateContext(IUserMessage message);
        /// <summary>Handles the result of a listener of command operation</summary>
        public abstract Task HandleResult(ICommandContext context, IResult result, bool command = true);

        /// <summary>
        /// Framework loading of the bot, to be called in <see cref="LoadAsync"/>
        /// </summary>
        /// <param name="services">Extra services to load</param>
        protected async Task LoadInternalAsync(params object[] services)
        {
            if (BotConfig.ShouldShard)
                SocketClient = new DiscordShardedClient(BotConfig);
            else
                SocketClient = new DiscordSocketClient(BotConfig);

            CommandService = new CommandService(BotConfig.CommandServiceConfig);
            ListenerService = new ListenerService(new ListenerServiceConfig
            {
                ServiceProviderFactory = _ => ServiceProvider,
                RunMode = BotConfig.CommandServiceConfig.DefaultRunMode,
                LogLevel = BotConfig.LogLevel,
                ContextFactory = CreateContext
            });

            RegisterLogs();

            var collection = new ServiceCollection();
            collection.AddSingleton(SocketClient.GetType(), SocketClient)
                .AddSingleton(CommandService)
                .AddSingleton(ListenerService)
                .AddSingleton(GetType(), this);

            DisconnectList.AddRange(ServiceList.Select(i =>
                new KeyValuePair<Type, bool>(i.GetType(), false)));

            var deps = new object[services.Length + ServiceList.Count];
            var incrementer = 0;

            for (; incrementer < services.Length; incrementer++)
                deps[incrementer] = services[incrementer];

            for (var init = 0; init < ServiceList.Count; incrementer++, init++)
                deps[incrementer] = ServiceList[init];

            ServiceProvider = BuildServices(collection, deps);

            RegisterTypeReaders();

            await ListenerService.AddModulesAsync(Assembly.GetAssembly(GetType())).ConfigureAwait(false);
            await CommandService.AddModulesAsync(Assembly.GetAssembly(GetType()), ServiceProvider).ConfigureAwait(false);

            SocketClient.MessageReceived += DefaultMessageReceivedHandler;
            Ready(DefaultReadyHandler);
        }

        /// <summary>
        /// Framework starting of the bot, to be called in <see cref="StartAsync"/>
        /// </summary>
        protected async Task StartInternalAsync()
        {
            await Configuration.Connect().ConfigureAwait(false);
            await SocketClient.LoginAsync(TokenType.Bot, BotConfig.Token).ConfigureAwait(false);
            await SocketClient.StartAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Framework stopping of the bot, to be called in <see cref="StopAsync"/>
        /// </summary>
        protected async Task StopInternalAsync(bool graceful, TimeSpan? timeout)
        {
            timeout ??= Timeout.InfiniteTimeSpan;
            if (graceful)
            {
                Parallel.ForEach(ServiceList, service =>
                {
                    DisconnectList[service.GetType()] = service.CanDisconnect(this);
                });

                await Task.WhenAny(Task.WhenAll(DisconnectList.Keys.Select(type =>
                    Task.FromResult(DisconnectList[type]))), Task.Delay(timeout.Value)).ConfigureAwait(false);
            }

            if (ServiceList.Count >= 0)
            {
                await Task.WhenAny(Task.WhenAll(ServiceList.Select(service =>
                    Task.Run(() => service.Disconnect(this)))), Task.Delay(timeout.Value)).ConfigureAwait(false);
            }

            await SocketClient.StopAsync().ConfigureAwait(false);
            await SocketClient.LogoutAsync().ConfigureAwait(false);
            await Configuration.Disconnect().ConfigureAwait(false);
        }

        /// <summary>
        /// Framework restarting of the bot, to be called in <see cref="RestartAsync"/>
        /// </summary>
        protected async Task RestartInternalAsync(bool graceful, TimeSpan? timeout)
        {
            await StopAsync(graceful, timeout).ContinueWith(async _ =>
            {
                await StartAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Framework unloading of the bot, to be called in <see cref="UnloadAsync"/>
        /// </summary>
        protected async Task UnloadInternalAsync()
        {
            foreach (var module in CommandService.Modules)
                await CommandService.RemoveModuleAsync(module).ConfigureAwait(false);

            foreach (var listener in ListenerService.Listeners)
                await ListenerService.RemoveModuleAsync(listener).ConfigureAwait(false);

            CommandService = null;
            ListenerService = null;

            ServiceList.Clear();
            ServiceList = null;
            ServiceProvider = null;

            SocketClient = null;
            Configuration = null;
        }

        /// <summary>Marks the specified service as ready to disconnect</summary>
        public void Ready<TService>() where TService : IService<TConfig, TGuild, TUser, TCommandContext>
            => Ready(typeof(TService));

        /// <summary>Marks the specified service as ready to disconnnect</summary>
        public void Ready(Type type)
        {
            if (!type.Extends(typeof(IService<TConfig, TGuild, TUser, TCommandContext>))) throw new
                    InvalidCastException($"Cannot cast {type.FullName} to {typeof(IService<TConfig, TGuild, TUser, TCommandContext>).FullName}");
            DisconnectList[type] = false;
        }
    }
}
