///Daniel Coimbra Salomão 
///Yanxia Bu
///CS3500 PS8
///
///Class for the Walls, belonging to the Model. This class contains the fields that are
///corresponding to the json received by the server. 
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SnakeGame;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace SnakeGame
{
    [DataContract(Name = "Wall", Namespace = "")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [DataMember(Name = "ID") ]
        [JsonProperty(PropertyName = "wall")]
        public int wall;
        [DataMember]
        [JsonProperty(PropertyName = "p1")]
        public Vector2D p1;
        [DataMember]
        [JsonProperty(PropertyName = "p2")]
        public Vector2D p2;
        /// <summary>
        /// Constructor for the currentWall
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void CurrentWall(int wall, Vector2D p1, Vector2D p2)
        {
            this.wall = wall;
            this.p1 = p1;
            this.p2 = p2;
        }

        
    }
}
