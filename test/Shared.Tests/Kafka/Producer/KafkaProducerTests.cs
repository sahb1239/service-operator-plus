using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Tests
{
    [TestFixture]
    public class KafkaProducerTests
    {
        [TestFixture]
        public class ProduceAsync
        {
            private Mock<Confluent.Kafka.IProducer<long, string>> kafkaProducer = 
                new Mock<Confluent.Kafka.IProducer<long, string>>(MockBehavior.Strict);

            [Test]
            public async Task ShouldCallProduceAsync()
            {
                // Arrange
                var topic = "some-topic";
                var key = 10L;
                var value = "some-value";
                var sut = new KafkaProducer<long, string>(kafkaProducer.Object);

                kafkaProducer.Setup(e => e.ProduceAsync(It.IsAny<string>(),
                        It.IsAny<Confluent.Kafka.Message<long, string>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        (string topic, Confluent.Kafka.Message<long, string> message, CancellationToken token) =>
                            new Confluent.Kafka.DeliveryResult<long, string>
                            {
                                Message = new Confluent.Kafka.Message<long, string>
                                {
                                    Key = key,
                                    Value = value,
                                }
                            });

                // Act
                await sut.ProduceAsync(topic, value);

                // Assert
                kafkaProducer.Verify(
                    e => e.ProduceAsync(topic, It.Is<Confluent.Kafka.Message<long, string>>(m => m.Value == value),
                        It.IsAny<CancellationToken>()), Times.Once);
                kafkaProducer.VerifyNoOtherCalls();
            }

            [Test]
            public async Task ShouldReturnKey()
            {
                // Arrange
                var topic = "some-topic";
                var key = 10L;
                var value = "some-value";
                var sut = new KafkaProducer<long, string>(kafkaProducer.Object);

                kafkaProducer.Setup(e => e.ProduceAsync(It.IsAny<string>(), It.IsAny<Confluent.Kafka.Message<long, string>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((string topic, Confluent.Kafka.Message<long, string> message, CancellationToken token) =>
                    new Confluent.Kafka.DeliveryResult<long, string>
                    {
                        Message = new Confluent.Kafka.Message<long, string>
                        {
                            Key = key,
                            Value = value,
                        }
                    });

                // Act
                var actual = await sut.ProduceAsync(topic, value);

                // Assert
                Assert.That(actual, Is.EqualTo(key));
            }
        }
    }
}
