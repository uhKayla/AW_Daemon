using System.Collections.Concurrent;
using AW_Daemon.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace AW_Daemon.Core;

public class ActivationHandler
{
    private readonly DiscordSocketClient _client;
    private readonly ulong _guildId;
    private const ulong RoleId = 1227077585792667669;
    private readonly ConcurrentDictionary<ulong, bool> _awaitingEmailConfirmation = new ConcurrentDictionary<ulong, bool>();
    
    public ActivationHandler(DiscordSocketClient client, ulong guildId)
    {
        _client = client;
        _guildId = guildId;
    }

    internal async Task HandleDiscordUsernameActivation(SocketSlashCommand command)
    {
        var guildId = command.GuildId;
        var guild = _client.GetGuild(guildId.GetValueOrDefault(_guildId));
        var user = guild.GetUser(command.User.Id);
        
        var un = command.User.Username;
        var content = new DiscordRequest()
        {
            DiscordUsername = un
        };

        var contentString = JsonConvert.SerializeObject(content);
        var url = "http://10.1.1.240:58082/v3/discord/activate";

        try
        {
            var response = await WebRequestHelper.MakePostRequestAsync(url, contentString);
            
            if (response.StatusCode == 200)
            {
                await user.AddRoleAsync(RoleId);
                await command.RespondAsync("Success!");
            }
            else if (response.StatusCode == 409)
            {
                await command.RespondAsync("It appears this account is already linked to a Discord account. Please contact support if this is a mistake...");
            }
            else if (response.StatusCode == 500)
            {
                await command.RespondAsync("I ran into an internal server problem! If it has not been reported, please report it to support so we can sort it out!: 500");
            }
            else if (response.StatusCode == 404)
            {
                await command.RespondAsync("I couldn't find your account! Maybe try activating with an email address or VRChat ID...");
            }
            else
            {
                await command.RespondAsync($"Something went wrong! I don't understand this error code, please report it to support!: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            await command.RespondAsync("uh oh" + e);
        }
    }

    internal async Task HandleEmailActivation(SocketSlashCommand command)
    {
        _client.MessageReceived += HandleMessageReceivedAsync;
        var user = command.User;
        // Mark this user as awaiting an email response
        _awaitingEmailConfirmation[user.Id] = true;

        var dmChannel = await user.CreateDMChannelAsync();
        await dmChannel.SendMessageAsync("Hello! Please reply with your email address for verification.");
    }
    private async Task HandleMessageReceivedAsync(SocketMessage message)
    {
        // Ignore messages from the bot itself or non-DM messages
        if (message.Author.Id == _client.CurrentUser.Id || !(message.Channel is IDMChannel)) return;

        // Check if this user was expected to respond with an email
        if (_awaitingEmailConfirmation.TryRemove(message.Author.Id, out _))
        {
            // Here, add logic to validate the message as an email address
            Console.WriteLine($"Received expected DM: {message.Content}");

            var isValidEmail = EmailValidation.IsValidEmail(message.Content);

            if (!isValidEmail)
            {
                await message.Channel.SendMessageAsync("This doesn't appear to be a valid email, please try again...");
                return;
            }
            
            await message.Channel.SendMessageAsync("Thank you for your email! We will process this shortly.");

            var sEmail = EmailSanitization.SanitizeEmail(message.Content);

            var content = new EmailActivationRequest()
            {
                DiscordUsername = message.Author.Username,
                Email = sEmail
            };
            
            var contentString = JsonConvert.SerializeObject(content);
            var url = "http://10.1.1.240:58082/v3/discord/activate/email";

            var response = await WebRequestHelper.MakePostRequestAsync(url, contentString);
            
            if (response.StatusCode == 200)
            {
                await message.Channel.SendMessageAsync("Success!");
                await HandleUserAddRole(message.Author.Id);
            }
            else if (response.StatusCode == 409)
            {
                await message.Channel.SendMessageAsync("It appears this account is already linked to a Discord account. Please contact support if this is a mistake...");
            }
            else if (response.StatusCode == 500)
            {
                await message.Channel.SendMessageAsync("I ran into an internal server problem! If it has not been reported, please report it to support so we can sort it out!: 500");
            }
            else if (response.StatusCode == 404)
            {
                await message.Channel.SendMessageAsync("I couldn't find your account! Maybe try activating with an email address or VRChat ID...");
            }
            else
            {
                await message.Channel.SendMessageAsync($"Something went wrong! I don't understand this error code, please report it to support!: {response.StatusCode}");
            }
            
        }
        // If the message is not from a user who was asked for an email, you might choose to ignore it or handle it differently
    }

    internal async Task HandleUserAddRole(ulong userId)
    {
        var guildId = _guildId;
        var guild = _client.GetGuild(guildId);
        var user = guild.GetUser(userId);
        await user.AddRoleAsync(RoleId);
    }
}

public class DiscordRequest
{
    public string DiscordUsername { get; set; }
}

public class UsernameActivationRequest
{
    public string DiscordUsername { get; set; }
    public string Username { get; set; }
}

public class EmailActivationRequest
{
    public string DiscordUsername { get; set; }
    public string Email { get; set; }
}