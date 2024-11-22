///Daniel Coimbra Salomão 
///Yanxia Bu
///CS3500 PS8
///
///Controller Class. This class will be responsable for parsing the information from the network and updating the model. 
///Then, using delegates events, update the view on the current state of the game.
using NetworkUtil;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace SnakeGame
{
    public class Controller
    {
        //creating the event handlers for errors, connected and update
        public delegate void ErrorHandler(string err);
        public event ErrorHandler? Error;

        public delegate void ConnectedHandler();
        public event ConnectedHandler? Connected;

        public delegate void GameUpdateHandler();
        public event GameUpdateHandler? UpdateArrived;

        //create the socket
        SocketState? theServer = null;

        private int id;
        private int worldSize;
        private bool first = true;
        private World world;
        private string name;
        /// <summary>
        /// Controller constructor
        /// </summary>
        public Controller()
        {
            name = "";
            id = 0;
            worldSize = 0;
            world = new();
        }
        /// <summary>
        /// connect method like in MVC ChatSystem model, using the name from the view and connectToServer to initialize the connection process
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="name"></param>
        public void Connect(string addr,string name)
        {
            this.name = name;

            Networking.ConnectToServer(OnConnect, addr, 11000);
        }
        /// <summary>
        /// getWord method that will return the current world, useful for the panel drawing method
        /// </summary>
        /// <returns></returns>
        public World GetWorld()
        {
            return world;
        }
        /// <summary>
        /// OnConnect method, will inform the view using the event handlers about the connection state, and will send the data of the name to server,
        /// also, will start the get data process from server using ReceiveMessage
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                // inform the view
                Error?.Invoke("Error connecting to server");
                return;
            }

            // inform the view
            Connected?.Invoke();

            theServer = state;

            // Start an event loop to receive messages from the server
            lock (state)
            {
                state.OnNetworkAction = ReceiveMessage;
            Networking.Send(state.TheSocket, name);
            
                Networking.GetData(state);
            }
        }

        /// <summary>
        /// Method for receiving data from server. This method will use processMessages and call getData on the networking
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessage(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                // inform the view
                Error?.Invoke("Lost connection to server");
                return;
            }
            ProcessMessages(state);

            // Continue the event loop
            // state.OnNetworkAction has not been changed, 
            // so this same method (ReceiveMessage) 
            // will be invoked when more data arrives
            Networking.GetData(state);
        }

        /// <summary>
        /// Method that will process the messages from the server and update the current world with the information json received
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state)
        {

            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            //check if the parts is the first string to be sent by the server, containing the id and the worldSize
            //section to check for first messages, that will contain the id, worldSize
            if (first)
            {
                id = int.Parse(parts[0]);
                worldSize = int.Parse(parts[1]);
                first = false;
                //world class constructor
                world.id = id;
                world.worldSize = worldSize;
            }


            // Loop until we have processed all messages.
            // We may have received more than one.
            List<string> newMessages = new List<string>();
            Dictionary<int, Wall> listOfWalls = new();
            //locking the foreach loop that receive the parts data from server, avoiding race conditions
            lock (world)
            {
                foreach (string p in parts)
                {
                    // Ignore empty strings added by the regex splitter
                    if (p.Length == 0 || p is null)
                        continue;
                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (p[p.Length - 1] != '\n')
                        break;
                    //String q = p.Trim('\n');
                    //JObject obj = JObject.Parse(q);

                    //check for walls, deserialize object and add it to the list of walls
                    if (p.Contains("wall"))
                    {
                        Wall wall = JsonConvert.DeserializeObject<Wall>(p)!;
                        listOfWalls.Add(wall.wall, wall);
                    }
                    // check for snake, then will check for the dc flag for removal of dictionary. Otherwise, it will add it accordingly
                    else if (p.Contains("snake"))
                    {
                        Snake snake = JsonConvert.DeserializeObject<Snake>(p)!;
                        if (snake.dc)
                        {
                            world.snakes.Remove(snake.snake);
                        }
                        else if (world.snakes.ContainsKey(snake.snake))
                        {
                            world.snakes[snake.snake] = snake;
                        }
                        else
                            world.snakes.Add(snake.snake, snake);
                    }
                    //check for power. Ill check if it is contained in dictionary, otherwise, will add it.
                    else if (p.Contains("power"))
                    {
                        Power power = JsonConvert.DeserializeObject<Power>(p)!;
                        if (world.powerups.ContainsKey(power.power))
                        {

                            world.powerups[power.power] = power;
                        }
                        else
                            world.powerups.Add(power.power, power);
                    }


                    // Display the message
                    // "messages" is the big message text box in the form.
                    // We must use a Dispatcher, because only the thread 
                    // that created the GUI can modify it.

                    newMessages.Add(p);

                    // Then remove it from the SocketState's growable buffer
                    // change p.length to 4096
                    state.RemoveData(0,p.Length);

                    //MessagesArrived?.Invoke(newMessages);

                }
                //if walls were added, update the world
                if (listOfWalls.Count>0)
                {
                    world.updateWalls(listOfWalls);
                }
                //world.updateSnakes(listOfSnakes);
                if (world.snakes.Count != 0 && world.snakes.ContainsKey(id))
                {
                    world.loaded = true;
                }
            }
            //update arrive delegate should be called to inform the view of a new server state, and draw it
            UpdateArrived?.Invoke();

        }
        public void Close()
        {
            theServer?.TheSocket.Close();
        }

        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="message"></param>
        public void MessageEntered(string message)
        {
            if (theServer is not null)
                Networking.Send(theServer.TheSocket, message + "\n");
        }
    }
}