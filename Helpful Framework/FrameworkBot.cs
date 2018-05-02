using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HelpfulUtilities.Discord.Listeners;
using HelpfulUtilities.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HelpfulUtilities.Discord.Commands.Readers;
using System.Reflection;
using Helpful.Framework.Config;
using HelpfulUtilities.Discord.Commands.Extensions;
using System.Linq;
using System.Threading;
using Helpful.Framework.Services;
using HelpfulUtilities.Discord.Extensions;

namespace Helpful.Framework
{
    /// <summary>A base framework bot for Discord bots employing the Discord.NET API wrapper</summary>
    public abstract partial class FrameworkBot<TConfig, TGuild, TUser>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild
        where TUser : class, IConfigUser
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
        protected internal IList<IService<TConfig, TGuild, TUser>> ServiceList { get; protected set; } = new List<IService<TConfig, TGuild, TUser>>();
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
        public abstract ICommandContext CreateContext(IUserMessage message);
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


            var collection = new ServiceCollection();
            collection.AddSingleton(SocketClient.GetType(), SocketClient)
                .AddSingleton(CommandService)
                .AddSingleton(ListenerService)
                .AddSingleton(new ConfigService<TConfig, TGuild, TUser>(this))
                .AddSingleton(GetType(), this);

            foreach (var service in services)
            {
                collection.AddSingleton(service.GetType(), service);
            }

            foreach (var service in ServiceList)
            {
                DisconnectList[service.GetType()] = false;
                collection.AddSingleton(service.GetType(), service);
            }

            ServiceProvider = collection.BuildServiceProvider();

            foreach (var pair in TypeReaders)
                CommandService.AddTypeReader(pair.Key, pair.Value, true);

            ListenerService.AddModules(Assembly.GetAssembly(GetType()));
            await CommandService.AddModulesAsync(Assembly.GetAssembly(GetType()), ServiceProvider);

            SocketClient.MessageReceived += MessageReceivedHandler;
            Ready(ReadyHandler);
        }

        /// <summary>
        /// Framework starting of the bot, to be called in <see cref="StartAsync"/>
        /// </summary>
        protected async Task StartInternalAsync()
        {
            await Configuration.Connect();
            await SocketClient.LoginAsync(TokenType.Bot, BotConfig.Token);
            await SocketClient.StartAsync();
        }

        /// <summary>
        /// Framework stopping of the bot, to be called in <see cref="StopAsync"/>
        /// </summary>
        protected async Task StopInternalAsync(bool graceful, TimeSpan? timeout)
        {
            timeout = timeout ?? Timeout.InfiniteTimeSpan;
            if (graceful)
            {
                Parallel.ForEach(ServiceList, service =>
                {
                    DisconnectList[service.GetType()] = service.CanDisconnect(this);
                });

                await Task.WhenAny(Task.WhenAll(DisconnectList.Keys.Select(type => 
                    Task.Run(() => DisconnectList[type]))), Task.Delay(timeout.Value));
            }

            if(ServiceList.LongCount() >= 0)
            {
                await Task.WhenAny(Task.WhenAll(ServiceList.Select(service =>
                    Task.Run(() => service.Disconnect(this)))), Task.Delay(timeout.Value));
            }

            await SocketClient.StopAsync();
            await SocketClient.LogoutAsync();
            await Configuration.Disconnect();
        }

        /// <summary>
        /// Framework restarting of the bot, to be called in <see cref="RestartAsync"/>
        /// </summary>
        protected async Task RestartInternalAsync(bool graceful, TimeSpan? timeout)
        {
            await StopAsync(graceful, timeout).ContinueWith(async _ =>
            {
                await StartAsync();
            });
        }

        /// <summary>
        /// Framework unloading of the bot, to be called in <see cref="UnloadAsync"/>
        /// </summary>
        protected async Task UnloadInternalAsync()
        {
            foreach (var module in CommandService.Modules)
                await CommandService.RemoveModuleAsync(module);

            foreach (var listener in ListenerService.Listeners)
                ListenerService.RemoveModule(listener);

            CommandService = null;
            ListenerService = null;

            ServiceList.Clear();
            ServiceList = null;
            ServiceProvider = null;

            SocketClient = null;
            Configuration = null;
        }

        /// <summary>Default handling of ready events</summary>
        public async Task ReadyHandler()
        {
            var guilds = SocketClient.Guilds.Where(g => !Configuration.Guilds.ContainsKey(g.Id));

            foreach (var guild in guilds)
                await Configuration.Create(guild);

            if (guilds.LongCount() > 0)
                await Configuration.WriteAsync(DatabaseType.Guild);
        }

        /// <summary>Default handling of messages</summary>
        public async Task MessageReceivedHandler(SocketMessage msg)
        {
            if (msg.Author.IsWebhook || msg.Author.Id == SocketClient.CurrentUser.Id) { return; }

            if (msg is SocketUserMessage message)
            {
                var pos = 0;
                var prefix = BotConfig.Prefix;
                var context = CreateContext(message);

                if (message.Channel is SocketTextChannel)
                    prefix = Configuration.Guilds[message.GetGuild().Id].Prefix;

                if (message.HasPrefix(prefix, SocketClient, ref pos))
                {
                    await HandleResult(context, await CommandService.ExecuteAsync(context, pos, ServiceProvider));
                }
                else
                {
                    foreach (var result in await ListenerService.ExecuteAsync(context, ServiceProvider))
                    {
                        await HandleResult(context, result, false);
                    }
                }
            }
        }

        /// <summary>Marks the specified service as ready to disconnect</summary>
        public void Ready<TService>() where TService : IService<TConfig, TGuild, TUser>
            => Ready(typeof(TService));

        /// <summary>Marks the specified service as ready to disconnnect</summary>
        public void Ready(Type type)
        {
            if (!type.Extends(typeof(IService<TConfig, TGuild, TUser>))) throw new
                    InvalidCastException($"Cannot cast {type.FullName} to {typeof(IService<TConfig, TGuild, TUser>).FullName}");
            DisconnectList[type] = false;
        }
    }
}
