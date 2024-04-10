using Discord.Interactions;
using Discord.WebSocket;

namespace AW_Daemon.Core;

public class SlashCommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly ActivationHandler _activationHandler;

    private const ulong RoleTestId = 701517732159422526;

    public SlashCommandHandler(DiscordSocketClient client, InteractionService interactionService, ActivationHandler activationHandler)
    {
        _client = client;
        _interactionService = interactionService;
        _activationHandler = activationHandler;
    }

    public async Task SlashCommand(SocketSlashCommand command)
    {
        try
        {
            // Directly use command.Guild to access the SocketGuild as needed within this method
            switch (command.Data.Name)
            {
                case "test":
                    await TestCommand(command);
                    break;
                case "verify":
                    await ActivationCommand(command);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling command: {ex.Message}");
        }
    }

    private async Task TestCommand(SocketSlashCommand command)
    {
        Console.WriteLine("Test Command Executed");
        await command.RespondAsync("Wow!");
    }

    private async Task ActivationCommand(SocketSlashCommand command)
    {
        Console.WriteLine("Running activation command...");
        var commandType = command.Data.Options.First().Value.ToString();
        ulong.TryParse(commandType, out ulong commandOption);

        try
        {
            switch (commandOption)
            {
                case 1:
                    await _activationHandler.HandleDiscordUsernameActivation(command);
                    break;
                case 2:
                    await _activationHandler.HandleEmailActivation(command);
                    break;
                default:
                    await command.RespondAsync("That isn't a valid option!");
                    return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling command: {ex}");
            await command.RespondAsync("An error occurred while processing your command.");
        }
    }
}
