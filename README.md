# EasyMusicBot
This is a music bot for Discord, a voip service. It accepts text commands thru both Discord chat and the built in GUI.

#Commands
Request (Add to playlist either URL or search term):
Usage: !request https://www.youtube.com/watch?v=QH2-TGUlwu4, !request nyan cat
  !songrequest
  !reqeust
  !play
  !sr

Skip (Removes the current song or a specified song from the playlist):
Usage: !skip, !skip 1 (Skips song with index of 1)
  !skip
  !next
  this song sucks
  !remove

Playlist (Shows the current playlist with indexes):
Usage: !playlist

Play / Pause (Plays or pauses the current song)
Usage: !play, !pause

Config: Change config settings. Does not actually write to config file, changes will be reset on application restart
Usage: 
  !config requestRole @everyone (Sets the role needed to request to @everyone)
  !config skipRole Skip (Sets the role needed to skip songs)
  !config maxLength 3600 (Sets the maximum length to 3600 seconds)
  !config spChannel false (Allows use of Musicbot in any channel)
  !config channel songrequest (Sets the channel to use to "songrequest")
