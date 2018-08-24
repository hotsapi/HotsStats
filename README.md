HotsStats [![Build status](https://ci.appveyor.com/api/projects/status/l28bqoc2yvx1420q?svg=true)](https://ci.appveyor.com/project/poma/hotsstats/branch/master)
===========
This tool shows players MMR and other useful stats during the loading screen in Heroes of the Storm. [Blizzard commented](https://www.reddit.com/r/heroesofthestorm/comments/4ahsxj/hotsstats_version_03_is_out/d10p5k1) that they are not going to ban players for using this app.

Installation
---------------------------
- Requirements:
  - Windows Vista or higher
  - .NET Framework 4.5 or higher
- Set HotS video mode to Windowed (Fullscreen) to allow other windows to be displayed on top of HotS client
- [__Download__](https://github.com/Poma/HotsStats/releases) **"Setup.exe"** from [Releases](https://github.com/Poma/HotsStats/releases) page (you don't need to download other files listed there)
- Run the "Setup.exe"

Screenshots
---------------------------
![Screenshot1](https://cloud.githubusercontent.com/assets/2109710/13773994/7893fdc6-eaad-11e5-99b9-d913f2551cd5.png)
![Screenshot2](https://cloud.githubusercontent.com/assets/2109710/13773945/35ccde54-eaad-11e5-8f35-79312addead6.png)
![Screenshot3](https://cloud.githubusercontent.com/assets/2109710/14224392/f1ad449e-f8a2-11e5-8d95-6c4a2a83d5ef.png)

Available stats
--------------------------
#### Stage 1 - loading screen
At this point we are able to extract only the current region and BattleTags. We don't know the current map or the heroes being played. So we can show only general stats:
* MMR
* Games played
* Average team MMR
* Clickable link to HotsLogs profile

#### Stage 2 - during game after 1:00 mark
This is when the well documented replay file is created. Now we know current map, heroes and hero levels. We can also pull additional stats from Hotslogs. Here is what we may be able to display for each player in full table:
* Hero level
* Hero win rate
* Map win rate
* Win rate with current role and sub-role
* Current team composition win rate (yay less whining about comps!)

This window functions like built-in stats that can be shown with Shift-Tab hotkey. I'd be happy to move those stats to loading screen if we find a way to get map and heroes data from battlelobby file (or maybe use OCR).

**- What about advanced stats during match?**

-- No there will not be any dynamic stats within this project because 1. You can't obtain this info manually and 2. Going down this road leads to all sorts of unfair tools like map hacks.

#### Stage 3 - after match ended
Now that we have a full replay file we can display tons of detailed stats like damage taken, self healing, objective counters (collected/lost coins, damage done to immortals etc). I hope that with help of the community we can develop an extensive replay analyzer lib that can be used by this app and hotslogs.

Development details
------------------
Here's how the app currently works. During loading screen the game creates the battlelobby and tracker files. Battlelobby is one of a few replay files not documented by Blizzard. Since I have no idea about the file structure I just search for anything that looks like a battle tag. If anyone finds how to extract current map and heroes from this file it would be awesome. Detailed info about replay files can be found on [wiki page](https://github.com/poma/HotsStats/wiki/Details-on-partial-replays)

Then I get MMR from HotsLogs JSON API. For more stats such as total games count and heroes win rate we have to parse HTML profile.

Code style
-------------------
While I'm the only one actively developing this project I use coding style that I like. Later on if there will be more contributors I plan to convert it to common conventions. Currently I use ~~tabs~~ spaces for indentation and C style brackets.
