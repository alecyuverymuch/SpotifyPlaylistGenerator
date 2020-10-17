# SpotifyPlaylistGenerator

A .NET Core Console Application that creates randomized playlists based off a set of user provided source profiles using the Spotify Developer API. 

Requirements: 
- .Net Core SDK 3.1+
- A Spotify account

To run this application, you first need to (register an application)[https://developer.spotify.com/documentation/general/guides/app-settings/] in the Spotify Developer's Dashboard to acquire a Client Id and a Client Secret.
You must also whitelist a callback URL. Then add your client id, client secret, and callback url to the SpotifyAuthenticationConfiguration field in the appsettings.json file.

When running the application, you must first perform the log in command before you are allowed to use any other options. 
Then, you must create a new playlist profile and select sources (existing playlists, artists, or your top tracks) to seed songs from. 
After you run the playlist generation, it will create a new playlist registered to your Spotify account and randomly add songs from the sources you configured.
