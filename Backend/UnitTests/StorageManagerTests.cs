﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YouShallNotPassBackend.Cryptography;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;
using YouShallNotPassBackend.Storage;

namespace YouShallNotPassBackendTests
{
    [TestClass]
    public class StorageManagerTests
    {
        private readonly StorageManager storageManager;
        private readonly Storage storage;
        private static readonly Random random = new();

        public StorageManagerTests()
        {
            string entriesLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entries");
            Directory.CreateDirectory(entriesLocation);

            Crypto crypto = new(Convert.ToHexString(RandomNumberGenerator.GetBytes(128 / 8)));
            storage = new(entriesLocation);
            storageManager = new(storage, crypto, 1000);
        }

        [TestCleanup()]
        public void Cleanup()
        {
            storageManager.Clear();
        }

        [TestMethod]
        public void TestAddGetEntry()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public void TestAddGetEntryWithLongerLabel()
        {
            Content content = GetContent("password", "Password from Slack", DateTime.Now.AddMinutes(15), 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public void TestAddGetEntryWithEvenLongerLabel()
        {
            string label = RandomString(1024);
            Content content = GetContent("password", label, DateTime.Now.AddMinutes(15), 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public void TestMaxAccessCountOf1()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
            Assert.ThrowsException<EntryExpiredException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestMaxAccessCountOf3()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 3);
            ContentKey contentKey = storageManager.AddEntry(content);

            Content retreivedContent = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent);

            Content retreivedContent2 = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent2);

            Content retreivedContent3 = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent3);

            Assert.ThrowsException<EntryExpiredException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestExpirationDate()
        {
            Content content = GetContent(DateTime.Now.AddSeconds(2), 100);
            ContentKey contentKey = storageManager.AddEntry(content);

            for (int i = 0; i < 5; i++)
            {
                Content retreivedContent = storageManager.GetEntry(contentKey);
                Assert.AreEqual(content, retreivedContent);
            }

            Thread.Sleep(2000);
            Assert.ThrowsException<EntryExpiredException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestDelete()
        {
            Content content = GetContent(DateTime.Now.AddSeconds(2), 100);
            ContentKey contentKey = storageManager.AddEntry(content);

            Content retreivedContent = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent);

            bool success = storageManager.DeleteEntry(contentKey.Id);
            Assert.IsTrue(success);
            
            Assert.ThrowsException<EntryNotFoundException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestDeleteExpired()
        {
            Content content = GetContent(DateTime.Now.AddSeconds(1), 100);
            ContentKey contentKey = storageManager.AddEntry(content);

            Content retreivedContent = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent);

            Thread.Sleep(3000);

            Assert.IsFalse(storage.Contains(contentKey.Id));
        }

        private static Content GetContent(string data, string label, DateTime expirationDate, int maxAccessCount)
        {
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);

            return new()
            {
                ContentType = ContentType.TEXT,
                Label = label,
                ExpirationDate = expirationDate,
                MaxAccessCount = maxAccessCount,
                Data = dataBytes
            };
        }

        private static Content GetContent(DateTime expirationDate, int maxAccessCount)
        {
            return GetContent("password", "my password", expirationDate, maxAccessCount);
        }
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
