using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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
}
