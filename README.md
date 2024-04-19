## You Need A Console

![Console Example](https://raw.githubusercontent.com/mbrajk/ynac/main/res/ynac-output.png "console example")

### Purpose

A console application that uses Spectre Console to create a visually appealing CLI that connects to the YNAB API and displays basic budget information.


| :exclamation: | Construction ahead |
|---------------|--------------------|

This is a proof of concept and not a stable release and will be very rough around the edges. Expect to run into issues and limitations if you use this application. 
This is just a personal project, but I am open to any feedback and issue requests.

### Requirements
- [Microsoft Dotnet](https://dotnet.microsoft.com/en-us/)
- Windows (for now)
- YNAB account w/ a Developer Personal Access Token.
  - Instructions: [https://api.ynab.com/](https://api.ynab.com/)

### Usage
- Create a YNAB Developer Personal Access Token ( instructions above )
- Copy the token you created. You cannot access it again and must generate a new one if you do not save it.
- Install dotnet core
- Download the git repo
- Navigate to the download folder
- Paste the Personal Access Token from YNAB into the `appsettings.json` file in the `Token` field under `YnabApi`
- Type `dotnet run` on the command line in the root folder or run in VS/VSCode/Rider/etc.
- Select a budget by typing the number of the budget
  - Alternatively, you can provide the budget name as the first argument after dotnet run. E.g. `dotnet run mybudget`.
  - The first budget containing the provided string will be retrieved if it exists.
- Your budget should be displayed on the console 🥳

### Libraries
- [Spectre.Console](https://spectreconsole.net/)
  - The reason the console looks good at all
- [Refit](https://github.com/reactiveui/refit)
  - Automatically generate REST API implementation from an interface
- [Polly](https://github.com/App-vNext/Polly)
  - Streamlined web request resiliency strategies

### Potential Improvements
- Multi-platform
- Install as a dotnet tool or provide an alternate standalone solution
- Include additional features from the YNAB API
- Further details about the existing commands
- Provide write commands (e.g. approving or categorizing a transaction)
- Refactoring the code. It started as a single file and is my first attempt at using vim motions/neovim so idk how to do stuff.
