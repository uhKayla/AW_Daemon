# AW_DAEMON

A Discord bot for verifying user accounts from ANGELWARE services in Discord servers.

Commands
- /verify - Verifies the user's account exists.

Verification can use the user's Discord name or the user's email address in a direct message channel. The bot then sends a POST request to our API and depends on a 200 response for success. Other HTTP responses are optional and result in different return messages.

## To run
- Enter your TOKEN in a .env file in the project directory `./AW_Daemon/.env`
- Run via Docker Compose or build and run directly within your IDE. To run standalone please refer to your operating system to set the environment variable (not recommended).

