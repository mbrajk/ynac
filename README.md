## You Need A Console

![Console Example](https://raw.githubusercontent.com/mbrajk/ynac/main/res/ynac-output.png "console example")

### Purpose

A multi-platform console application that uses Spectre Console to create a visually appealing CLI that connects to the YNAB API and displays basic budget information. Verified to work on Windows, Mac and Linux.


| :warning: | Construction ahead |
|---------------|--------------------|

This project is still a work in progress and although it has a full release, this is just a personal project. Therefore it may have rough edges and incomplete features.
Feel free to suggest features, provide feedback, or report bugs on the [issue tracker](https://github.com/mbrajk/ynac/issues).

## Running from Binaries
### Requirements
- Windows/MacOS/Linux (tested to work on Sequoia on Apple Silicon and on Ubuntu)
- YNAB account w/ a Developer Personal Access Token.
  - Instructions: [https://api.ynab.com/](https://api.ynab.com/)
- Binaries are self-contained executables and do not require installing the dotnet runtime
  - Eventually a framework-dependent binary will be provided to save space for users that already have the runtime

### Usage
- Download the latest stable release [executable](https://github.com/mbrajk/ynac/releases)
- Open a Terminal
- Navigate to the folder where you downloaded or saved the `ynac` executable
- Run `./ynac.exe` (Windows) or `./ynac` (MacOS/Linux) 
- You will be prompted for your YNAB API token. It will be persisted to `config.ini` for following runs so you do not need to input it every time
- You will be presented with the budget selection options
  - Alternatively you can provide your budget name as the first argument to ynac (e.g. `ynac mybudget`)
  - This works like a search so you can provide a partial name
  - If only one budget is found it will be opened immediately
- Using the exe from any location
  - Place the executable somewhere on your existing path or
  - Add the folder containing the `ynac` binary to your path

![Budget Selection](https://raw.githubusercontent.com/mbrajk/ynac/main/res/ynac-budget-select.png "budget selection")

## Development
:warning: The following instructions are only needed if you would like to develop or debug the application or otherwise prefer to run it through .NET

### Requirements
- [Microsoft Dotnet 10](https://dotnet.microsoft.com/en-us/)
- YNAB account w/ a Developer Personal Access Token.
  - Instructions: [https://api.ynab.com/](https://api.ynab.com/)

### Usage
- Create a YNAB Developer Personal Access Token ( instructions above )
- Copy the token you created. You cannot access it again and must generate a new one if you do not save it.
- Install either the dotnet 10 SDK or Runtime depending on if you want to develop or simply run the application
  - Both can be found here: [Dotnet 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Download this git repo
- Navigate to the download folder
- Paste the Personal Access Token from YNAB into the `config.ini` file in the `Token` field under `YnabApi`
- Type `dotnet run` on the command line in the root folder or run in VS/VSCode/Rider/etc.
- You will be presented with the budget selection options
- Select a budget and it will be displayed on the console 🥳

### Spectre.Console
- [Spectre.Console](https://spectreconsole.net/)
  - The library driving the console styling

### Potential Improvements
- Include additional features from the YNAB API
- Further details about the existing commands
- Provide write commands (e.g. approving or categorizing a transaction)
- Further improvements detailed on the [issue tracker](https://github.com/mbrajk/ynac/issues)

For AI tooling, refer to the current AI instructions: [AI_INSTRUCTIONS.md](./AI_INSTRUCTIONS.md).
