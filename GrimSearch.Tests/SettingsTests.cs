using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using GrimSearch.ViewModels;

namespace GrimSearch.Tests;

[TestClass]
public class SettingsTests
{
    [TestMethod]
    public void IncludeBlueprintsDefaultsToTrueForExistingSettings()
    {
        var settings = JsonConvert.DeserializeObject<StoredSettings>("{}");

        Assert.IsTrue(settings.IncludeBlueprints);
    }

    [TestMethod]
    public void IncludeBlueprintsPersistsFalse()
    {
        var json = JsonConvert.SerializeObject(new StoredSettings { IncludeBlueprints = false });
        var settings = JsonConvert.DeserializeObject<StoredSettings>(json);

        Assert.IsFalse(settings.IncludeBlueprints);
    }

    [TestMethod]
    public void SelectedItemQualitiesRoundTrip()
    {
        var json = JsonConvert.SerializeObject(new StoredSettings
        {
            SelectedItemQualities = new[] { "Legendary", "Epic" }
        });

        var settings = JsonConvert.DeserializeObject<StoredSettings>(json);

        CollectionAssert.AreEqual(new[] { "Legendary", "Epic" }, settings.SelectedItemQualities);
    }

    [TestMethod]
    public void EmptySelectedItemQualitiesRoundTrip()
    {
        var json = JsonConvert.SerializeObject(new StoredSettings
        {
            SelectedItemQualities = new string[0]
        });

        var settings = JsonConvert.DeserializeObject<StoredSettings>(json);

        Assert.IsNotNull(settings.SelectedItemQualities);
        Assert.AreEqual(0, settings.SelectedItemQualities.Length);
    }

    [TestMethod]
    public void SelectedItemQualitiesRemainNullForExistingSettings()
    {
        var settings = JsonConvert.DeserializeObject<StoredSettings>("{}");

        Assert.IsNull(settings.SelectedItemQualities);
    }

    [TestMethod]
    public void SelectedItemTypesRoundTrip()
    {
        var json = JsonConvert.SerializeObject(new StoredSettings
        {
            SelectedItemTypes = new[] { "ArmorJewelry_Ring", "WeaponMelee_Axe" }
        });

        var settings = JsonConvert.DeserializeObject<StoredSettings>(json);

        CollectionAssert.AreEqual(
            new[] { "ArmorJewelry_Ring", "WeaponMelee_Axe" },
            settings.SelectedItemTypes);
    }

    [TestMethod]
    public void EmptySelectedItemTypesRoundTrip()
    {
        var json = JsonConvert.SerializeObject(new StoredSettings
        {
            SelectedItemTypes = new string[0]
        });

        var settings = JsonConvert.DeserializeObject<StoredSettings>(json);

        Assert.IsNotNull(settings.SelectedItemTypes);
        Assert.AreEqual(0, settings.SelectedItemTypes.Length);
    }

    [TestMethod]
    public void SelectedItemTypesRemainNullForExistingSettings()
    {
        var settings = JsonConvert.DeserializeObject<StoredSettings>("{}");

        Assert.IsNull(settings.SelectedItemTypes);
    }

    [TestMethod]
    public void ItemLanguageDefaultsToEnglishForExistingSettings()
    {
        var settings = JsonConvert.DeserializeObject<StoredSettings>("{}");

        Assert.AreEqual("EN", settings.ItemLanguage);
    }

    [TestMethod]
    public void ItemLanguageRoundTrips()
    {
        var json = JsonConvert.SerializeObject(new StoredSettings { ItemLanguage = "JA" });
        var settings = JsonConvert.DeserializeObject<StoredSettings>(json);

        Assert.AreEqual("JA", settings.ItemLanguage);
    }

    [TestMethod]
    [DataRow("DE", "German")]
    [DataRow("JA", "Japanese")]
    [DataRow("ZH", "Chinese")]
    [DataRow("RU", "Russian")]
    [DataRow("xx", "XX")]
    public void ItemLanguageOptionMapsCodeToDisplayName(string code, string expectedName)
    {
        var language = new ItemLanguageOption(code);

        Assert.AreEqual(code.ToUpperInvariant(), language.Code);
        Assert.AreEqual(expectedName, language.DisplayName);
        Assert.AreEqual(expectedName, language.ToString());
    }
}
