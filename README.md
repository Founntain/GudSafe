# GudSafe

GudSafe is a self hostable open source file uploader using asp.net core being platform independent. GudSafe is mainly developed by [@Cesan](https://github.com/Cesan) and [@Founntain](https://github.com/Founntain).

If you want to contribute to the project, read the [contributing information](https://github.com/Founntain/gudsafe#-contributing-to-the-project) and follow the steps described there.

## Requirements

#### GudSafe requirements

- A working computer (best would be a server, not your private pc)
- .NET 6 or later installed
	> Follow Microsoft's instructions on how to install it on your platform
- Use a reverse proxy for the GudSafe instance
	> This is not a definitive requirement but our configuration is built to use one.
	> 
	> To configure the reverse proxy correctly best is to follow these steps provided by Microsoft [Configure Nginx](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-6.0#configure-nginx) if you are using nginx (recommended)

#### Download GudSafe
You can download the latest release on the repos [releases page](https://github.com/Founntain/gudsafe/releases). If you want to build the project from source follow the steps below.

## Building the project
- Clone or download the source
- Open the solution file (.sln) with Visual Studio or Jetbrains Rider.
- If you use an IDE you can just use the build option there, everything should be done automatically
	> Alternatively you can use the dotnet cli by using *dotnet restore* and *dotnet build* in the project root
	> More information on this can be found in Microsoft documentation
- After you have built the project. You can run the executable found in the *GudSafe.WebApp/bin/(Debug or Release)/net6.0* folder

## Contribute
#### Requirements
- Everything needed to build the project
- Know how to write asp.net web apps using mvc and razor
	> You can also support by contributing design or client sided js/ajax which does not require this as much

#### How to contribute
- Make a fork of this repository
- Best to focus implementing/fixing stuff from the github issues
- Implement own ideas and features
	> Best would be to discuss this with us first to avoid making you work 
- Make a pull request on this repository
- Wait for a pull request review
	> This can take some time as we have work to do outside of this and are doing this in our free time.
- If changes are requested this is not to waste your time but to make the code fit better to what we want in our project

#### Other
If you have a cool idea for a feature but can't implement it yourself, open an issue so we can discuss if it fits in the project and we can do it for you.