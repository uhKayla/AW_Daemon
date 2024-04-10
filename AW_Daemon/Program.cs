using AW_Daemon.Core;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace AW_Daemon;

public class Program
{
    // Vars
    private const ulong GuildId = 1197911643196227635;
    public SocketGuild Guild { get; private set; }
    
    // Initialize main async loop
    public static Task Main(string[] args)
    {
        return new Program().MainAsync();
    }
    
    // Client socket
    private readonly DiscordSocketClient _client = new(new DiscordSocketConfig
    {
        LogLevel = LogSeverity.Verbose
    });

    // Primary service provider
    private IServiceProvider _services;
    
    // Services to initialize
    public ActivationHandler _activationHandler { get; private set; }

    // Main program loop
    private async Task MainAsync()
    {
        _client.Log += Log; // Subscribe to log events
        _client.Ready += ClientReady;
        
        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TOKEN"));
        await _client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }
    
    /// <summary>
    /// Main program logger.
    /// </summary>
    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    /// <summary>
    /// Configure our service provider.
    /// </summary>
    private IServiceProvider ConfigureServices(SocketGuild guild)
    {
        _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton<InteractionService>()
            .AddSingleton<ActivationHandler>(provider => new ActivationHandler(provider.GetRequiredService<DiscordSocketClient>(), GuildId))
            .AddSingleton<SlashCommandHandler>()
            .BuildServiceProvider();
        return _services;
    }

    /// <summary>
    /// Configure our client and commands.
    /// </summary>
    private async Task ClientReady()
    {
        // Fetch Guild
        Guild = _client.GetGuild(GuildId);

        // If we cannot find the guild, return with an error.
        if (Guild == null)
        {
            Console.WriteLine("Guild couldn't be found!");
            return;
        }
        
        // Configure services if guild is found and available.
        _services = ConfigureServices(Guild);
        _activationHandler = _services.GetRequiredService<ActivationHandler>();
        
        // Get the command handler from services and subscribe to the SlashCommandExecuted event
        var commandHandler = _services.GetRequiredService<SlashCommandHandler>();
        _client.SlashCommandExecuted += async command => { await commandHandler.SlashCommand(command); };
        
        // Commands
        var guildCommand = new SlashCommandBuilder()
            .WithName("test")
            .WithDescription("This is my first guild slash command!")
            .AddOption("user", ApplicationCommandOptionType.User, "The users whos roles you want to be listed", true)
            .Build();

        var activateCommand = new SlashCommandBuilder()
            .WithName("verify")
            .WithDescription("Verify your account and add a role!")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("type")
                .WithDescription("Choose your type of activation.")
                .WithRequired(true)
                .AddChoice("Discord Username", 1)
                .AddChoice("Email Address", 2)
                .WithType(ApplicationCommandOptionType.Integer)
            )
            .Build();
        
        try
        {
            await Guild.DeleteApplicationCommandsAsync();
            await Guild.CreateApplicationCommandAsync(guildCommand);
            await Guild.CreateApplicationCommandAsync(activateCommand);
            // await Guild.CreateApplicationCommandAsync(orderRegisterCommand);
            
            Console.WriteLine("Commands registered.");
        }
        catch (ApplicationCommandException exception)
        {
            Console.WriteLine(exception);
        }
    }
    
}
