EXTERNAL DisableAllUI(exceptions)
EXTERNAL EnableUI(elements)
EXTERNAL FocusUI(elements)
EXTERNAL UnfocusUI(elements)
EXTERNAL FocusPlayer(name)
EXTERNAL ForceSelectionTile(x, y)

~ FocusPlayer("WK")

~ DisableAllUI("")

~ EnableUI("SelectedPlayer_Stats, PlayerName, CharacterFace, Movement, End Turn Button")

Naturally, missions aren’t walks in the park. The Company has hired <b>Guards</b> to protect their property overnight.

<b>Guards</b> will take their turn after yours. Most patrol around the area; you can preview their path by hovering over them.

These people just work here. Remember that. As obstructive as their presence may be, you should always <b>avoid altercations</b> with them.

If a guard spots someone in their <color="red">field of vision</color>, they’ll follow Company policy and chase them down. They’re faster than you.

~ EnableUI("Health")

~ FocusUI("Health")

When they catch up, they’ll resort to violence. If anyone’s <color="red">Health Points (HP)</color> reaches zero, you’ll have to abort the mission.

Fortunately, the crew has come prepared with countermeasures.

~ UnfocusUI("Health")

~ EnableUI("Player Hands")

~ FocusUI("Player Hands")

Each character has a unique hand of <b>Cards.</b> They have a variety of beneficial effects. Consider them a toolkit.

You can right-click any the card to enlarge it and see it in more detail. You may also hover over the symbols and keywords to view tooltips that explain what they mean.

~ UnfocusUI("Player Hands")

~ EnableUI("Energy")

~ FocusUI("Energy")

Cards cost <color="blue">Energy Points (EP)</color> to use. Like <color="yellow">MP</color>, you can fully restore everyone’s <color="blue">EP</color> by ending your turn.

Let’s spend some right now.

Unlike most guards, the guard in WK’s path won’t budge. Heading straight for the extraction point is out of the question.

~ UnfocusUI("Energy")

~ FocusUI("Player Hands")

~ ForceSelectionTile(7, 1)

Fortunately, WK has a Distraction Card in their hand. Distractions allow you to direct and mislead guards.

Select the card from WK’s hand, then select the highlighted tile.
