using Common.Normal;
using TServer.ECSComponent;

namespace TServer.ECSEntity
{
    public class ERole
    {
        public int Id;
        public ExtSocket exSocket;
        public CPosition<double> Position;
        public EDungeon Dungeon;
        public ERole()
        {
            Position = new CPosition<double> { x = 0D, y = 0D };
        }
    }
}
