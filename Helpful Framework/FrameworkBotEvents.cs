using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Helpful.Framework
{
    public abstract partial class FrameworkBot
    {
        /// <summary>Fired when a new console line is sent</summary>
        public event Func<string, Task> ConsoleInput;

        /// <summary>Fired when connected to the Discord gateway</summary>
        public void Connected(Func<DiscordSocketClient, Task> func) 
            => CastInternal().ShardConnected += func;

        /// <summary>Fired when connected to the Discord gateway</summary>
        public void Connected(Func<Task> func)
        {
            if (SocketClient is DiscordSocketClient bot)
                bot.Connected += func;
            else
                CastInternal().ShardConnected += _ => func();
        }

        /// <summary>Fired when disconnected from the Discord gateway</summary>
        public void Disconnected(Func<Exception, DiscordSocketClient, Task> func) 
            => CastInternal().ShardDisconnected += func;

        /// <summary>Fired when disconnected from the Discord gateway</summary>
        public void Disconnected(Func<Exception, Task> func)
        {
            if (SocketClient is DiscordSocketClient bot)
                bot.Disconnected += func;
            else
                CastInternal().ShardDisconnected += (ex, _) => func(ex);
        }

        /// <summary>Fired when guild data has finished downloading</summary>
        public void Ready(Func<DiscordSocketClient, Task> func) 
            => CastInternal().ShardReady += func;

        /// <summary>Fired when guild data has finished downloading</summary>
        public void Ready(Func<Task> func)
        {
            if (SocketClient is DiscordSocketClient bot)
                bot.Ready += func;
            else
                CastInternal().ShardReady += _ => func();
        }

        /// <summary>Fired when a heartbeat is received from the Discord gateway</summary>
        public void LatencyUpdated(Func<int, int, DiscordSocketClient, Task> func)
            => CastInternal().ShardLatencyUpdated += func;

        /// <summary>Fired when a heartbeat is received from the Discord gateway</summary>
        public void LatencyUpdated(Func<int, int, Task> func)
        {
            if (SocketClient is DiscordSocketClient bot)
                bot.LatencyUpdated += func;
            else
                CastInternal().ShardLatencyUpdated += (o, n, _) => func(o, n);
        }

        private DiscordShardedClient CastInternal(BaseSocketClient client = null)
        {
            client = client ?? SocketClient;
            return client as DiscordShardedClient ?? throw new InvalidOperationException("Socket Client chosen was not a sharded client.");
        }

    }
}
