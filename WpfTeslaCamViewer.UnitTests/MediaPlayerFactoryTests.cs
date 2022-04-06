using LibVLCSharp.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WpfTeslaCamViewer.Factories;

namespace WpfTeslaCamViewer.UnitTests
{
    [TestClass]
    public class MediaPlayerFactoryTests
    {
        private Mock<ILibVlcFactory> mockVlcFactory;

        public MediaPlayerFactoryTests()
        {
            Core.Initialize();
        }

        [TestInitialize]
        public void Initialize()
        {
            mockVlcFactory = new Mock<ILibVlcFactory>();
            var mockVlc = new Mock<LibVLC>().Object;
            mockVlcFactory.Setup(x => x.GetLibVlcInstance()).Returns(mockVlc);
        }

        [TestMethod]
        public void GetNewMediaPlayer_ReturnsNewMediaPlayer()
        {
            // Arrange
            var mediaPlayerFactory = GetMediaPlayerFactory();

            // Act
            var result = mediaPlayerFactory.GetNewMediaPlayer();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(MediaPlayer));
        }

        private MediaPlayerFactory GetMediaPlayerFactory() => new(mockVlcFactory.Object);
    }
}
