﻿// <copyright file="FunService.cs" company="The Shinoa Development Team">
// Copyright (c) 2016 - 2017 OmegaVesko.
// Copyright (c)        2017 The Shinoa Development Team.
// All rights reserved.
// Licensed under the MIT license.
// </copyright>

namespace Shinoa.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using SQLite;

    public class FunService : IDatabaseService
    {
        private SQLiteConnection db;

        public bool AddBinding(IMessageChannel channel)
        {
            if (db.Table<BotFunctionSpamBinding>().Any(b => b.ChannelId == channel.Id.ToString())) return false;

            db.Insert(new BotFunctionSpamBinding
            {
                ChannelId = channel.Id.ToString(),
            });
            return true;
        }

        public bool RemoveBinding<T>(T channel)
        {
            return db.Delete<BotFunctionSpamBinding>((channel as IMessageChannel).Id.ToString()) != 0;
        }

        public bool CheckBinding(IMessageChannel channel)
        {
            return db.Table<BotFunctionSpamBinding>().All(b => b.ChannelId != channel.Id.ToString());
        }

        void IService.Init(dynamic config, IDependencyMap map)
        {
            if (!map.TryGet(out db)) db = new SQLiteConnection(config["db_path"]);
            db.CreateTable<BotFunctionSpamBinding>();

            var client = map.Get<DiscordSocketClient>();
            client.MessageReceived += MessageReceivedHandler;
        }

        private async Task MessageReceivedHandler(SocketMessage m)
        {
            if (m.Author.IsBot) return;
            if (CheckBinding(m.Channel as ITextChannel)) return;

            switch (m.Content)
            {
                case @"o/":
                    await m.Channel.SendMessageAsync(@"\o");
                    break;
                case @"\o":
                    await m.Channel.SendMessageAsync(@"o/");
                    break;
                case @"/o/":
                    await m.Channel.SendMessageAsync(@"\o\");
                    break;
                case @"\o\":
                    await m.Channel.SendMessageAsync(@"/o/");
                    break;
                default:
                    if (m.Content.ToLower() == "wake me up")
                    {
                        await m.Channel.SendMessageAsync($"_Wakes {m.Author.Username} up inside._");
                    }
                    else if (m.Content.ToLower().StartsWith("wake") && m.Content.ToLower().EndsWith("up"))
                    {
                        var messageArray = m.Content.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                            .Skip(1)
                            .Reverse()
                            .Skip(1)
                            .Reverse();

                        await m.Channel.SendMessageAsync(
                            $"_Wakes {messageArray.Aggregate(string.Empty, (current, word) => current + word + " ").Trim()} up inside._");
                    }

                    break;
            }
        }

        private class BotFunctionSpamBinding
        {
            [PrimaryKey]
            public string ChannelId { get; set; }
        }
    }
}
