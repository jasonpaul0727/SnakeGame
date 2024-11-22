///
/// Daniel Coimbra Salomão u1397516
/// Yanxia Bu u1343113
///
/// Instructor s: Daniel Kopta and Travis Martin
///
/// Assignment: PS9 - Snake Server
///
/// 12/8/2022

using System.Data.SqlTypes;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using System;
using NetworkUtil;
using System.Text.RegularExpressions;
using SnakeGame;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;
using System.Security.Cryptography;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json.Serialization;
using System.Security.Cryptography.X509Certificates;
///  this class is the server class which is read the xml file and connect to the server and user 
///  can use the command to control the snake run, eat power , respawn and collision
namespace Server
{
    public class server
    {
        //dictionary containing the clients, world model, gameSettings and a dictionary for clients update flag
        private Dictionary<long, SocketState> clients;
        private static World w;
        private static GameSettings gameSettings = new();
        Dictionary<int, bool> clientReadyToUpdate = new();
        private static int powerupCount = 0;
        public server()
        {
            //start a new dictionary and world
            clients = new Dictionary<long, SocketState>();
            w = new World();

        }
        /// <summary>
        /// Xmlread class, used to read the game settings xml file and set the world settings based on its content
        /// </summary>
        /// <param name="arg"></param>
        public void Xmlread(string[] arg)
        {
            int i = 0;
            String URLString = "C:\\Users\\dcsal\\source\\repos\\game-codequeen_game\\PS8Skeleton\\Server\\settings.xml";
            XmlReader reader = XmlReader.Create(URLString);
            DataContractSerializer ser = new DataContractSerializer(typeof(GameSettings));
            gameSettings = (GameSettings)ser.ReadObject(reader)!;
            //get the walls in a for each loop
            foreach (var b in gameSettings.Walls!)
            {
                w.walls[i] = b;
                i++;
            }
            w.worldSize = gameSettings.UniverseSize;
            reader.Close();
            Console.WriteLine("conectar com sucesso");
        }
        /// <summary>
        /// Class main. it will start server, read the xml and call update with a stopwatch on msperframe.
        /// </summary>
        /// <param name="args"></param>
        static void Main(String[] args)
        {
            server server = new server();
            server.Xmlread(args);
            // seperate
            Networking.StartServer(server.NewClientConnected, 11000);
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            //loop for updating method. Use of watch to update according to msperframe value
            while (true)
            {
                // wait until the next frame
                while (watch.ElapsedMilliseconds < gameSettings.MSPerFrame)
                { /* empty loop body */ }

                watch.Restart();
                lock (w)
                {
                    server.Update();
                }

            }
        }
        /// <summary>
        /// Update method
        /// this method will deal with sending the appropriate json file to each client.
        /// Including, movement, death, powerup, and other important interations between snakes with world
        /// </summary>
        private void Update()
        {
            String ss = "";
            foreach (Snake sn in w.snakes.Values)
            {
                //all movement related updates will only happen if snake is alive
                if (sn.alive)
                {
                    //movement: check for snake direction, then check for the boolean flag directionChanged in the snake class
                    //if direction has changed, then a new body part will be added, otherwise, the head will change position
                    if (sn.dir.y == 1)
                    {
                        if (sn.directionChanged)
                        {
                            sn.body.Add(new Vector2D(sn.body[sn.body.Count - 1].x, sn.body[sn.body.Count - 1].y + 3));
                            sn.directionChanged = false;
                        }
                        sn.body[sn.body.Count - 1].y += 3;
                    }
                    else if (sn.dir.y == -1)
                    {
                        if (sn.directionChanged)
                        {
                            sn.body.Add(new Vector2D(sn.body[sn.body.Count - 1].x, sn.body[sn.body.Count - 1].y - 3));
                            sn.directionChanged = false;
                        }
                        sn.body[sn.body.Count - 1].y -= 3;
                    }

                    else if (sn.dir.x == 1)
                    {
                        if (sn.directionChanged)
                        {
                            sn.body.Add(new Vector2D(sn.body[sn.body.Count - 1].x + 3, sn.body[sn.body.Count - 1].y));
                            sn.directionChanged = false;
                        }
                        sn.body[sn.body.Count - 1].x += 3;
                    }

                    else if (sn.dir.x == -1)
                    {
                        if (sn.directionChanged)
                        {
                            sn.body.Add(new Vector2D(sn.body[sn.body.Count - 1].x - 3, sn.body[sn.body.Count - 1].y));
                            sn.directionChanged = false;
                        }
                        sn.body[sn.body.Count - 1].x -= 3;
                    }

                    //check if snake ate powerup with the proper method
                    if (atePowerup(sn.body))
                    {
                        //update the snake score value
                        sn.score += 1;
                        //if eaten a powerup, the snake tail will increase
                        if (sn.body[0].x == sn.body[1].x)
                        {
                            if (sn.body[0].y > sn.body[1].y)
                                sn.body[0].y += 12;
                            else
                                sn.body[0].y -= 12;
                        }
                        else if (sn.body[0].y == sn.body[1].y)
                        {
                            if (sn.body[0].x > sn.body[1].x)
                                sn.body[0].x += 12;
                            else
                                sn.body[0].x -= 12;

                        }
                    }
                    //if snake is going over the map size, add a new bodypart on the opposite side
                    if (sn.body[sn.body.Count - 1].x > w.worldSize / 2)
                    {
                        sn.body.Add(new((-w.worldSize / 2), sn.body[sn.body.Count - 1].y));
                        sn.body.Add(new((-w.worldSize / 2), sn.body[sn.body.Count - 1].y));

                    }
                    else if (sn.body[sn.body.Count - 1].x < -w.worldSize / 2)
                    {
                        sn.body.Add(new(w.worldSize / 2, sn.body[sn.body.Count - 1].y));
                        sn.body.Add(new((w.worldSize / 2), sn.body[sn.body.Count - 1].y));
                    }
                    else if (sn.body[sn.body.Count - 1].y > w.worldSize / 2)
                    {
                        sn.body.Add(new(sn.body[sn.body.Count - 1].x, -w.worldSize / 2));
                        sn.body.Add(new(sn.body[sn.body.Count - 1].x, -w.worldSize / 2));
                    }
                    else if (sn.body[sn.body.Count - 1].y < -w.worldSize / 2)
                    {
                        sn.body.Add(new(sn.body[sn.body.Count - 1].x, w.worldSize / 2));
                        sn.body.Add(new(sn.body[sn.body.Count - 1].x, w.worldSize / 2));
                    }

                    //when the snake is deleted from one side, there is a small margin that should be considered
                    double dif;
                    //delete end of snake after movement increased head
                    if (sn.body[0].x == sn.body[1].x)
                    {
                        if (sn.body[0].y > sn.body[1].y)
                            sn.body[0].y -= 3;
                        else
                            sn.body[0].y += 3;
                        if (Math.Abs(sn.body[0].y - sn.body[1].y) <= 3)
                            sn.body.Remove(sn.body[0]);

                    }

                    else if (sn.body[0].y == sn.body[1].y)
                    {
                        if (sn.body[0].x > sn.body[1].x)
                            sn.body[0].x -= 3;
                        else
                            sn.body[0].x += 3;
                        if (Math.Abs(sn.body[0].x - sn.body[1].x) <= 3)
                            sn.body.Remove(sn.body[0]);
                    }

                    if (sn.body[0].x > w.worldSize / 2)
                    {
                        dif = Math.Abs(sn.body[0].x - sn.body[1].x) - 2000;
                        sn.body.Remove(sn.body[0]);
                        sn.body[0].x -= dif;
                    }
                    else if (sn.body[0].x < -w.worldSize / 2)
                    {
                        dif = Math.Abs(sn.body[0].x - sn.body[1].x) - 2000;
                        sn.body.Remove(sn.body[0]);
                        sn.body[0].x += dif;
                    }
                    else if (sn.body[0].y > w.worldSize / 2)
                    {
                        dif = Math.Abs(sn.body[0].y - sn.body[1].y) - 2000;
                        sn.body.Remove(sn.body[0]);
                        sn.body[0].y -= dif;
                    }
                    else if (sn.body[0].y < -w.worldSize / 2)
                    {
                        dif = Math.Abs(sn.body[0].y - sn.body[1].y) - 2000;
                        sn.body.Remove(sn.body[0]);
                        sn.body[0].y += dif;
                    }

                }

                sn.died = false;
                //check for colision and change flags
                if (sn.alive && collisionWall(sn.body) || sn.alive && collisionSnake(sn))
                {
                    sn.died = true;
                    sn.alive = false;
                }
                ss += JsonConvert.SerializeObject(sn) + "\n";
                //remove skane if flag of disconnect changed
                if (sn.dc)
                {
                    w.snakes.Remove(sn.snake);
                }
                //if dead, timer for alive will run and check with respawnrate
                if (!sn.alive)
                {
                    sn.deathTimer++;
                    if (sn.deathTimer == gameSettings.RespawnRate)
                        snakeRespawn(sn);
                }
            }
            //powerup spawner method
            powerupSpawner();
            //make json file
            foreach (var p in w.powerups.Values)
            {
                ss += JsonConvert.SerializeObject(p) + "\n";
            }
            //send for each client
            lock (clients)
            {
                foreach (SocketState state in clients.Values)
                {
                    if(clientReadyToUpdate.ContainsKey((int)state.ID))
                        if (clientReadyToUpdate[(int)state.ID])
                            Networking.Send(state.TheSocket, ss);
                }
            }


        }
        /// <summary>
        /// this method is  trigger when snake respawn
        /// </summary>
        /// <param name="sn"></param>
        private void snakeRespawn(Snake sn)
        {
            sn.body = generateSnake();
            sn.dir = new Vector2D(0, -1);
            sn.score = 0;
            sn.alive = true;
            sn.died = false;
            sn.deathTimer = 0;
        }
        //pwerup spawner
        /// <summary>
        /// spawn powerup, use a random location on the map where the intersector with walls will be false
        /// </summary>
        private void powerupSpawner()
        {
            if (powerupCount < 21)
            {
                Random rd = new Random();
                int valueX = (rd.Next(-w.worldSize / 2, w.worldSize / 2));
                int valueY = (rd.Next(-w.worldSize / 2, w.worldSize / 2));

                RectangleF pwp = new RectangleF((float)valueX - 15, (float)valueY - 15, (float)30, (float)30);
                //pwerup check with collision with wall
                //using the retangle intersector
                foreach (var c in w.walls)
                {
                    double UpperLeftXWall = Math.Min(c.Value.p1.x, c.Value.p2.x);
                    double UpperLeftYWall = Math.Min(c.Value.p1.y, c.Value.p2.y);
                    RectangleF wall = new RectangleF((float)UpperLeftXWall - 50, (float)UpperLeftYWall - 50, (float)Math.Abs(c.Value.p1.x - c.Value.p2.x) + 50, (float)Math.Abs(c.Value.p1.y - c.Value.p2.y) + 50);
                    // if power up intersect with wall 
                    if (pwp.IntersectsWith(wall))
                    {
                        powerupSpawner();
                        
                    }
                }
                Power pw = new(powerupCount, new Vector2D(valueX, valueY), false);
                w.powerups.Add(w.powerups.Count, pw);
                powerupCount += 1;
            }

        }
        /// <summary>
        /// new client is being connected
        /// </summary>
        /// <param name="state"></param>
        private void NewClientConnected(SocketState state)
        {

            if (state.ErrorOccurred)
                return;

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (clients)
            {
                clients[state.ID] = state;
            }
            clientReadyToUpdate[(int)state.ID] = false;
            // change the state's network action to the
            // receive handler so we can process data when something
            // happens on the network
            lock (clients)
            {
                state.OnNetworkAction = ReceiveMessageonfirst;
                Console.WriteLine("waiting for clients");
                Networking.GetData(state);
            }
        }
        
        /// <summary>
        /// check for snake head and powerup position
        /// </summary>
        /// <param name="snakeParts"></param>
        /// <returns></returns>
        private bool atePowerup(List<Vector2D> snakeParts)
        {
            RectangleF snakeP = new RectangleF((float)snakeParts[snakeParts.Count - 1].x - 5, (float)snakeParts[snakeParts.Count - 1].y - 5, (float)10, (float)10);

            foreach (var p in w.powerups)
            {
                RectangleF pwp = new RectangleF((float)p.Value.loc.GetX() - 5, (float)p.Value.loc.GetY() - 5, (float)10, (float)10);
                if (snakeP.IntersectsWith(pwp))
                {
                    if (!p.Value.died)
                    {
                        //if power is alive, the flag will turn to died and the powerup count will decrease
                        p.Value.died = true;
                        powerupCount--;
                        return true;
                    }
                }
            }
            return false;
        }
        //check for a position in the map and check for collision with wall
        private List<Vector2D> generateSnake()
        {
            Random rd = new Random();
            List<Vector2D> snakePart = new List<Vector2D>();
            int valueXHead = (rd.Next((-w.worldSize / 2) + 150, (w.worldSize / 2) - 150));
            int valueYHead = (rd.Next((-w.worldSize / 2) + 150, (w.worldSize / 2)) - 150);
            int valueXBody = valueXHead;
            int valueYBody = valueYHead + 120;

            snakePart.Add(new Vector2D(valueXBody, valueYBody));
            snakePart.Add(new Vector2D(valueXHead, valueYHead));
            //if there is collision with wall, call method again
            if (spawnCollision(snakePart))
            {
                snakePart = generateSnake();
            }

            return snakePart;
        }

        /// <summary>
        /// check the snake body with the retangle type and check for each wall with the rectangle type, then use the method of intercept
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private bool spawnCollision(List<Vector2D> part)
        {
            Vector2D part1 = part[0];
            Vector2D part2 = part[1];
            double UpperLeftX = Math.Min(part1.x, part2.x);
            double UpperLeftY = Math.Min(part1.y, part2.y);
            //make a rectangle with the snake body
            RectangleF snakePt = new RectangleF((float)UpperLeftX, (float)UpperLeftY, (float)Math.Abs(part1.x - part2.x) + 10, (float)Math.Abs(part1.y - part2.y) + 10);
            //make a rectangle for each wall and check for intersection
            foreach (var c in w.walls)
            {
                double UpperLeftXWall = Math.Min(c.Value.p1.x, c.Value.p2.x);
                double UpperLeftYWall = Math.Min(c.Value.p1.y, c.Value.p2.y);
                RectangleF wall = new RectangleF((float)UpperLeftXWall - 25, (float)UpperLeftYWall - 25, (float)Math.Abs(c.Value.p1.x - c.Value.p2.x) + 50, (float)Math.Abs(c.Value.p1.y - c.Value.p2.y) + 50);

                if (snakePt.IntersectsWith(wall))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// collision snake method, check for all snakes and create a rectangle for their body
        /// create a rectangle for the head, so that the head will return true when intersection with the body
        /// </summary>
        /// <param name="snake"></param>
        /// <returns></returns>
        private bool collisionSnake(Snake snake)
        {
            List<Vector2D> snakeParts = snake.body;
            Vector2D snakeHeadVector = snakeParts[snakeParts.Count - 1];
            RectangleF snakeHeadRectangle = new RectangleF((float)snakeParts[snakeParts.Count - 1].x - 5, (float)snakeParts[snakeParts.Count - 1].y - 5, (float)10, (float)10);

            foreach (var snakes in w.snakes.Values)
            {
                //using the 2 body coordinates for the collision detection
                double X1 = snakes.body[0].x;
                double Y1 = snakes.body[0].y;
                for (int part = 1; part < snakes.body.Count - 1; part++)
                {
                    double X2 = snakes.body[part].x;
                    double Y2 = snakes.body[part].y;
                    // create snake body rectangle
                    RectangleF rec = new((float)Math.Min(X1, X2), (float)Math.Min(Y1, Y2), (float)Math.Abs(X1 - X2), (float)Math.Abs(Y1 - Y2));
                    if (snakes.body[part] != snakeHeadVector && rec.Width < w.worldSize && rec.Height < w.worldSize)
                    {
                        if (snakeHeadRectangle.IntersectsWith(rec))
                        {
                            return true;
                        }

                    }
                    X1 = X2;
                    Y1 = Y2;
                }

            }

            return false;
        }
        
        /// <summary>
        /// collision with a wall method
        /// this method will check for the collision between head and wall, by creating  rectangle type on the head and one for each wall in the game
        /// </summary>
        /// <param name="snakeParts"></param>
        /// <returns></returns>
        private bool collisionWall(List<Vector2D> snakeParts)
        {
            RectangleF snakeP = new RectangleF((float)snakeParts[snakeParts.Count - 1].x - 5, (float)snakeParts[snakeParts.Count - 1].y - 5, (float)10, (float)10);


            foreach (var c in w.walls)
            {
                double UpperLeftXWall = Math.Min(c.Value.p1.x, c.Value.p2.x);
                double UpperLeftYWall = Math.Min(c.Value.p1.y, c.Value.p2.y);
                RectangleF wall = new RectangleF((float)UpperLeftXWall - 25, (float)UpperLeftYWall - 25, (float)Math.Abs(c.Value.p1.x - c.Value.p2.x) + 50, (float)Math.Abs(c.Value.p1.y - c.Value.p2.y) + 50);
                //using the intersection method to check for collision between head and wall
                if (snakeP.IntersectsWith(wall))
                {
                    return true;
                }
            }


            return false;
        }
        /// <summary>
        /// method for receiving the first message
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessageonfirst(SocketState state)
        {

            //List<Vector2D> vector2Ds = new List<Vector2D>();
            //vector2Ds.Add(new Vector2D(100,100));
            //vector2Ds.Add(new Vector2D(100,220));
            String s = "";
            if (state.ErrorOccurred)
            {
                RemoveClient(state.ID);
                return;
            }

            ///happen once

            string name = state.GetData();
            name = name.Substring(0, name.Length - 1);
            //getting the name then making a new snake with the state id of teh current client and calling the generate snake method
            lock (state)
            {
                Snake snake = new((int)state.ID, name, generateSnake(), new Vector2D(0, -1), 0, false, true, false, false);
                w.snakes.Add(w.snakeCount, snake);
                Networking.Send(state.TheSocket, w.snakeCount + "\n" + w.worldSize.ToString() + "\n");
                w.snakeCount += 1;
            }
            ///
            //state.RemoveData(0,4096);
            //String s = JsonConvert.SerializeObject(w.walls);
            lock (state)
            {
                //for each wall serialize it and send the sate to the client
                foreach (var p in w.walls.Values)
                {
                    // serialize the walls
                    s += JsonConvert.SerializeObject(p) + "\n";
                    // loop them and send each wall which networking send is inside of the for loop
                }
                Networking.Send(state.TheSocket, s);

                Console.WriteLine("Name is " + name);
                //now be ready to receive message method and the client is ready to receive the updates from server
                state.OnNetworkAction = ReceiveMessage;
                Networking.GetData(state);
                clientReadyToUpdate[(int)state.ID] = true;
            }
        }
        /// <summary>
        /// this is try to receive meesage fron server 
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessage(SocketState state)
        {
            Console.WriteLine("message receiving");
            // Remove the client if they aren't still connected
            if (state.ErrorOccurred)
            {
                RemoveClient(state.ID);
                return;

            }



            ProcessMessage(state);
            // Continue the event loop that receives messages from this client
            Networking.GetData(state);
        }
        /// <summary>
        /// processing the messaage when server receive 
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessage(SocketState state)
        {
            string totalData = state.GetData();

            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.
            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens.
                if (p[p.Length - 1] != '\n')
                    break;
                //check for the key press, then updating the direction on the current user snake and the flag of direction change
                if (p.Substring(0, p.Length - 1).Contains("left") && w.snakes[(int)state.ID].dir.x != 1 && w.snakes[(int)state.ID].dir.x != -1)
                {
                    if (w.snakes[(int)state.ID].alive)
                    {
                        w.snakes[(int)state.ID].dir.x = -1;
                        w.snakes[(int)state.ID].dir.y = 0;
                        w.snakes[(int)state.ID].directionChanged = true;
                    }
                }
                //check for the key press, then updating the direction on the current user snake and the flag of direction change

                else if (p.Substring(0, p.Length - 1).Contains("right") && w.snakes[(int)state.ID].dir.x != 1 && w.snakes[(int)state.ID].dir.x != -1)
                {
                    if (w.snakes[(int)state.ID].alive)
                    {
                        w.snakes[(int)state.ID].dir.x = 1;
                        w.snakes[(int)state.ID].dir.y = 0;
                        w.snakes[(int)state.ID].directionChanged = true;
                    }

                }
                //check for the key press, then updating the direction on the current user snake and the flag of direction change

                else if (p.Substring(0, p.Length - 1).Contains("up") && w.snakes[(int)state.ID].dir.y != 1 && w.snakes[(int)state.ID].dir.y != -1)
                {
                    if (w.snakes[(int)state.ID].alive)
                    {
                        w.snakes[(int)state.ID].dir.x = 0;
                        w.snakes[(int)state.ID].dir.y = -1;
                        w.snakes[(int)state.ID].directionChanged = true;
                    }
                }
                else if (p.Substring(0, p.Length - 1).Contains("down") && w.snakes[(int)state.ID].dir.y != 1 && w.snakes[(int)state.ID].dir.y != -1)
                {
                    if (w.snakes[(int)state.ID].alive)
                    {
                        w.snakes[(int)state.ID].dir.x = 0;
                        w.snakes[(int)state.ID].dir.y = 1;
                        w.snakes[(int)state.ID].directionChanged = true;
                    }
                }

                Console.WriteLine("received message from client " + state.ID + ": \"" + p.Substring(0, p.Length - 1) + "\"");

                // Remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);

                // Broadcast the message to all clients
                // Lock here beccause we can't have new connections
                // adding while looping through the clients list.
                // We also need to remove any disconnected clients.
                HashSet<long> disconnectedClients = new HashSet<long>();

                foreach (long id in disconnectedClients)
                    RemoveClient(id);
            }
        }

        /// <summary>
        /// Removes a client from the clients dictionary
        /// </summary>
        /// <param name="id">The ID of the client</param>
        private void RemoveClient(long id)
        {
            Console.WriteLine("Client " + id + " disconnected");
                //when removing the client, making sure to change the snake flags of disconnected and alive to their correct state  
            lock (clients)
            {
                clients.Remove(id);
                w.snakes[(int)id].dc = true;
                w.snakes[(int)id].alive = false;

            }
        }

    }
}
