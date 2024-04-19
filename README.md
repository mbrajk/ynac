## You Need A Console

### Purpose

This is a console application that uses Spectre Console to create a visually appealing CLI that connects to the YNAB Api and displays basic budget information.


| :exclamation: | Construction ahead |
|---------------|--------------------|

This is a proof of concept and not a stable release and will be very rough around the edges. Expect to run into issues and limitations if you use this application. 
This is just a personal project, but I am open to any feedback and issue requests.

### Requirements
- Dotnet Core
- Windows (for now)
- YNAB account w/ devloper access (api key)

### Usage
- Turn on developer access through YNAB
- Grab your api key
- Install dotnet core
- Download the git repo
- Navigate to the download folder
- Place the api key in the `appsettings.json` file
- Type `dotnet run` on the command line in the root folder or run in VS/VSCode/Rider/etc.
- Select a budget by typing the number of the budget, alternatively you can provide the budget name as the first argument after dotnet run. E.g. `dotnet run mybudget`. The first budget containing the provided string will be retreived, if one exists.
- Your budget should be displayed

### Libraries
- Spectre.Console
- Refit
- Polly

### Potential Improvements
- Multi-platform
- Install as a dotnet tool or provide alternate standalone solution
- Include additional features from the YNAB Api
- Further details about the existing commands
- Provide write commands (e.g. approving or categorizing a transaction)
- Refactoring the code. It started as a single file and it is my first attempt at using vim motions/neovim so idk how to do stuff.
