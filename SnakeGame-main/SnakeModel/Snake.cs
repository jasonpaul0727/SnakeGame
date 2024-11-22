///Snake Class for Model. 
///Containing the fields that make up the json sent by the Server
///
///Daniel Coimbra Salomão 
///Yanxia Bu
///CS3500 PS8

using Newtonsoft.Json;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Snake
    {
        [JsonProperty(PropertyName = "snake")]
        public int snake;
        [JsonProperty(PropertyName = "name")]
        public string name;
        [JsonProperty(PropertyName = "body")]
        public List<Vector2D> body;
        [JsonProperty(PropertyName = "dir")]
        public Vector2D dir;
        [JsonProperty(PropertyName = "score")]
        public int score;
        [JsonProperty(PropertyName = "died")]
        public bool died;
        [JsonProperty(PropertyName = "alive")]
        public bool alive;
        [JsonProperty(PropertyName = "dc")]
        public bool dc;
        [JsonProperty(PropertyName = "join")]
        public bool join;
        public bool directionChanged = false;
        public int deathTimer;
        public  Snake(int snake, string name, List<Vector2D> body, Vector2D dir, int score,bool died, bool alive, bool dc, bool join)
        {
            this.snake = snake;
            this.name = name;
            this.body = body;
            this.dir = dir;
            this.score = score;
            this.died = died;
            this.alive = alive;
            this.dc = dc;
            this.join = join;
        }
    }
}
