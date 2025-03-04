# AwsKickStarter.Lambda

This library provides base classes and interfaces for creating AWS Lambdas with minimal boilerplate code and support for dependency injection.

Each base class has an associated handler interface, the choice of base class will depend on the use case.

| Use case | Base class | Handler interface |
|----------|------------|-------------------|
| Simple Lambda with no input or output | `Lambda` | `ILambdaHandler` |
| Simple Lambda with input | `LambdaIn<TInput>` | `ILambdaInHandler<TInput>` |
| Simple Lambda with output | `LambdaOut<TOutput>` | `ILambdaOutHandler<TOutput>` |
| Simple Lambda with input and output | `LambdaInOut<TInput, TOutput>` | `ILambdaInOutHandler<TInput, TOutput>` |
| Lambda with S3 source. | `S3Lambda` | `IS3LambdaHandler` |
| Lambda with SNS source. | `SnsLambda` | `ISnsLambdaHandler` |
| Lambda with SNS source deserialized as typed message | `SnsLambda<TMessage>` | `ISnsLambdaHandler<TMessage>` |
| Lambda with SQS source. | `SqsLambda` | `ISqsLambdaHandler` |
| Lambda with SQS source deserialized as typed message | `SqsLambda<TMessage>` | `ISqsLambdaHandler<TMessage>` |
| Lambda with SQS source and SQSBatchResponse | `SqsBatchResponseLambda` | `ISqsBatchResponseLambdaHandler` |
| Lambda with SQS source deserialized as typed message and SQSBatchResponse | `SqsBatchResponseLambda<TMessage>` | `ISqsBatchResponseLambdaHandler<TMessage>` |


All of the handler interfaces define a single `Handle` method with the appropriate parameters and return types.
For example `ISqsBatchResponseLambdaHandler<TMessage>` defines `Task<bool> Handle(TMessage message)`.

The `IS3LambdaHandler` handler will receive a collection of S3 events that may be processed in bulk.
The `ISnsLambdaHandler` handler will receive a collection of notifications that may be processed in bulk.
The `ISqsLambdaHandler` handler will receive a collection of messages that may be processed in bulk. If an unhandled exception is thrown all messages will remain on the queue to be retried.
The `ISqsBatchResponseLambdaHandler` handler will receive multiple calls each with a single message. If the handler returns false, or an unhandled exception is thrown, the single failed message will remain on the queue to be retried.
When implementing a batch response lambda the event source should be configured with `ReportBatchItemFailures`. See [AWS best practices](https://docs.aws.amazon.com/prescriptive-guidance/latest/lambda-event-filtering-partial-batch-responses-for-sqs/best-practices-partial-batch-responses.html).

The SNS and SQS lambda support automatic gzip decompression of messages when the message attributes include `Content-Encoding` set to `gzip`.

## Creating a Lambda

To create a lambda you first create an empty class that derives from a suitable base class.

```csharp
namespace MyLambdaNamespace;
public class MyLambda : SqsBatchResponseLambda<MyMessage> { }
```

This will expect an implementation of the matching handler interface to be in the same assembly as the lambda class. There must only be a single class implementing the matching handler interface in the same assembly.

```csharp
namespace MyLambdaNamespace;
public class MyMessageHandler : ISqsBatchResponseLambdaHandler<MyMessage>
{
    public Task<bool> Handle(MyMessage message)
    {
        ...
    }
}
```

## Handler

When creating the lambda in AWS the handler will be the `Handler` method provided by the base class. For example
```
MyProject::MyLambdaNamespace.MyLambda::Handler
```

## Dependency Injection

Adding services for dependency injection is easy, simply create a class that implements the `IServiceConfiguration` interface and register your services there. These will be available for use in the handler class. For example

```csharp
public class MyLambdaServiceConfiguration : IServiceConfiguration
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => services.AddTransient<IMyService, MyService>();
}
```


## Configuration

Custom configuration is implemented using `appsettings.json` & `appsettings.<Environment>.json` file(s) and/or environment variables.

## Logging

Logging is implemented using Serilog with the choice of output sinks done in the appsettings.json file. This would allow you to log to your preferred target such as Seq. For example

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console" ],
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" }
    ],
    "Enrich": [ "FromLogContext" ],
    "Properties": {
      "Application": "My Lambda"
    }
  }
}
```

Additional logging configuration is possible by overriding the `ConfigureLogging` method on the lambda class. For example

```csharp
public class MyLambda : SqsLambda<MyInput>
{
    public override void ConfigureLogging(LoggerConfiguration loggerConfiguration)
        => loggerConfiguration.Enrich.WithProperty("MyProperty", "MyValue");
}
```


# Demos

Running the demonstrations below requires the following:

* Docker
* [AWS CLI](https://awscli.amazonaws.com/AWSCLIV2.msi)
* [AWS profile named `localstack`](https://docs.localstack.cloud/user-guide/integrations/aws-cli/#configuring-a-custom-profile)
* [Powershell 7](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell)
* [dotnet SDK >= 8](https://get.dot.net)
* AWS dotnet lambda tools `dotnet tool install -g Amazon.Lambda.Tools`

## Simple

```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace Simple.Lambda
```
This will invoke the lambda without waiting for completion.

```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace Simple.LambdaIn
```
This will invoke the lambda passing a payload without waiting for completion.

```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace Simple.LambdaOut
```
This will invoke the lambda waiting for completion and display the output.

```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace Simple.LambdaInOut
```
This will invoke the lambda passing a payload waiting for completion and display the output.

## S3

```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace S3
```
This will receive an event that an S3 file has been created.

Watch the lambda container log to see the activity.

## SNS

```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace Sns.Lambda
```
This will receive five notifications from the topic.
One of the notifications will fail, when this happens only the failed message will be retried after one minute.
After three retries the single failed messages is discarded.

Watch the lambda container log to see the activity.

LambdaT will show the same behaviour.
```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace Sns.LambdaT
```
## Basic SQS

```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace Sqs.Basic
```
This will receive a batch of five messages from the queue, two of which were sent directly to the queue and three published to SNS.
One of the messages will fail, when this happens all messages are left on the queue for all messages to be retried after one minute.
After three retries all five messages are moved to the dead letter queue.

Watch the lambda container log to see the activity.

To check the number of messages on the dead letter queue use this command
```pwsh
./Show-LambdaQueueCounts.ps1
```

BasicT will show the same behaviour.
```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace Sqs.BasicT
```

## Batch Response SQS

```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace Sqs.BatchResponse
```
This will receive a batch of five messages from the queue, two of which were sent directly to the queue and three published to SNS.
One of the messages will fail, when this happens only the failed message is left on the queue to be retried after one minute.
After three retries the single failed messages is moved to the dead letter queue.

Watch the lambda container log to see the activity.

To check the number of messages on the dead letter queue use this command
```pwsh
./Show-LambdaQueueCounts.ps1
```

BatchResponseT will show the same behaviour.
```pwsh
./Run-LambdaDemo.ps1 -LambdaNamespace Sqs.BatchResponseT
```
