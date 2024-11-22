Author 
 Daniel Coimbra u1397516
 Yanxia Bu u1343113
 
date 12/08//2022
 
Team: codequeen-game
 
Instructed by Travis Martin, Daniel Kopta and TAs. (Thank you Every TA for helping us)
 
Additional feature:  No addition feature in this assignment 

special instruction:When the player opens the server, a snake will appear randomly after he opens the snake client and he will automatically run when the player 
needs to operate ("W" "A" "S" "D") to change direction. The snake will respawn when it hits a wall or crashes into itself. When the snake respawns, it will repeat 
the previous steps. The snake will add score and length when it eats the powerup when the snake wraps around

Design decision：We decided to write sprint 1 first and after reading XmL our program never ran. After two days of debugging we found that our wall ID did not change. On Wednesday we started writing three hits. The snake hits the wall, the snake hits itself, and the snake hits the powerup. We also let the other Al snakes join the game on the same day and dealt with their role in the game. On Thursday we found the connection bug and fixed it, then wrote the extra feature and the rest of the features that should be done.we face  many bug when we write this HW, such as when we drawing the Wall the ID we forgot to change, and we add many not useful code in MVCchatSystem and cause our program crash.  However, After multiple days working it fixed such bugs and made huge progressive. 

Extra help：we use some code the professor provides such as in- class materia, PS8 Skelton , chatsytem and MVCchatSystem and StackOverflow( only when we debug). We also ask TA and professor for help in debug and we fully appreciate Jim Morgan, Aston Loosie, Annabella Miler, Jo Elton , Zhongyi Jiang and Ethan….. We appreciate you guys providing huge help for us and assisting us overcome the trouble. 

Important tips: After we remove every border(top, bottom, left and right) our snake does not have a problem after crossing the bottom to top or right to left.However, the Ai client snake have the bug which is the snake will become long line after it cross from bottom to the top even it did not eat power up.

Some function Tips:
collision with AI might have problems due to collision rectangle formation. But very rarely.
the power up take long time to respawn  when snake run with AI 


 
 
date 11/21/2022
 
Team: codequeen-game
 
Instructed by Travis Martin, Daniel Kopta and TAs. (Thank you Every TA for helping us)
 

 
Additional feature: After completing the basic requirements, we added a lot of artistry to give the assignment a more unique and personalized feel, 
first by turning the food into sushi and adding a pair of eyes to the snake. Secondly, we made the snake dinamically explode when it died. 
We also have a score of +1 when the snake eats the sushi on top of the screen.


special instruction: If it is the wrong location in the server which is not ” localhost”, the error  message  will show to remind the player to re enter the correct server .  
the player should hit the “W” “ A” “S” “D” for up , left , down and right. When the snake eats the sushi(powerup) the length of the snake will be 
longer than before, and death will occur when the snake hits the wall or collides with another AI snake. The snake will be resurrected after each death 
with a little bit of a long wait. 

Design decision：First, we started to write the controller according to TA's suggestion. We started very well but we found 
that we were not successful in adding the wall. We then created a list to add the wall but we would draw the wall before the 
received data so that our game would be a direct throwexception. After studying the error we decided to set a boolean flag to 
determine if we received the message. After solving this problem we start to do the keyboard stuff which is “W”,”A”, “S”,”D”.
We use the messageenter method  provide by C# for getting the information when the user clicks each button. After that we do the 
reconnecting and showing errors when the server is wrong at the mainpage.xaml too.
In the model part we  created snakes, walls, powerups and worlds according to the requirements provided by our teacher. 
We didn't encounter any major problems at this stage. When we started writing the draw part, we didn't know how to start at first. 
After we discussed TA Jiang, we figured out the best way to start the drawing part. There we made some decisions, for uniqueness of our code. First, we
added the snake with unique colors up to 9 possibilities, and added eyes on the head. Second, we decided to add an image for the powerup, using a png file.
Finally, we used an explosion animation, for that we used a variable to add distace on every frame, and particles that will spread from the head position at
the time of death.

External Resources：we use some code the professor provides such as in- class materia, PS8 Skelton , chatsytem and MVC FullchatSystem and 
StackOverflow( only when we debug). We also downloaded the sushi image on google. 

Important tips: After we remove every border(top, bottom, left and right) our snake does not have a problem after crossing the 
bottom to top or right to left.However, the Ai client snake have the bug which is the snake will become long line after it cross 
from bottom to the top even it did not eat power up
