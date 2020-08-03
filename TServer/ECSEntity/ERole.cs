using Common.Normal;
using TServer.ECSComponent;

namespace TServer.ECSEntity
{
    public class ERole : EEntity
    {
        public ExtSocket exSocket;

        public ERole() :base()
        {
            EntityType = EEntityType.ROLE;
            Position = new CPosition<double> { x = 0D, y = 0D, z = 0D };

        }
    }
}
