using GrimSearch.Utils.CharacterFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace GrimSearch.Tests.FileUtils
{
    [TestClass]
    public class CharacterFileTests
    {
        [TestMethod]
        public void TestReadCharacter_1_3_0()
        {
            var character = new CharacterFile();

            using (var stream = File.OpenRead("Resources/Saves/1.3.0/main/_Acidopholus/player.gdc"))
            {
                character.Read(stream);
            }

            Assert.AreEqual("Acidopholus", character.Header.Name);
            Assert.AreEqual(70, character.Inventory.Sacks.Sum(x => x.Items.Count));
            Assert.AreEqual(9, character.Stash.stashPages.Sum(x => x.items.Count));
        }

        [TestMethod]
        public void TestDeadHardcoreCharacterState()
        {
            var character = new CharacterFile();
            character.Header.Hardcore = 1;

            Assert.IsTrue(character.IsHardcore);
            Assert.IsFalse(character.IsDeadHardcore);

            character.Stats.deaths = 1;
            Assert.IsTrue(character.IsDeadHardcore);
        }

        [TestMethod]
        public void TestTransferStashCharacterMode()
        {
            var normalStash = new TransferStashFile().ToCharacterFile();
            var hardcoreStash = new TransferStashFile().ToCharacterFile("Hardcore transfer stash", true);

            Assert.IsFalse(normalStash.IsHardcore);
            Assert.AreEqual("Transfer stash", normalStash.Header.Name);
            Assert.IsTrue(hardcoreStash.IsHardcore);
            Assert.AreEqual("Hardcore transfer stash", hardcoreStash.Header.Name);
        }
    }
}
