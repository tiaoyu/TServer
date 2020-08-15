using Common.Normal;
using TServer.ECSComponent;

namespace TServer.ECSEntity
{
    public class ERole : EEntity
    {
        public ExtSocket exSocket;

        public ERole() : base()
        {
            EntityType = EEntityType.ROLE;
            Position = new CPosition<double> { x = 1D, y = 1D, z = 0D };
            SightDistance = 150;
            CanBeSeeDistance = 150;
            AutoAttackDistance = 10;
        }
    }
}
