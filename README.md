## You Need A Console

// image

### Purpose

This is a console application that connects to the YNAB Api and displays basic budget information on the console. This is an alpha release and will be very rough around the edges. Expect to run into issues and limitations if you use this application. 
This is just a personal project but I am open to any feedback and issue requests.

### Usage
- Install dotnet core
- Download the git repo
- Navigate to the download folder
- Type `dotnet run`
- Select a budget by typing the number of the budget, alternatively you can provide the budget name as the first argument after dotnet run.
- Your budget should be displayed

### Requirements
Dotnet Core
Windows (for now)
YNAB account w/ devloper access (api key)
//detail the api key process

### Libraries
Spectre.Console
Refit

### Planned Improvements
- Multi-platform
- Install as a dotnet tool or provide standalone exe
- Include additional features from the YNAB Api
- Futher details about the existing commands
- Provide write commands (e.g. approving or categorizing a transaction)
- Refactoring the code. It started as a single file and it is my first attempt at using neovim so idk how to do stuff.
