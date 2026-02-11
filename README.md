# Goodtocode.Mediator

[![NuGet CI/CD](https://github.com/goodtocode/aspect-mediator/actions/workflows/gtc-mediator-nuget.yml/badge.svg)](https://github.com/goodtocode/aspect-mediator/actions/workflows/gtc-mediator-nuget.yml)

Goodtocode.Mediator is a lightweight implementation of the mediator pattern for .NET, designed to enable CQRS (Command Query Responsibility Segregation) in clean architecture solutions. It provides a simple way to decouple request handling, validation, and business logic without external dependencies.

### Core Abstractions
- **IRequest<TResponse> / IRequest**: Marker interfaces for queries and commands, with or without a return type.
- **IRequestHandler<TRequest, TResponse> / IRequestHandler<TRequest>**: Handlers that process requests and return a result (or not).
- **ISender**: The main interface for sending requests; exposes `Send` methods for both command and query patterns.
- **IRequestDispatcher**: Internal dispatcher that resolves handlers and pipeline behaviors from DI, and invokes them.
- **IPipelineBehavior<TRequest, TResponse> / IPipelineBehavior<TRequest>**: Optional middleware for cross-cutting concerns (validation, logging, etc.), chained before the handler.

### Pipeline Behaviors
Pipeline behaviors allow you to add cross-cutting logic around request handling, such as validation, logging, or transaction management. Behaviors are resolved from DI and executed in order before the handler.

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestDelegateInvoker<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Handling {typeof(TRequest).Name}");
        var response = await next();
        Console.WriteLine($"Handled {typeof(TRequest).Name}");
        return response;
    }
}
```

### How It Works
1. Define a request (command or query) implementing `IRequest<TResponse>` or `IRequest`.
2. Implement a handler for the request, containing the logic in `.Handle`.
3. Optionally, implement pipeline behaviors for validation, logging, etc.
4. Register handlers and behaviors in DI.
5. Use `ISender.Send(request)` to dispatch from controllers or services.

### Example: Aggregator Controller Using Goodtocode.Mediator
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
public class EnrollmentController : ApiControllerBase
{
    // Query: Get details
    [HttpGet("{enrollmentId}")]
    public async Task<EnrollmentDto> GetEnrollment(Guid enrollmentId)
    {
        return await Mediator.Send(new GetMyEnrollmentQuery { Id = enrollmentId });
    }

    // Command: Create
    [HttpPost]
    public async Task<ActionResult> Post(EnrollCommand command)
    {
        var response = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetMyEnrollment), new { enrollmentId = response.Id }, response);
    }

    // Command: Patch
    [HttpPatch("{enrollmentId}")]
    public async Task<ActionResult> Patch(Guid enrollmentId, UnenrollCommand command)
    {
        command.Id = enrollmentId;
        await Mediator.Send(command);
        return NoContent();
    }

    // Command: Delete
    [HttpDelete]
    public async Task<IActionResult> Delete(Guid enrollmentId)
    {
        await Mediator.Send(new DeleteEnrollmentCommand { Id = enrollmentId });
        return NoContent();
    }
}
```

### Benefits
- Decouples controllers from business logic and validation
- Supports CQRS by separating commands and queries
- Enables clean, testable, and maintainable code

## Features
- Simple integration with Blazor, ASP.NET Core, and .NET DI

## Installation
Install via NuGet:

```
dotnet add package Goodtocode.Mediator
```

## License
MIT


## Contact
- [GitHub Repo](https://github.com/goodtocode/aspect-httpclient)
- [@goodtocode](https://twitter.com/goodtocode)

## Version History

| Version | Date       | Changes                | .NET Version |
| ------- | ---------- | ---------------------- | ------------ |
| 1.0.0   | 2025-01-01 | Initial release        | 9            |
| 1.1.0   | 2026-01-22 | Version bump           | 10           |
