using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfTeslaCamViewer.UnitTests;

[TestClass]
public class TeslaCamCameraConverterTests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    public void ReadJson_ValidTeslaCamReason_ReturnsCorrectReason(long testCamera)
    {
        // Arrange
        var testJson =
            $"{{\"timestamp\":\"2021-11-18T12:09:26\",\"city\":\"Somewhere\",\"est_lat\":\"11.3493\",\"est_lon\":\"142.1821\",\"reason\":\"\",\"camera\":\"{testCamera}\"}}";

        // Act
        var result = testJson.Deserialize<TeslaCamEventInfo>();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(TeslaCamCamera.Unknown, result.Camera, "Camera should not be Unknown");
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    [DataRow(long.MinValue)]
    [DataRow(long.MaxValue)]
    public void ReadJson_InvalidTeslaCamReason_ReturnsUnknown(long testCamera)
    {
        // Arrange
        var testJson =
            $"{{\"timestamp\":\"2021-11-18T12:09:26\",\"city\":\"Somewhere\",\"est_lat\":\"11.3493\",\"est_lon\":\"142.1821\",\"reason\":\"\",\"camera\":\"{testCamera}\"}}";

        // Act
        var result = testJson.Deserialize<TeslaCamEventInfo>();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(TeslaCamCamera.Unknown, result.Camera, "Camera should be Unknown");
    }
}