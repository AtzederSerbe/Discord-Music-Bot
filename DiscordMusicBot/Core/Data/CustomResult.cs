using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordMusicBot.Core.Data
{
    public class CustomResult : RuntimeResult
    {

        public Embed Embed { get; }
        public string Message { get; }

        public CustomResult(string message, Embed embed) : base(null, null)
        {
            Message = message;
            Embed = embed;
        }
    }

}
