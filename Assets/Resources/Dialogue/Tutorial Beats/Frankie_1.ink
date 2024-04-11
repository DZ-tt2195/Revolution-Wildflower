EXTERNAL EnableUI(elements)
EXTERNAL DisableAllUI(exceptions)
EXTERNAL FocusUI(elements)
EXTERNAL ForceMovementTile(x, y)
EXTERNAL FocusPlayer(name)
EXTERNAL ChainTutorial(fileName, className, eventName)

~ FocusPlayer("Frankie")

~ DisableAllUI("")
To clear a mission, you must complete all objectives and successfully reach an extraction point.

Objectives are specific points of interest spread across the map. You can only complete an objective once a character has reached an adjacent tile.

~ EnableUI("SelectedPlayer_Stats, PlayerName, CharacterFace, Movement")

~ FocusUI("Movement")

~ ForceMovementTile(5, 1)

A character's <color="yellow">Movement Points (MP)</color> determine how far they can move in a turn.

Let's spend some of Frankie's MP to get him next to the objective. That desk with the computer looks promising.

~ ChainTutorial("Frankie_2", "PhaseManager", "MovementCompleted")

Click on Frankie to preview his range of movement, then confirm his move by selecting the highlighted tile.
