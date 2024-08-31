using System.IO;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;
using PawsyApp.PawsyCore.Modules.GuildSubmodules;
using System.Collections.Concurrent;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using System;

namespace PawsyApp.PawsyCore.Modules.Core;

internal class GuildModule() : IModuleIdent
{
    public IModule? Owner { get => _owner; set => _owner = value; }
    public ConcurrentBag<IModule> Modules => _modules;
    public string Name { get => "GuildGlobal"; }
    public ulong ID { get => _id; set => _id = value; }
    IModuleSettings? IModule.Settings => Settings;
    public string GetSettingsLocation() =>
    Path.Combine(Helpers.GetPersistPath(ID), $"{Name}.json");

    protected ulong _id;
    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];
    protected readonly ConcurrentDictionary<ulong, SlashCommandBundle> GuildCommands = [];
    protected GuildSettings? Settings;

    public delegate Task GuildMessageHandler(SocketUserMessage message, SocketGuildChannel channel);
    public delegate Task GuildThreadCreatedHandler(SocketThreadChannel channel);
    public event GuildMessageHandler? OnGuildMessage;
    public event GuildMessageHandler? OnGuildMessageEdit;
    public event GuildThreadCreatedHandler? OnGuildThreadCreated;

    public void Activate()
    {
        //Decide if we should activate modules here
        if (this is IModule module)
        {
            Settings = module.LoadSettings<GuildSettings>();
            module.AddModule<MeowBoardModule>();
            module.AddModule<FilterMatcherModule>();
            module.AddModule<LogMuncherModule>();
            module.AddModule<ModderRoleCheckerModule>();
        }

        //Subscribe to events
        PawsyProgram.SocketClient.MessageReceived += OnMessage;
        PawsyProgram.SocketClient.MessageUpdated += OnMessageEdit;
        PawsyProgram.SocketClient.SlashCommandExecuted += OnSlashCommand;
        PawsyProgram.SocketClient.ThreadCreated += OnThreadCreated;

        foreach (var item in Modules)
        {
            if (Settings is not null && Settings.EnabledModules.Contains(item.Name))
            {
                WriteLog.LineNormal($"Registering {item.Name}");
                item.RegisterHooks();
            }
        }
    }
    public void RegisterHooks() { return; }
    public async void RegisterSlashCommand(SlashCommandBundle bundle)
    {
        await WriteLog.LineNormal("Registering a command");
        var sockCommand = await PawsyProgram.SocketClient.GetGuild(ID).CreateApplicationCommandAsync(bundle.BuiltCommand);
        //var restCommand = await PawsyProgram.RestClient.CreateGuildCommand(bundle.BuiltCommand, ID);
        GuildCommands.TryAdd(sockCommand.Id, bundle);
    }
    private async Task OnThreadCreated(SocketThreadChannel channel)
    {
        if (channel.Guild.Id != ID)
            return;

        if (OnGuildThreadCreated is not null)
        {
            await OnGuildThreadCreated(channel);
        }
    }
    private async Task OnSlashCommand(SocketSlashCommand command)
    {
        if (command.GuildId is not ulong guild)
            return;

        if (guild != ID)
            return;

        if (Settings is null || !GuildCommands.TryGetValue(command.CommandId, out SlashCommandBundle bundle))
        {
            await command.RespondAsync("Sowwy, meow. That command is not available", ephemeral: true);
            return;
        }

        if (Settings.EnabledModules.Contains(bundle.ModuleName))
        {
            await bundle.Handler(command);
            return;
        }

        await command.RespondAsync($"Sowwy, meow :sob: The {bundle.ModuleName} module is disabled on this guild.", ephemeral: true);
    }
    private async Task OnMessage(SocketMessage message)
    {
        //Filter out bots, system and webhook message
        if (message.Author.IsBot || message.Author.IsWebhook || message.Source == MessageSource.System)
            return;

        if (message is SocketUserMessage uMessage)
        {
            //Guild messages
            if (OnGuildMessage is not null && message.Channel is SocketGuildChannel gChannel && gChannel.Guild.Id == ID)
                await OnGuildMessage(uMessage, gChannel);
        }
    }

    private async Task OnMessageEdit(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel channel)
    {
        //Filter out bots, system and webhook message
        if (message.Author.IsBot || message.Author.IsWebhook || message.Source == MessageSource.System)
            return;

        if (message is SocketUserMessage uMessage)
        {
            //Guild messages
            if (OnGuildMessageEdit is not null && message.Channel is SocketGuildChannel gChannel && gChannel.Guild.Id == ID)
            {
                /*
                await WriteLog.Cutely("Pawsy heard an update", [
                ("CacheID", cacheable.Id),
                ("Cached", cacheable.HasValue),
                ("Author", uMessage.Author),
                ("Channel", gChannel.Guild.Name),
                ]);
                */

                await OnGuildMessageEdit(uMessage, gChannel);
            }
        }
    }
}
