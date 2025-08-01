﻿namespace CoffeeChess.Domain.Shared.Abstractions;

public abstract class AggregateRoot<TDomainEvent>
{
    public IReadOnlyCollection<TDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private readonly List<TDomainEvent> _domainEvents = [];

    public void ClearDomainEvents() => _domainEvents.Clear();
    
    public void AddDomainEvent(TDomainEvent @event) => _domainEvents.Add(@event);
}