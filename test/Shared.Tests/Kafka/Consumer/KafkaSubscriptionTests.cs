using System.Threading;
using Moq;
using NUnit.Framework;

namespace Shared.Tests.Kafka.Consumer
{
    [TestFixture]
    public class KafkaSubscriptionTests
    {
        [TestFixture]
        public class Topic
        {
            [Test]
            public void ReturnsTopicFromSource()
            {
                // Arrange
                var sourceMock = new Mock<IKafkaSubscriptionSource<long, string>>(MockBehavior.Strict);
                sourceMock.SetupGet(e => e.Topic).Returns("Some topic");

                var sut = new KafkaSubscription<long, string>(sourceMock.Object);

                // Act
                var actual = sut.Topic;

                // Assert
                Assert.That(actual, Is.EqualTo("Some topic"));
            }
        }

        [TestFixture]
        public class HandleMessage
        {
            [Test]
            public void HandleMessageShouldInvokeMessageReceived()
            {
                // Arrange
                var sourceMock = new Mock<IKafkaSubscriptionSource<long, string>>(MockBehavior.Strict);
                var sut = new KafkaSubscription<long, string>(sourceMock.Object);
                var manualResetEvent = new ManualResetEvent(false);

                long actualKey = -1; 
                string actualValue = null;

                sut.MessageReceived += (key, value) =>
                {
                    actualKey = key;
                    actualValue = value;
                    manualResetEvent.Set();
                };

                // Act
                sut.HandleMessage(10, "Some value");

                // Assert
                manualResetEvent.WaitOne();
                Assert.Multiple(() =>
                {
                    Assert.That(actualKey, Is.EqualTo(10));
                    Assert.That(actualValue, Is.EqualTo("Some value"));
                });
            }
        }
    }
}
