namespace AwsKickStarter.Lambda.Demo.Services;

public interface IMyService
{
    Task Process(MyInput input);
    Task Process(string input);
}
