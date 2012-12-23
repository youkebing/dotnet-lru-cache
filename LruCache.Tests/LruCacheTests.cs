using System;
using System.Collections.Generic;
using NUnit.Framework;
using LruCache;

namespace LruCache.Tests
{
    [TestFixture]
    public class LruCacheTests
    {
        [Test]
        public void Constructor_MaxSizeAndItemSizeSpecified_MaxSizeAndItemSizeAreAsExpected()
        {
            var lru = new LruCache<string, string>(1024, 2);
            Assert.AreEqual(1024, lru.MaxSize);
            Assert.AreEqual(2, lru.ItemSize);
        }

        [Test]
        public void Constructor_OnlyMaxSizeSpecified_MaxSizeIsAsExpectedItemSizeIsOne()
        {
            var lru = new LruCache<string, string>(1024);
            Assert.AreEqual(1024, lru.MaxSize);
            Assert.AreEqual(1, lru.ItemSize);
        }

        [Test]
        public void Constructor_MaxSizeLessThanZero_ThrowsArgumentOutOfRangeException()
        {
            try
            {
                var lru = new LruCache<string, string>(-1);
                Assert.Fail("Should not have reached this point");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Constructor_MaxSizeIsZero_ThrowsArgumentOutOfRangeException()
        {
            try
            {
                var lru = new LruCache<string, string>(0);
                Assert.Fail("Should not have reached this point");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Constructor_ItemSizeLessThanZero_ThrowsArgumentOutOfRangeException()
        {
            try
            {
                var lru = new LruCache<string, string>(1024, -1);
                Assert.Fail("Should not have reached this point");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Constructor_MaxSizeLessThanItemSize_ThrowsArgumentOutOfRangeException()
        {
            try
            {
                var lru = new LruCache<string, string>(1, 2);
                Assert.Fail("Should not have reached this point");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Add_ItemIsAdded_ItemExistsInCache()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            Assert.IsTrue(lru.ContainsKey("k1"));
        }

        [Test]
        public void Add_SomeItemsAdded_FirstItemAddedIsOldest()
        {
            var lru = new LruCache<string, string>(1024);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");
            lru.Add("k3", "v3");
            lru.Add("k4", "v4");
            lru.Add("k5", "v5");

            Assert.AreEqual("v1", lru.OldestValue);
            Assert.AreEqual("k1", lru.OldestKey);
        }

        [Test]
        public void Add_SomeItemsAdded_LastItemAddedIsNewest()
        {
            var lru = new LruCache<string, string>(1024);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");
            lru.Add("k3", "v3");
            lru.Add("k4", "v4");
            lru.Add("k5", "v5");

            Assert.AreEqual("v5", lru.NewestValue);
            Assert.AreEqual("k5", lru.NewestKey);
        }

        [Test]
        public void Add_ItemIsAdded_CurrentSizeIncreasesByItemSize()
        {
            var lru = new LruCache<string, string>(1024, 2);
            Assert.AreEqual(0, lru.CurrentSize);
            lru.Add("k1", "v1");
            Assert.AreEqual(2, lru.CurrentSize);
            lru.Add("k2", "v2");
            Assert.AreEqual(4, lru.CurrentSize);
            lru.Add("k3", "v3");
            Assert.AreEqual(6, lru.CurrentSize);
            lru.Add("k4", "v4");
            Assert.AreEqual(8, lru.CurrentSize);
            lru.Add("k5", "v5");
        }

        [Test]
        public void Add_MaxSizeIsReached_OldestItemDiscarded()
        {
            var lru = new LruCache<string, string>(5);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");
            lru.Add("k3", "v3");
            lru.Add("k4", "v4");
            lru.Add("k5", "v5");
            lru.Add("k6", "v6");

            Assert.IsFalse(lru.ContainsKey("k1"));
        }

        [Test]
        public void Add_MaxSizeIsReached_DiscardedCountIncreases()
        {
            var lru = new LruCache<string, string>(5);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");
            lru.Add("k3", "v3");
            lru.Add("k4", "v4");
            lru.Add("k5", "v5");

            Assert.AreEqual(0, lru.DiscardedCount);

            lru.Add("k6", "v6");
            Assert.AreEqual(1, lru.DiscardedCount);

            lru.Add("k7", "v7");
            Assert.AreEqual(2, lru.DiscardedCount);
        }

        [Test]
        public void Add_DuplicateKeyAdded_ThrowsArgumentException()
        {
            try
            {
                var lru = new LruCache<string, string>(5);
                lru.Add("k1", "v1");
                lru.Add("k1", "v1");
                Assert.Fail("Should not have reached this point");
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Remove_ItemIsRemoved_ContainsKeyReturnsFalse()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            lru.Remove("k1");

            Assert.IsFalse(lru.ContainsKey("k1"));
        }

        [Test]
        public void Remove_ItemIsRemoved_ReturnsTrue()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            Assert.IsTrue(lru.Remove("k1"));
        }

        [Test]
        public void Remove_ItemIsRemoved_CurrentSizeDecreasesByItemSize()
        {
            var lru = new LruCache<string, string>(5);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");
            lru.Add("k3", "v3");
            lru.Add("k4", "v4");
            lru.Add("k5", "v5");

            Assert.AreEqual(5, lru.CurrentSize);

            lru.Remove("k1");
            Assert.AreEqual(4, lru.CurrentSize);

            lru.Remove("k2");
            Assert.AreEqual(3, lru.CurrentSize);

            lru.Remove("k3");
            Assert.AreEqual(2, lru.CurrentSize);

            lru.Remove("k4");
            Assert.AreEqual(1, lru.CurrentSize);

            lru.Remove("k5");
            Assert.AreEqual(0, lru.CurrentSize);
        }

        [Test]
        public void Remove_KeyDoesNotExist_ReturnsFalse()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            Assert.IsFalse(lru.Remove("k2"));
        }

        [Test]
        public void Remove_CacheIsEmpty_ReturnsFalse()
        {
            var lru = new LruCache<string, string>(1);
            Assert.IsFalse(lru.Remove("k1"));
        }

        [Test]
        public void Clear_CacheIsNotEmpty_CurrentSizeIsZero()
        {
            var lru = new LruCache<string, string>(5);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");
            lru.Add("k3", "v3");
            lru.Add("k4", "v4");
            lru.Add("k5", "v5");

            Assert.AreEqual(5, lru.CurrentSize);

            lru.Clear();

            Assert.AreEqual(0, lru.CurrentSize);
        }

        [Test]
        public void IndexerGet_KeyExists_ReturnsValue()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            var value = lru["k1"];

            Assert.AreEqual("v1", value);
        }

        [Test]
        public void IndexerGet_KeyExists_HitCountIncreased()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            Assert.AreEqual(0, lru.HitCount);

            var value = lru["k1"];

            Assert.AreEqual(1, lru.HitCount);
        }

        [Test]
        public void IndexerGet_KeyExists_MissCountNotIncreased()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            Assert.AreEqual(0, lru.MissCount);

            var value = lru["k1"];

            Assert.AreEqual(0, lru.MissCount);
        }

        [Test]
        public void IndexerGet_KeyDoesNotExist_ReturnsDefaultValue()
        {
            var lru = new LruCache<string, string>(1);
            var value = lru["k1"];
            Assert.IsNull(value);
        }

        [Test]
        public void IndexerGet_KeyDoesNotExist_HitCountNotIncreased()
        {
            var lru = new LruCache<string, string>(1);

            Assert.AreEqual(0, lru.HitCount);

            var value = lru["k1"];

            Assert.AreEqual(0, lru.HitCount);
        }

        [Test]
        public void IndexerGet_KeyDoesNotExist_MissCountIncreased()
        {
            var lru = new LruCache<string, string>(1);

            Assert.AreEqual(0, lru.MissCount);

            var value = lru["k1"];

            Assert.AreEqual(1, lru.MissCount);
        }

        [Test]
        public void IndexerSet_KeyExists_NewValueIsSet()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            Assert.AreEqual("v1", lru["k1"]);

            lru["k1"] = "NewValue";

            Assert.AreEqual("NewValue", lru["k1"]);
        }

        [Test]
        public void IndexerSet_KeyDoesNotExist_ThrowsKeyNotFoundException()
        {
            try
            {
                var lru = new LruCache<string, string>(1);
                lru["k1"] = "value";
                Assert.Fail("Should not have reached this point");
            }
            catch (KeyNotFoundException)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void TryGetValue_KeyExists_ValuePopulated()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            string value;
            lru.TryGetValue("k1", out value);

            Assert.AreEqual("v1", value);
        }

        [Test]
        public void TryGetValue_KeyExists_HitCountIncreased()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            Assert.AreEqual(0, lru.HitCount);

            string value;
            lru.TryGetValue("k1", out value);

            Assert.AreEqual(1, lru.HitCount);
        }

        [Test]
        public void TryGetValue_KeyDoesNotExist_DefaultValuePopulated()
        {
            var lru = new LruCache<string, string>(1);

            string value = null;
            lru.TryGetValue("k1", out value);

            Assert.IsNull(value);
        }

        [Test]
        public void TryGetValue_KeyDoesNotExist_MissCountIncreased()
        {
            var lru = new LruCache<string, string>(1);

            Assert.AreEqual(0, lru.MissCount);

            string value = null;
            lru.TryGetValue("k1", out value);

            Assert.AreEqual(1, lru.MissCount);
        }

        [Test]
        public void ContainsKey_KeyExists_ReturnsTrue()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            Assert.IsTrue(lru.ContainsKey("k1"));
        }

        [Test]
        public void ContainsKey_KeyDoestNotExist_ReturnsFalse()
        {
            var lru = new LruCache<string, string>(1);

            Assert.IsFalse(lru.ContainsKey("k1"));
        }

        [Test]
        public void Contains_EntryExists_ReturnsTrue()
        {
            var lru = new LruCache<string, string>(1);
            lru.Add("k1", "v1");

            var entry = new KeyValuePair<string, string>("k1", "v1");
            
            Assert.IsTrue(lru.Contains(entry));
        }

        [Test]
        public void Contains_EntryDoesNotExist_ReturnsFalse()
        {
            var lru = new LruCache<string, string>(1);

            var entry = new KeyValuePair<string, string>("k1", "v1");

            Assert.IsFalse(lru.Contains(entry));
        }

        [Test]
        public void OldestItemValue_CacheNotEmpty_ReturnsOldestValue()
        {
            var lru = new LruCache<string, string>(2);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");
            
            Assert.AreEqual("v1", lru.OldestValue);
        }

        [Test]
        public void OldestItemValue_CacheIsEmpty_ReturnsDefaultValue()
        {
            var lru = new LruCache<string, string>(2);

            Assert.AreEqual(default(string), lru.OldestValue);
        }

        [Test]
        public void OldestItemKey_CacheNotEmpty_ReturnsOldestKey()
        {
            var lru = new LruCache<string, string>(2);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");

            Assert.AreEqual("k1", lru.OldestKey);
        }

        [Test]
        public void OldestItemKey_CacheIsEmpty_ReturnsDefaultValue()
        {
            var lru = new LruCache<string, string>(2);

            Assert.AreEqual(default(string), lru.OldestKey);
        }

        [Test]
        public void NewestItemValue_CacheNotEmpty_ReturnsNewestValue()
        {
            var lru = new LruCache<string, string>(2);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");

            Assert.AreEqual("v2", lru.NewestValue);
        }

        [Test]
        public void NewestItemValue_CacheIsEmpty_ReturnsDefaultValue()
        {
            var lru = new LruCache<string, string>(2);

            Assert.AreEqual(default(string), lru.NewestValue);
        }

        [Test]
        public void NewestItemKey_CacheNotEmpty_ReturnsNewestKey()
        {
            var lru = new LruCache<string, string>(2);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");

            Assert.AreEqual("k2", lru.NewestKey);
        }

        [Test]
        public void NewestItemKey_CacheIsEmpty_ReturnsDefaultValue()
        {
            var lru = new LruCache<string, string>(2);

            Assert.AreEqual(default(string), lru.NewestKey);
        }

        [Test]
        public void ToString_Test()
        {
            var lru = new LruCache<string, string>(1024);

            var expected = "LruCache<String, String> Max Size: 1024 Current Size: 0 Item Size 1 Hits: 0 Misses: 0 Discarded: 0";

            Assert.AreEqual(expected, lru.ToString());
        }

        [Test]
        public void Resize_NewSizeLessThanZero_ThrowsArgumentOutOfRangeException()
        {
            var lru = new LruCache<string, string>(1024);
            
            try
            {
                lru.Resize(-1);
                Assert.Fail("Should not have reached this point.");
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Resize_NewSizeIsZero_ThrowsArgumentOutOfRangeException()
        {
            var lru = new LruCache<string, string>(1024);
            
            try
            {
                lru.Resize(0);
                Assert.Fail("Should not have reached this point.");
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Resize_NewSizeLessThanItemSize_ThrowsArgumentOutOfRangeException()
        {
            var lru = new LruCache<string, string>(1024, 4);
            
            try
            {
                lru.Resize(1);
                Assert.Fail("Should not have reached this point.");
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Resize_ValidNewSize_CacheResized()
        {
            var lru = new LruCache<string, string>(1024);
            Assert.AreEqual(1024, lru.MaxSize);
            
            lru.Resize(1024 * 1024);
            Assert.AreEqual(1024 * 1024, lru.MaxSize);
        }

        [Test]
        public void Resize_NewSizeCausesTrim_OnlyOldestItemsDiscarded()
        {
            var lru = new LruCache<string, string>(5);
            lru.Add("k1", "v1");
            lru.Add("k2", "v2");
            lru.Add("k3", "v3");
            lru.Add("k4", "v4");
            lru.Add("k5", "v5");

            lru.Resize(3);

            Assert.IsFalse(lru.ContainsKey("k1"));
            Assert.IsFalse(lru.ContainsKey("k2"));
            Assert.IsTrue(lru.ContainsKey("k3"));
            Assert.IsTrue(lru.ContainsKey("k4"));
            Assert.IsTrue(lru.ContainsKey("k5"));
        }
    }
}