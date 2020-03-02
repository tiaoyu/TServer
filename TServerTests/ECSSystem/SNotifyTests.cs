using Xunit;
using TServer.ECSSystem;
using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSEntity;

namespace TServer.ECSSystem.Tests
{
    public class SNotifyTests
    {
        [Fact()]
        public void UpdateTest()
        {
            var role = new ERole { Id = 0 };
            SNotify.Instance.Register(ENotify.ENotify_RoleDead, (notify) =>
            {
                if (notify is NotifyRoleDead notifyRoleDead)
                {
                    notifyRoleDead.Role.Id = 5;
                }
            });
            NotifyRoleDead.Push(role);
            SNotify.Instance.Update();

            Assert.True(role.Id == 5);
        }
    }
}