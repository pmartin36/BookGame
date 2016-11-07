using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CompendiumEntry {

	public string letter {get;set;}
	public string PowerupName {get; set;}
	public Sprite Illustration {get; set;}
	public string Description {get; set;}

	public CompendiumEntry (string s){
		letter = s;
		Sprite illus = null;
		switch (s) {
		default:
			break;
		case "A":
			PowerupName = "Air";
			Description = "Blows player in direction opposite bellows.  Direct Bellows using Mouse Cursor or Control Stick.";
			illus = Resources.Load<Sprite> ("Sprites/A_Illustration");
			break;
		case "B":
			PowerupName = "Bouncy Platform";
			Description = "Places a platform below the player that can be bounced off of. Must be airborn when placing.";
			illus = Resources.Load<Sprite> ("Sprites/B_Illustration");
			break;
		case "C":
			PowerupName = "Copy";
			Description = "Makes a copy of your left slot item. Copies number of charges as well.";
			illus = Resources.Load<Sprite> ("Sprites/C_Illustration");
			break;
		case "D":
			PowerupName = "Douple Jump";
			Description = "Provides a second jump. Can be activated with the jump button or the item slot button.";
			illus = Resources.Load<Sprite> ("Sprites/D_Illustration");
			break;
		case "E":
			PowerupName = "Eraser";
			Description = "Throw eraser at letter. When hit, the letter gets erased. If gold letter, it no longer needs to have collected.";
			illus = Resources.Load<Sprite> ("Sprites/E_Illustration");
			break;
		case "F":
			PowerupName = "Floating Platform";
			Description = "Places a platform below the player than can be stood on.  Must be airborn when placing.";
			illus = Resources.Load<Sprite> ("Sprites/F_Illustration");
			break;
		case "G":
			PowerupName = "Grapple";
			Description = "Throw a grappling hook at letter. When hit, pulls the player towards the spot. Number of available jumps is preserved";
			illus = Resources.Load<Sprite> ("Sprites/G_Illustration");
			break;
		case "H":
			PowerupName = "Higher";
			Description = "Throw at letter. When hit, letter will begin to rise to the top of the level but will collide with other letters.";
			illus = Resources.Load<Sprite> ("Sprites/H_Illustration");
			break;
		case "I":
			PowerupName = "Ice";
			Description = "Slippery movement. Decreased acceleration.";
			illus = Resources.Load<Sprite> ("Sprites/I_Illustration");
			break;
		case "J":
			PowerupName = "Jump High";
			Description = "Jump almost 2 times higher than normal";
			illus = Resources.Load<Sprite> ("Sprites/J_Illustration");
			break;
		case "K":
			PowerupName = "Key";
			Description = "Unlock areas protected by keyholes";
			illus = Resources.Load<Sprite> ("Sprites/K_Illustration");
			break;
		case "L":
			PowerupName = "Lower";
			Description = "Throw at letter. When hit, letter will begin to lower to the bottom of the level but will collide with other letters.";
			illus = Resources.Load<Sprite> ("Sprites/L_Illustration");
			break;
		case "M":
			PowerupName = "Make Useable";
			Description = "Throw at blue letter. When hit, letter will become collectable (gold). No effect if thrown at gold letter.";
			illus = Resources.Load<Sprite> ("Sprites/M_Illustration");
			break;
		case "N":
			PowerupName = "None";
			Description = "Clears all existing powerups. Can not collect this powerup if player has no existing powerups.";
			illus = Resources.Load<Sprite> ("Sprites/N_Illustration");
			break;
		case "O":
			PowerupName = "Opposite";
			Description = "Reverses player input.";
			illus = Resources.Load<Sprite> ("Sprites/O_Illustration");
			break;
		case "P":
			PowerupName = "Play Again";
			Description = "Restart the level but retain your powerups. P letter will not be collectable again";
			illus = Resources.Load<Sprite> ("Sprites/P_Illustration");
			break;
		case "Q":
			PowerupName = "Quick";
			Description = "Increases movement speed";
			illus = Resources.Load<Sprite> ("Sprites/Q_Illustration");
			break;
		case "R":
			PowerupName = "Rotate";
			Description = "Rotate the letter 90 degrees counter(anti)clockwise. Will collide with other letters.";
			illus = Resources.Load<Sprite> ("Sprites/R_Illustration");
			break;
		case "S":
			PowerupName = "Slow";
			Description = "Decreases movement speed";
			illus = Resources.Load<Sprite> ("Sprites/S_Illustration");
			break;
		case "T":
			PowerupName = "Timer";
			Description = "Once collected you must end the level or get rid of this powerup within 10 seconds.";
			illus = Resources.Load<Sprite> ("Sprites/T_Illustration");
			break;
		case "U":
			PowerupName = "Umbrella";
			Description = "Slow fall speed. Cannot change movement direction while deployed. Can be deployed multiple times during a jump. Can only be used while airborn.";
			illus = Resources.Load<Sprite> ("Sprites/U_Illustration");
			break;
		case "V":
			PowerupName = "Vapor";
			Description = "Camera effect which makes it difficult to tell where the player is in relation to the level";
			illus = Resources.Load<Sprite> ("Sprites/V_Illustration");
			break;
		case "W":
			PowerupName = "Wall Grab";
			Description = "Grab on to a section of a letter that is fully vertical. Can be used multiple times in a row to climb walls.";
			illus = Resources.Load<Sprite> ("Sprites/W_Illustration");
			break;
		case "X":
			PowerupName = "eXtra";
			Description = "Gives one more charge to each charge based powerup the player possesses.";
			illus = Resources.Load<Sprite> ("Sprites/X_Illustration");
			break;
		case "Y":
			PowerupName = "Yank";
			Description = "Throw at letter. When hit, letter begin to move towards player's horizontal position.  Will collide with other letters.";
			illus = Resources.Load<Sprite> ("Sprites/Y_Illustration");
			break;
		case "Z":
			PowerupName = "Zoom";
			Description = "Zoom out to see more of the level.";
			illus = Resources.Load<Sprite> ("Sprites/X_Illustration");
			break;
		}

		if (illus != null) {
			Illustration = illus;
		}
		else {
			Illustration = null;
		}
	}
}
