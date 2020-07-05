using Common.Normal;
using TServer.ECSComponent;

namespace TServer.ECSEntity
{
    public class ERole
    {
        public int Id;
        public ExtSocket exSocket;

        public CPosition<double> Position { get; set; }
        public CDungeon Dungeon { get; set; }
        public CSight Sight { get; set; }
        public CMovement Movement { get; set; }

        /// <summary> 
        /// 视野半径 格子为单位长度
        /// </summary>
        public int SightDistance { get; set; }

        public ERole()
        {
            Position = new CPosition<double> { x = 0D, y = 0D };
            Sight = new CSight();
            Movement = new CMovement();
            SightDistance = 20;
        }
    }
}
