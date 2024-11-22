using System;
using SnakeGame;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// this class is done by yanxia and Daniel. this class is set up the data member that read in xml 
namespace Server
{
    // it has 5 object firsrt one is framepershot , msperframe which is frame speed and respawn waiting time and the size of  wall 
    [DataContract(Name = "GameSettings", Namespace ="")]
    public class GameSettings
    {
        [DataMember]
        public int FramesPerShot { get; set; }
        [DataMember]
        public int MSPerFrame { get; set; }
        [DataMember]
        public int RespawnRate { get; set; }
        [DataMember]
        public int UniverseSize { get; set; }
        [DataMember]
        public List<Wall>? Walls { get; set; }

    }
}
