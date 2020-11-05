ABOUT THE PROJECT

Pirate Wars is a website application containing two parts: game and ranking. Game is commonly known under the name "Battleships", which purpose is to sink all opponent's ships. Application enables user to generate a board with randomly placed ships and then play against AI opponent. After the end of the game player may enter their nick to appear in the ranking, which consists of 8 best games listed by number of shots used to win.

---

GAME

Main aim is for the game to follow the rules of "Battleships". Those are:
 - ships may not adhere with their flanks or peaks,
 - if a ship built of multiple elements is hit the shot element is marked as "shot",
 - if all elements of a ship are hit the whole ship is marked as "drowned",
 - hitting a ship gives another opportunity to shoot,
 - player who sunk all opponent's ships first is declared a winner. 
 
First problem that appears is how to place the ships on a board. It concerns not only the first presented rule of "Battleships" but also that ship may not start at the end of the board and appear on the other side, resulting in it being "cut" into two smaller ships. To prevent ships from adhering after each created ship all tiles around it are being blocked. This deletes these tiles from the set of tiles available to being any part of a ship. Secondly, to prevent ships from being "cut", each tile selected to be a starting point for a ship is being checked if it is not too close to boarder given the number of elements and the drawn direction of expansion. If it is too close then a new starting point is being drawn.

Second problem is shooting and sinking ships, eventually leading to the end of the game. The aim is to prevent user from cheating by checking AI tiles marked as "ship" in a browser. It is resolved by sending a request to the server each time player or AI shoots. The server stores all information about user and AI boards and answers if there is a miss or hit, and if it is a hit whether it is a shot or drowned. By sending information only about one tile no information about the other tiles are disclosed to the user.

AI

It is not hard to win against AI which shoots randomly. The goal is to make the game more challenging by making smarter AI opponent. Two problems have to be solved to achieve this goal. First, after hitting a ship AI should shoot at tiles next to the previous tile as long as the ship is drowned. Second, shooting tiles next to or at the peaks of a drowned ship should be blocked, because according to the rules of the game no ships can adhere.

First problem is solved in two steps. Firstly, each time AI shoots a ship diagonal tiles to the shot one are being blocked. Secondly, before every AI action player tiles are being checked for shot tiles. If any shot tile is found, AI is obligated to shoot at adjacent tiles. After hitting next ship tile three of four possible tiles to shoot in the next step are already blocked, one being marked as "shot" and two because of earlier blockage of diagonal tiles. This results in AI moving in only one plane. 

Second problem is partly solved by first problem solution. Blocking diagonal tiles when hitting a ship lefts side tiles of the ship already protected from any attempts at shooting. But still there are two possible not blocked tiles at both ends of a ship. They are found during the action of looking for tiles marked as "shot". When such tile is found not only it's marking is changed for "drowned" but also checked for adjacent tiles being available for shooting. If such tile is found it is being blocked.

RANKING

Ranking of the game contains 8 best games ordered by the lowest number of shots. It includes player's nick, time and date of the game as well. Each started game is being sent to database, making it possible to continue playing after restarting a server and to update the ranking every time a finished game qualifies.

---

Pirate Wars is a game designed by the rules of "Battleships". It guarantees wide choice of player and AI boards options and competitive play against smart AI opponent. Best scores are being listed in game ranking rooted in solid database.
