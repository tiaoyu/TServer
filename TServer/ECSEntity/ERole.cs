using Common.Normal;
using Common.TTimer;
using TServer.ECSComponent;
using TServer.Net;

namespace TServer.ECSEntity
{
    public class ERole : EEntity
    {
        public ExtSocket exSocket;
        public long LastActiveTime;
        public int UpdateTimerId;
        public ERole() : base()
        {
            EntityType = EEntityType.ROLE;
            Position = new CPosition<double> { x = 1D, y = 1D, z = 0D };
            SightDistance = 150;
            CanBeSeeDistance = 150;
            AutoAttackDistance = 10;
            LastActiveTime = Program.stopwatch.ElapsedMilliseconds;
        }

        public override void Update()
        {
            UpdateTimerId = Program.TimerManager.Insert(0, 10000, int.MaxValue, null, (obj) =>
              {
                  if (Program.stopwatch.ElapsedMilliseconds - LastActiveTime >= 6000)
                  {
                      //GameServer.DicRole.Remove(exSocket.Guid);
                      Program.Server.TCloseSocket(exSocket.SocketEventArgs);
                      Program.TimerManager.AddToRemove(UpdateTimerId);
                  }
              });
        }
    }
}
