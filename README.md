# SVSim Server
This repository holds the SVSim server code. This includes serveral major components:
* **SVSim.EmulatedEntrypoint** - The main API
* **SVSim.BattleEngine** - The engine that drives multiplayer battles. Currently implemented as a headless, stripped down version of the game client thanks to the assistance of the torment nexus. Probably should be rewritten from scratch in the future.
* **SVSim.BattleNode** - Project that holds the websocket handling for battles. Generally feeds commands into the battle engine and then sends out responses to connected clients. Open rooms use a similar websocket method but may be better in their own project when implemented so there's no overlap.
* **SVSim.ContentServer** - Another 'API', this is treated as a filesystem passthrough pretty much for a specific directory. This should eventually be a proxy for an S3 or something but that's a later problem.
* **SVSim.Database** - Class library holding database models and DB structure migrations using EFCore.
* **SVSim.Bootstrap** - Bootstraps an initial DB with appropriate data taken from prod (and some that i made up!).

## Server Operators
This will be fleshed out when I feel like writing more, so right now you may need to be a bit technical to set this up and figure it all out locally.

Database can be bootstrapped via the following:

`dotnet run --project SVSim.Bootstrap`

Connection string is configured in appsettings.json or with adding `--connection-string` as a parameter. The `appsettings.json` route is recommended as that's also what the API will connect to.

The emulated entrypoint has a swagger page. Most of it is not very useful, but there's an explicit admin endpoint for importing user json dumps from the client loader. This is how you would import a dumped user json. Authentication is at the top right, or you can use curl/postman. Auth secret is configured in `appsettings.json` as well.

Battle node needs your IP or domain configured in the appsettings.json for `SV.EmulatedEntrypoint` so the main API knows what IP or domain to hand over.

Great majority of configuration can be done via the DB (GameConfig table, or one of the other ones depending), with some appsetting overrides if needed.

Freeplay mode can be enabled in the appropriate gameconfig, gives everyone unlimited currency and all cards and cosmetics. Mostly for testing or if you just want everyone to have everything.

Content server is configured just via appsettings, and acts like a filesystem passthrough proxy. The files are expected to be in the same format they were in for the official SV content files. I'll make those files available at some point since there's a lot and it's not appropriate to have them on github. Without the content server being filled out, people will need to have all the assets already to access the game properly (and downloading voices for the story wont work).

## Contributing
Just fork it, I really don't want to review PRs.