///Daniel Coimbra Salomão 
///Yanxia Bu
///CS3500 PS8
///Class for the World, belonging to the Model.
///The class contains fields like, snakeCount, id, worldSize, and 3 dictionaries,
///that correspond to the walls, powerups and snakes dictionaries.
namespace SnakeGame
{
    public class World
    {
        public int snakeCount { get; set; }
        public int id = 0;
        public int worldSize;
        public Dictionary<int,Wall> walls;
        public Dictionary<int,Power> powerups;
        public Dictionary<int,Snake> snakes;
        public bool loaded=false;
        
        public World()
        {
            snakes = new Dictionary<int,Snake>();
            powerups = new();
            walls = new Dictionary<int,Wall>();
            this.id= 0;
            this.worldSize = 2000;
        }

        /// <summary>
        /// This method is used to update the walls, since the walls dictionary will be complete only once, we decided
        /// to use a method and call it once after the server sends all wall data
        /// </summary>
        /// <param name="wall"></param>
        public void updateWalls(Dictionary<int,Wall> wall)
        {
            this.walls = wall;
        }

    }
}