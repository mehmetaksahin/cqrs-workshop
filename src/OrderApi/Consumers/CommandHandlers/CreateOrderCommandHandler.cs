﻿using MassTransit;
using OrderApi.Domain.Commands;
using OrderApi.Domain.Events;
using OrderApi.Data.Entities;
using OrderApi.Data.Repositories.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderApi.Consumers.CommandHandlers
{
    public class CreateOrderCommandHandler : IConsumer<CreateOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public CreateOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Consume(ConsumeContext<CreateOrderCommand> context)
        {
            var order = new Order()
            {
                Id = context.Message.Id,
                OrderCode = context.Message.OrderCode,
                OrderDate = context.Message.OrderDate,
                UserId = context.Message.UserId,
                TotalPrice = context.Message.TotalPrice,
                Status = "Created"
            };

            await _orderRepository.Create(order);

            //publish event
            var orderCreatedEvent = new OrderCreatedEvent()
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                OrderDate = order.OrderDate,
                UserId = order.UserId,
                TotalPrice = order.TotalPrice,
                Status = order.Status
            };

            await context.Publish(orderCreatedEvent);
        }
    }
}