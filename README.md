# Chinook

1.	Move data retrieval methods to separate class / classes (use dependency injection)
=>	Moved methods to different base classes
	
2.	Favorite / unfavorite tracks. An automatic playlist should be created named "My favorite tracks"
=>	This feature has been implemented.

3.	Search for artist name
=>	Separate component called "SearchArtist" was made. Data is passed to other components via EventCallback.

4.	The user's playlists should be listed in the left navbar. If a playlist is added (or modified), this should reflect in the left navbar (NavMenu.razor). Preferrably, this list should be refreshed without a full page reload. (suggestion: you can use Event, 	Reactive.NET, SectionOutlet, or any other method you prefer)
=>	It was done using CascadingParameter. To maintain the state details, a separate class called "ApplicationStateChange" was created.

5.	Add tracks to a playlist (existing or new one). The dialog is already created but not yet finished.
=>	This feature has been implemented.

6.  Exception handling should be implemented.
=>  Exception handling has been implemented at the page level.Null exceptions are also handled at the same level.

7.  Error handling
=>  Utilised NLog as a source for logging

I just followed only those point was mentioned. There was no change made to the application's architecture (CLEAN artcure, CQRS, etc.). I did not use any service layer in order to maintain data retries. Just added those methods to the base classes and service classes.
