using UnityEngine;
using System.Collections;

namespace CW
{
	public class BattlePlayer : MonoBehaviour
	{
	
		//CLASS VARIABLES
		public ArrayList deck;
		public ArrayList hand, cardsInPlay, GraveYard, treeID;
		public Vector3 DeckPos, handPos, FieldPos, TreePos;
		public AbstractCard clickedCard, targetCard;
		public bool player1, isReady, isActive, hasDeck, isGameOver = false, isWon = true;
		public int cardID;
		public int count = 0, currentMana, maxMana;
		private int showTurn = -1;
		private ProtocolManager protocols;
		private GameObject manaObj, gameOver;
		private DeckData deckData;
		public int playerID;
		public int matchID;
		public string playerName;

		public ProtocolManager getProtocolManager ()
		{
			return protocols;
		}

		//Initializes the player variables
		public void init (bool player1)
		{
			// get protocolManager from GameManager
			protocols = GameManager.protocols;

			deck = new ArrayList ();
			hand = new ArrayList ();
			GraveYard = new ArrayList ();
			cardsInPlay = new ArrayList ();
			treeID = new ArrayList ();
		
			//Set's player's coordinates of interest for p1 and p2
			setPlayerNum (player1);

			//Creates the player's tree and mana displayer
			createTree ();
			createMana ();
		}

		public GameObject instantiateCard ()
		{
			return (GameObject)Instantiate (Resources.Load ("Prefabs/Battle/Card"));
		}
	
		//Sets the starting mana for each player as well
		public void setPlayerNum (bool isPlayer1)
		{
		
			//Boolean so client can know which is the player and the Ghost Player
			//Ghost Player = player over the net
			this.player1 = isPlayer1;
		
			//PLAYER 1 COORDINATES OF INTEREST
			if (player1) {
				DeckPos = new Vector3 (825, 10, -400);
				handPos = new Vector3 (150, 10, -375);
				FieldPos = new Vector3 (-450, 10, -150);
				TreePos = new Vector3 (-800, 10, -300);
			
				//Player 1 takes first turn every time
				currentMana = 1;
				maxMana = 1;
				isActive = true;
			
				//PLAYER 2 COORDINATES OF INTEREST
			} else {

				Debug.Log ("Set PLayer Num Working");
				handPos = new Vector3 (150, 10, 400);
				FieldPos = new Vector3 (-450, 10, 150);
				DeckPos = new Vector3 (-825, 10, 400);
				TreePos = new Vector3 (800, 10, 300);
			
				//Mana and sets p2's inactive to false
				currentMana = 1;
				maxMana = 1;
				isActive = false;

			}
		}




		//Instantiates the Tree with Tree script
		public void createTree ()
		{
			GameObject obj = (GameObject)Instantiate (Resources.Load ("Prefabs/Battle/Tree"));
			obj.AddComponent ("Trees");
			Trees script = obj.GetComponent<Trees> ();
			script.init (this);
			treeID.Add (obj);
		}


		//Creates a visual for the text that displays how much mana a player has
		private void createMana ()
		{
		
			//Instantiates the Graphic for the Mana and sets it's position
			manaObj = (GameObject)Instantiate (Resources.Load ("Prefabs/Battle/Mana"));
			if (player1) {
				manaObj.transform.position = new Vector3 (TreePos.x + 200, TreePos.y, TreePos.z);
			} else {
				manaObj.transform.position = new Vector3 (TreePos.x - 200, TreePos.y, TreePos.z);
			}
		}


		//Instantiate's the GameOver button 
		public void createGameover ()
		{
			int gold = 100; //100 gold if won
			isGameOver = true;
			Debug.Log ("Battleplayer game_over");

			gameOver = (GameObject)Instantiate (Resources.Load ("Prefabs/Battle/GameOver"));
			if (!isWon) {
				gold = 25;//25 gold if lost
				Texture2D loseTexture = (Texture2D)Resources.Load ("Prefabs/Battle/lose", typeof(Texture2D));
				gameOver.renderer.material.mainTexture = loseTexture;
			} else {
				Texture2D winTexture = (Texture2D)Resources.Load ("Prefabs/Battle/win", typeof(Texture2D));
				gameOver.renderer.material.mainTexture = winTexture;
			}
			//gameOver.transform.Find ("GameOverText").GetComponent<TextMesh> ().text = "You've been awarded " + gold + " gold";
			//gameOver.transform.position = new Vector3 (0, 30, 0);
			// return player to lobby
			GameManager.protocols.sendQuitMatch (playerID);
		}

		//Method Instantiates the cards with AbstractCard scripts from the deckData
		//passed in from server and adds the new card to the deck arraylist
		public void createDeck ()
		{		
			//For loop for every card in deck passed in from server
			for (int i = 0; i < deckData.getSize(); i++) {
			
				//GameObject instantiated for Card
				GameObject obj = (GameObject)Instantiate (Resources.Load ("Prefabs/Battle/Card"));
			
				//Card back for deck
				//GameObject cardBacks = (GameObject) Instantiate(Resources.Load("Prefabs/Battle/card_old"));

				//Card front for hand
				//obj.AddComponent("AbstractCard");
				AbstractCard script = obj.GetComponent<AbstractCard> ();
			
				//CardData contains all the variables for an indivdual card
				CardData tempCard = deckData.popCard ();
				//public void init(BattlePlayer player, int cardID, int diet, int level, int attack,
				//int health,string species_name, string type, string description
				script.init (this, tempCard.cardID, tempCard.dietType, tempCard.level, tempCard.attack, 
			             tempCard.health, tempCard.speciesName, "Large Animal", tempCard.description);

				//Add the card to the deck arraylist
				deck.Add (obj);
			
			}
		
			//Makes the deck 
			GameObject DeckTop = (GameObject)Instantiate (Resources.Load ("Prefabs/Battle/CardBack"));
			DeckTop.transform.position = new Vector3 (DeckPos.x, DeckPos.y, DeckPos.z);
		
		
		}
	
	
	
		//Deals cards to player's hand a
		public void dealCard (int numToDeal)
		{
		
			//Deal as many cards as required
			for (int i = 0; i < numToDeal; i++) {
			
				//Max amount of cards reached
				if (hand.Count >= 7)
					return;
			
				//Card taken from deck, and is given the logic from
				GameObject p1card = (GameObject)deck [0];			
				AbstractCard script = p1card.GetComponent<AbstractCard> ();
				script.handler = new InHand (script, this);
			
				//Position the newly dealt card
				p1card.transform.position = new Vector3 ((handPos.x + 280) - 185 * hand.Count, 10, handPos.z);
			
				//Remove from  deck and add to hand
				deck.Remove (p1card);
				hand.Add (p1card);
				// Send position = hand.Count to Server

				// Hack PlayerID = 0 means its player2
				if (playerID != 0) {
					//protocols.sendDealCard(playerID, hand.Count);

				}
			}

			//For every object in hand arraylist
			for (int i = 0; i < hand.Count; i++) {
			
				//Reposition and arrange each card in hand
				GameObject setCard = (GameObject)hand [i];
				setCard.transform.position = new Vector3 ((handPos.x + 280) - 185 * i, 10, handPos.z);

			}
		}

		public void dealDummyCard (int numToDeal)
		{
			// TODO
			//Deal as many cards as required
			for (int i = 0; i < numToDeal; i++) {
			
				//Max amount of cards reached
				if (hand.Count >= 7)
					return;
			
				//Card taken from deck, and is given the logic from
				GameObject p2card = (GameObject)Instantiate (Resources.Load ("Prefabs/Battle/CardBack"));
				p2card.transform.position = new Vector3 ((handPos.x + 280) - 185 * hand.Count, 10, handPos.z);
				//Position the newly dealt card
				//p2card.transform.position = new Vector3((handPos.x + 280) - 185 * hand.Count, 10, handPos.z);
			
				//Remove from  deck and add to hand
				//deck.Remove (p2card);
				hand.Add (p2card);
				// Send position = hand.Count to Server
			
				// Hack
				//if(playerID != 0){
				//	protocols.sendDealCard(playerID, hand.Count);
				
				//}
			}
		
			//For every object in hand arraylist
			for (int i = 0; i < hand.Count; i++) {
			
				//Reposition and arrange each card in hand
				GameObject setCard = (GameObject)hand [i];
				setCard.transform.position = new Vector3 ((handPos.x + 280) - 185 * i, 10, handPos.z);
			
			}
		}

		public void attackWith (int attackerIndex, int attackedIndex)
		{
			GameObject attackerObj = (GameObject)GameManager.player2.cardsInPlay [attackerIndex];
			AbstractCard attackerCard = attackerObj.GetComponent<AbstractCard> ();
			bool damageBack = false;
		
			GameObject attackedObj = (GameObject)GameManager.player1.cardsInPlay [attackedIndex];
			AbstractCard attackedCard = attackedObj.GetComponent<AbstractCard> ();
			if (attackedCard.diet != AbstractCard.DIET.HERBIVORE) {
				damageBack = true;
				attackerCard.receiveAttack (attackedCard.dmg);
			}
			attackerCard.attack (attackerCard, attackedCard, damageBack);
		
		}

		public void attackTree (int attackerIndex)
		{
			GameObject attackerObj = (GameObject)GameManager.player2.cardsInPlay [attackerIndex];
			AbstractCard attackerCard = attackerObj.GetComponent<AbstractCard> ();
		
			GameObject attackedObj = (GameObject)GameManager.player1.treeID [0];
			Trees tree = attackedObj.GetComponent<Trees> ();
		
			attackerCard.attackTree (tree);
		
		}

		//Called by GameManager when a card is removed from play
		//to keep all the cards neatly arranged preventing
		//cards from stacking on top of each other
		public void reposition ()
		{

			//Position each card in play correctly 
			for (int i = 0; i < hand.Count; i++) {
			
				//Retrieves the card from the array of cards in play
				GameObject obj = (GameObject)hand [i];
				obj.transform.position = new Vector3 ((handPos.x + 280) - 185 * i, 10, handPos.z);
			}

			//Position each card in play correctly 
			for (int i = 0; i < cardsInPlay.Count; i++) {
			
				//Retrieves the card from the array of cards in play
				GameObject obj = (GameObject)cardsInPlay [i];
				obj.transform.position = new Vector3 (FieldPos.x + 185 * i, FieldPos.y, FieldPos.z);

				AbstractCard card = obj.GetComponent<AbstractCard> ();
				card.fieldIndex = i;
			}
		}

		// Added to update player 2 mana only 
		public void addMana ()
		{
			//Increment the max mana
			if (maxMana < 9)
				maxMana++;
		
			//Restore full mana
			currentMana = maxMana;
		}


		// Deal new card for layer
		public void startTurn ()
		{
			showTurn = 120;
			if (hand.Count != 5) {
				dealCard (1);

				//TODO:  Does the player really lose if he gets to 1 card?
				if (deck.Count == 0) {
					Debug.Log ("Player loses");
					createGameover ();
				}
			}

			//Increment the max mana
			if (maxMana < 9)
				maxMana++;
		
			//Restore full mana
			currentMana = maxMana;
		}


		// resets Cards 
		public void endTurn ()
		{
			showTurn = 120;
			Debug.Log ("endTurn called");
			for (int i = 0; i < cardsInPlay.Count; i++) {
				//Gets the AbstractCard component
				AbstractCard cardInPlay = ((GameObject)cardsInPlay [i]).GetComponent<AbstractCard> ();
				cardInPlay.endTurn ();
			}
	
		
		}

		//Resets the players active cards in field 	
		public void resetCards ()
		{
			for (int i = 0; i < cardsInPlay.Count; i++) {
				//Gets the AbstractCard component
				AbstractCard cardInPlay = ((GameObject)cardsInPlay [i]).GetComponent<AbstractCard> ();
				cardInPlay.endTurn ();
			}
		}
	
		private float manaAnimate = 1.0f;
		// Update is called once per frame
		void Update ()
		{
			showTurn--;
			Texture2D manaTexture = (Texture2D)Resources.Load ("Images/Battle/mana" + (int)manaAnimate, typeof(Texture2D));
			//Constantly sets the text for mana
			manaObj.transform.Find ("ManaText").GetComponent<TextMesh> ().text = currentMana + " / " + maxMana;
			manaObj.transform.GetComponent<MeshRenderer> ().material.mainTexture = manaTexture;
			manaAnimate += 0.05f;
			if (manaAnimate > 4.9) {
				manaAnimate = 1.0f;
			}
		}
	
	
		//When this method's called, Server has just sent the deck information
		//needed to instantiate cards, so we instantiate cards
		public void setDeck (DeckData deckData)
		{
			this.deckData = deckData; 
		
			//Takes the deck data and instatiates card GameObjects with AbstractCard scripts
			createDeck ();
		}

		void OnGUI ()
		{
			if (showTurn > 0 && isActive) { //Shows active player
				GUI.skin.box.fontStyle = FontStyle.Bold;
				GUI.skin.box.fontSize = 20;
				GUI.Box (new Rect ((Screen.width / 2.0f) - 100, (Screen.height / 2.0f) - 50, 250, 50), playerName + "'s Turn");
			}
			if (isGameOver) {
				if (GUI.Button (new Rect ((Screen.width / 2.0f) - ((Screen.width / 12.8f) / 100 * 150) / 2.0f, //left
			                       ((Screen.height * 2.0f) / 3.0f), //height
			                       (Screen.width / 12.8f) / 100 * 150, 
			                       (Screen.width / 12.8f) / 100 * 40), "Back to Lobby")) {
					//GameManager.protocols.sendReturnToLobby();
					Game.SwitchScene("World");
				}
			}
		}
	}
}