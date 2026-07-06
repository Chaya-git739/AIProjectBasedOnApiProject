using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BffService.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace BffService.Tests;

public class AggregationServiceTests
{
    [Fact]
    public async Task GetOrderDetails_InvalidOrderId_ReturnsBadRequest()
    {
        var orderClient = new Mock<IOrderClient>();
        var productClient = new Mock<IProductClient>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AggregationService>>();

        var svc = new AggregationService(orderClient.Object, productClient.Object, logger.Object);
        var res = await svc.GetOrderDetailsAsync("abc", "corr-invalid");

        res.Match(
            _ => throw new Xunit.Sdk.XunitException("expected error"),
            err =>
            {
                err.StatusCode.Should().Be(400);
                err.Body.ErrorCode.Should().Be("INVALID_INPUT");
                err.Body.CorrelationId.Should().Be("corr-invalid");
                return true;
            }
        );
    }

    [Fact]
    public async Task GetOrderDetails_HappyPath_ReturnsDto()
    {
        var orderClient = new Mock<IOrderClient>();
        var productClient = new Mock<IProductClient>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AggregationService>>();

        var orderDto = new OrderServiceContract.OrderDto
        {
            Id = 1,
            UserId = 10,
            OrderItems = new List<OrderServiceContract.OrderItemDto>
            {
                new() { GiftId = 5, Quantity = 2 }
            }
        };

        orderClient
            .Setup(x => x.GetOrderAsync("1", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpClientResult<OrderServiceContract.OrderDto>(orderDto, true, 200));

        productClient
            .Setup(x => x.GetGiftAsync(5, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpClientResult<CatalogServiceContract.GiftDto>(
                new CatalogServiceContract.GiftDto { Id = 5, Name = "Gift", TicketPrice = 12.5m },
                true,
                200));

        var svc = new AggregationService(orderClient.Object, productClient.Object, logger.Object);
        var res = await svc.GetOrderDetailsAsync("1", "corr-1");

        res.Match(
            dto =>
            {
                dto.Should().NotBeNull();
                dto.Meta.CorrelationId.Should().Be("corr-1");
                dto.Purchaser.UserId.Should().Be(10);
                dto.Total.Should().Be(27.5m);
                return true;
            },
            err => throw new Xunit.Sdk.XunitException("expected success")
        );
    }

    [Fact]
    public async Task GetOrderDetails_OrderNotFound_ReturnsNotFound()
    {
        var orderClient = new Mock<IOrderClient>();
        var productClient = new Mock<IProductClient>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AggregationService>>();

        orderClient
            .Setup(x => x.GetOrderAsync("55", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpClientResult<OrderServiceContract.OrderDto>(null, false, 404, "not found"));

        var svc = new AggregationService(orderClient.Object, productClient.Object, logger.Object);
        var res = await svc.GetOrderDetailsAsync("55", "corr-404");

        res.Match(
            _ => throw new Xunit.Sdk.XunitException("expected error"),
            err =>
            {
                err.StatusCode.Should().Be(404);
                err.Body.ErrorCode.Should().Be("NOT_FOUND");
                return true;
            }
        );
    }

    [Fact]
    public async Task GetOrderDetails_OrderTimeout_ReturnsGatewayTimeout()
    {
        var orderClient = new Mock<IOrderClient>();
        var productClient = new Mock<IProductClient>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AggregationService>>();

        orderClient
            .Setup(x => x.GetOrderAsync("77", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpClientResult<OrderServiceContract.OrderDto>(null, false, 408, "timeout"));

        var svc = new AggregationService(orderClient.Object, productClient.Object, logger.Object);
        var res = await svc.GetOrderDetailsAsync("77", "corr-504");

        res.Match(
            _ => throw new Xunit.Sdk.XunitException("expected error"),
            err =>
            {
                err.StatusCode.Should().Be(504);
                err.Body.ErrorCode.Should().Be("UPSTREAM_TIMEOUT");
                return true;
            }
        );
    }

    [Fact]
    public async Task GetOrderDetails_ProductFailure_ReturnsFailedDependency()
    {
        var orderClient = new Mock<IOrderClient>();
        var productClient = new Mock<IProductClient>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AggregationService>>();

        var orderDto = new OrderServiceContract.OrderDto
        {
            Id = 2,
            UserId = 11,
            OrderItems = new List<OrderServiceContract.OrderItemDto> { new() { GiftId = 9, Quantity = 1 } }
        };

        orderClient
            .Setup(x => x.GetOrderAsync("2", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpClientResult<OrderServiceContract.OrderDto>(orderDto, true, 200));

        productClient
            .Setup(x => x.GetGiftAsync(9, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpClientResult<CatalogServiceContract.GiftDto>(null, false, 500, "catalog down"));

        var svc = new AggregationService(orderClient.Object, productClient.Object, logger.Object);
        var res = await svc.GetOrderDetailsAsync("2", "corr-2");

        res.Match(
            _ => throw new Xunit.Sdk.XunitException("expected error"),
            err =>
            {
                err.StatusCode.Should().Be(424);
                err.Body.ErrorCode.Should().Be("DEPENDENCY_FAILED");
                return true;
            }
        );
    }

    [Fact]
    public async Task GetOrderDetails_ProductTimeout_ReturnsGatewayTimeout()
    {
        var orderClient = new Mock<IOrderClient>();
        var productClient = new Mock<IProductClient>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AggregationService>>();

        var orderDto = new OrderServiceContract.OrderDto
        {
            Id = 3,
            UserId = 12,
            OrderItems = new List<OrderServiceContract.OrderItemDto> { new() { GiftId = 15, Quantity = 1 } }
        };

        orderClient
            .Setup(x => x.GetOrderAsync("3", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpClientResult<OrderServiceContract.OrderDto>(orderDto, true, 200));

        productClient
            .Setup(x => x.GetGiftAsync(15, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpClientResult<CatalogServiceContract.GiftDto>(null, false, 408, "timeout"));

        var svc = new AggregationService(orderClient.Object, productClient.Object, logger.Object);
        var res = await svc.GetOrderDetailsAsync("3", "corr-prod-timeout");

        res.Match(
            _ => throw new Xunit.Sdk.XunitException("expected error"),
            err =>
            {
                err.StatusCode.Should().Be(504);
                err.Body.ErrorCode.Should().Be("UPSTREAM_TIMEOUT");
                return true;
            }
        );
    }
}
