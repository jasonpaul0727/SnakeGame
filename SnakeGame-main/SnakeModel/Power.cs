///Daniel Coimbra Salomão 
///Yanxia Bu
///CS3500 PS8
///
///Class for the powerup, belonging to the model. This class will contain the fields that the json sent by the server will hold
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Power
    {
        [JsonProperty(PropertyName = "power")]
        public int power;

        [JsonProperty(PropertyName = "loc")]
        public Vector2D loc;

        [JsonProperty(PropertyName = "died")]
        public bool died;

        public Power(int power, Vector2D loc, bool died)
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
        }
        public void CurrentPower(int power, Vector2D loc, bool died)
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
        }
    }
}
