using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Shop.Core.Extensions;
using Shop.Core.SharedKernel;
using Shop.Domain.Entities.CustomerAggregate.Events;
using Shop.Query.Abstractions;
using Shop.Query.Application.Customer.Queries;
using Shop.Query.QueriesModel;

namespace Shop.Query.EventHandlers;

public class CustomerEventHandler(
    IMapper mapper,
    ISynchronizeDb synchronizeDb,
    ICacheService cacheService,
    ILogger<CustomerEventHandler> logger) :
    INotificationHandler<CustomerCreatedEvent>,
    INotificationHandler<CustomerUpdatedEvent>,
    INotificationHandler<CustomerDeletedEvent>
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly ILogger<CustomerEventHandler> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly ISynchronizeDb _synchronizeDb = synchronizeDb;

    public async Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        var customerQueryModel = _mapper.Map<CustomerQueryModel>(notification);
        await _synchronizeDb.UpsertAsync(customerQueryModel, filter => filter.Id == customerQueryModel.Id);
        await ClearCacheAsync(notification);
    }

    public async Task Handle(CustomerDeletedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        await _synchronizeDb.DeleteAsync<CustomerQueryModel>(filter => filter.Email == notification.Email);
        await ClearCacheAsync(notification);
    }

    public async Task Handle(CustomerUpdatedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        var customerQueryModel = _mapper.Map<CustomerQueryModel>(notification);
        await _synchronizeDb.UpsertAsync(customerQueryModel, filter => filter.Id == customerQueryModel.Id);
        await ClearCacheAsync(notification);
    }

    private async Task ClearCacheAsync(CustomerBaseEvent @event)
    {
        var cacheKeys = new[] { nameof(GetAllCustomerQuery), $"{nameof(GetCustomerByIdQuery)}_{@event.Id}" };
        await _cacheService.RemoveAsync(cacheKeys);
    }

    private void LogEvent<TEvent>(TEvent @event) where TEvent : CustomerBaseEvent =>
        _logger.LogInformation("----- Triggering the event {EventName}, model: {EventModel}", typeof(TEvent).Name, @event.ToJson());
}