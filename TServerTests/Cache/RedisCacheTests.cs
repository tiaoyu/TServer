using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using Common.Protobuf;

namespace Common.Cache.Tests
{
    [TestClass()]
    public class RedisCacheTests
    {
        [TestMethod()]
        public void SetTest()
        {
            var str1 = "hello,world!";
            RedisCache.Instance.Set("a", str1);
            RedisCache.Instance.Set("aa", Encoding.UTF8.GetBytes(str1));
            var pack = new C2SLogin
            {
                Name = "tiaoyu",
                Password = "include"
            };
            RedisCache.Instance.Set("aaa", pack.Serialize());
            var str2 = RedisCache.Instance.Get<string>("a");
            Assert.IsTrue(str1.Equals(str2));
        }
    }
}